﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
		private readonly HashAlgorithm m_hashAlgorithm;
		private readonly int m_maxIdentitySize;

		public UserHashIdentityMiddleware(IEnumerable<IUserIdentityProvider> userIdentityProviders, ISaltProvider saltProvider)
		{
			m_userIdentityProviders = userIdentityProviders.ToArray();
			m_saltProvider = saltProvider;
			m_hashAlgorithm = new SHA256Managed();
			m_maxIdentitySize = m_userIdentityProviders.Max(p => p.MaxBytesInIdentity);
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
			ReadOnlySpan<byte> saltSpan = m_saltProvider.GetSalt();

			using IMemoryOwner<byte> uidMemoryOwner = MemoryPool<byte>.Shared.Rent(m_maxIdentitySize + saltSpan.Length);
			Span<byte> uidSpan = uidMemoryOwner.Memory.Span;

			int identityBytesWritten = -1;
			Span<byte> identitySpan = uidSpan.Slice(0, m_maxIdentitySize);
			foreach (IUserIdentityProvider provider in m_userIdentityProviders)
			{
				if (provider.TryWriteBytes(context, identitySpan, out identityBytesWritten))
				{
					break;
				}
			}

			if (identityBytesWritten <= 0)
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
