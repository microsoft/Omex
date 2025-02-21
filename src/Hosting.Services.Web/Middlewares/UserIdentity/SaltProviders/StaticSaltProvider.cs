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
	/// Consumer provides the salt string, this provider then writes the salt to memory
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
