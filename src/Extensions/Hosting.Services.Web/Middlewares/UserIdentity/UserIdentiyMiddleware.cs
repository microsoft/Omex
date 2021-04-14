// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// Adds user information if needed
	/// </summary>
	internal class UserHashIdentityMiddleware : IMiddleware, IDisposable
	{
		private const int HashSize = 32; // sha256 hash size, from here https://github.com/dotnet/runtime/blob/26a71f95b708721065f974fd43ba82a1dcb3e8f0/src/libraries/System.Security.Cryptography.Algorithms/src/Internal/Cryptography/HashProviderDispenser.Windows.cs#L85
		private readonly IUserIdentityProvider[] m_userIdentityProviders;
		private readonly ISaltProvider m_saltProvider;
		private readonly int m_maxIdentitySize;

		public UserHashIdentityMiddleware(IEnumerable<IUserIdentityProvider> userIdentityProviders, ISaltProvider saltProvider)
		{
			m_userIdentityProviders = userIdentityProviders.ToArray();
			m_saltProvider = saltProvider;
			m_maxIdentitySize = m_userIdentityProviders.Max(p => p.MaxBytesInIdentity);
		}

		async Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
		{
			Activity? activity = Activity.Current;

			if (activity != null && string.IsNullOrEmpty(activity.GetUserHash()))
			{
				string userHash = await CreateUserHashAsync(context).ConfigureAwait(false);
				if (!string.IsNullOrWhiteSpace(userHash))
				{
					activity.SetUserHash(userHash);
				}
			}

			await next(context);
		}

		internal async Task<string> CreateUserHashAsync(HttpContext context)
		{
			using IMemoryOwner<byte> uidMemoryOwner = MemoryPool<byte>.Shared.Rent(m_maxIdentitySize + m_saltProvider.GetSalt().Length);

			// Done because span cannot be declared in an async function
			uidMemoryOwner.Memory.Span.Fill(0);

			int identityBytesWritten = -1;
			foreach (IUserIdentityProvider provider in m_userIdentityProviders)
			{
				if (await provider.TryWriteBytesAsync(context, uidMemoryOwner.Memory.Span.Slice(0, provider.MaxBytesInIdentity), out identityBytesWritten)
					.ConfigureAwait(false))
				{
					break;
				}
			}

			return GetHashString(m_saltProvider.GetSalt(), uidMemoryOwner.Memory.Span, identityBytesWritten);
		}

		/// <summary>
		/// This method was made because span byte cannot be declared in async or lambda functions
		/// </summary>
		private string GetHashString(ReadOnlySpan<byte> saltSpan, Span<byte> uidSpan, int identityBytesWritten)
		{
			if (identityBytesWritten <= 0)
			{
				return string.Empty;
			}

			saltSpan.CopyTo(uidSpan.Slice(identityBytesWritten));

			using IMemoryOwner<byte> hashMemoryOwner = MemoryPool<byte>.Shared.Rent(HashSize);
			Span<byte> hashSpan = hashMemoryOwner.Memory.Span;

#if NETCOREAPP3_1
			using HashAlgorithm hashAlgorithm = new SHA256Managed(); // need to have new instance each time since its not thread-safe

			if (!hashAlgorithm.TryComputeHash(uidSpan, hashSpan, out _))
			{
				return string.Empty;
			}

			return BitConverter.ToString(hashSpan.ToArray()).Replace("-", "");
#else
			SHA256.HashData(uidSpan, hashSpan);

			return Convert.ToHexString(hashSpan);
#endif
		}

		public void Dispose() => m_saltProvider.Dispose();
	}
}
