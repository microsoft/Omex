// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares.UserIdentity.Options;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <remarks>
	/// Provider would change salt after 40 hours to make sure that hash could not be traced back to user.
	/// Salt would be different in each instance so different replicas of the same service would create non-identical hashes for the same user.
	/// </remarks>
	internal class StaticSaltProvider : ISaltProvider
	{
		private readonly IMemoryOwner<byte> m_currentSaltMemory;
		private readonly byte[] m_saltValue;

		public StaticSaltProvider(IOptions<StaticSaltProviderOptions> options)
		{
			m_saltValue = Encoding.UTF8.GetBytes(options.Value.SaltValue);
			m_currentSaltMemory = MemoryPool<byte>.Shared.Rent(m_saltValue.Length);
		}

		public int MaxBytesInSalt => m_saltValue.Length;

		public ReadOnlySpan<byte> GetSalt()
		{
			Span<byte> saltSpan = m_currentSaltMemory.Memory.Span;
			m_saltValue.CopyTo(saltSpan);
			return saltSpan;
		}

		public void Dispose()
		{
			m_currentSaltMemory.Dispose();
		}
	}
}
