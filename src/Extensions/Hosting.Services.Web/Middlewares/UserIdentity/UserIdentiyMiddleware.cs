// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// Adds user information if needed
	/// </summary>
	internal class UserIdentiyMiddleware : IMiddleware, IDisposable
	{
		private const int HashSize = 32; // sha256 hash size, from here https://github.com/dotnet/runtime/blob/26a71f95b708721065f974fd43ba82a1dcb3e8f0/src/libraries/System.Security.Cryptography.Algorithms/src/Internal/Cryptography/HashProviderDispenser.Windows.cs#L85
		private readonly IUserIdentityProvider m_userIdentityProvider;
		private readonly ISaltProvider m_saltProvider;
		private readonly HashAlgorithm m_hashAlgorithm;

		public UserIdentiyMiddleware(IUserIdentityProvider userIdentityProvider, ISaltProvider saltProvider)
		{
			m_userIdentityProvider = userIdentityProvider;
			m_saltProvider = saltProvider;
			m_hashAlgorithm = new SHA256Managed();
		}

		Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
		{
			Activity? activity = Activity.Current;

			if (activity != null && string.IsNullOrEmpty(activity.GetUserHash()))
			{
				string userHash = CreateUserHash(context);
				if (!string.IsNullOrWhiteSpace(userHash))
				{
					activity.SetUserHash(userHash);
				}
			}

			return next(context);
		}

		internal string CreateUserHash(HttpContext context)
		{
			int identitySize = m_userIdentityProvider.MaxBytesInIdentity;
			ReadOnlySpan<byte> saltSpan = m_saltProvider.GetSalt();

			using IMemoryOwner<byte> uidMemoryOwner = MemoryPool<byte>.Shared.Rent(identitySize + saltSpan.Length);
			Span<byte> uidSpan = uidMemoryOwner.Memory.Span;
			if (!m_userIdentityProvider.TryWriteBytes(context, uidSpan.Slice(0, identitySize), out int identityBytesWritten))
			{
				return string.Empty;
			}

			saltSpan.CopyTo(uidSpan.Slice(identityBytesWritten));

			using IMemoryOwner<byte> hashMemoryOwner = MemoryPool<byte>.Shared.Rent(HashSize);
			Span<byte> hashSpan = hashMemoryOwner.Memory.Span;
			if (!m_hashAlgorithm.TryComputeHash(uidSpan, hashSpan, out int hashBytesWritten))
			{
				return string.Empty;
			}

#if NETCOREAPP3_1
			return BitConverter.ToString(hashSpan.ToArray()).Replace("-", "");
#else
			return Convert.ToHexString(hashSpan);
#endif
		}

		public void Dispose()
		{
			m_hashAlgorithm.Dispose();
			m_saltProvider.Dispose();
		}
	}
}
