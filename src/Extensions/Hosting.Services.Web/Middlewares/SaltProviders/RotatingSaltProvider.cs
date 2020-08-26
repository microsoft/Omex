// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Security.Cryptography;
using Microsoft.Extensions.Internal;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	internal class RotatingSaltProvider : ISaltProvider
	{
		private const int SaltLength = 48; // currently recomended salt size 32
		private const int HoursToKeepSalt = 40; // should not be more then 48 hours
		private readonly RandomNumberGenerator m_random;
		private readonly IMemoryOwner<byte> m_currentSaltMemory;
		private readonly ISystemClock m_systemClock;
		private DateTimeOffset m_saltGenerationTime;

		public RotatingSaltProvider(ISystemClock systemClock)
		{
			m_random = new RNGCryptoServiceProvider();
			m_currentSaltMemory = MemoryPool<byte>.Shared.Rent(SaltLength);
			m_saltGenerationTime = DateTime.MinValue;
			m_systemClock = systemClock;
		}

		public Span<byte> GetSalt()
		{
			DateTimeOffset currentTime = m_systemClock.UtcNow;
			Span<byte> saltSpan = m_currentSaltMemory.Memory.Span;

			if ((currentTime - m_saltGenerationTime).TotalHours > HoursToKeepSalt)
			{
				m_random.GetNonZeroBytes(saltSpan);
				m_saltGenerationTime = currentTime;
			}

			return saltSpan;
		}

		public void Dispose()
		{
			m_currentSaltMemory.Dispose();
			m_random.Dispose();
		}
	}
}
