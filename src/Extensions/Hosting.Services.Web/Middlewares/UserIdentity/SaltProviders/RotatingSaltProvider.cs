// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Security.Cryptography;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <remarks>
	/// Provider would change salt after 40 hours to make sure that hash could not be traced back to user.
	/// Salt would be different in each instance so different replicas of the same service would create non-identical hashes for the same user.
	/// </remarks>
	internal class RotatingSaltProvider : ISaltProvider
	{
		private const int SaltLength = 48; // currently recommended salt size 32
		private const int HoursToKeepSalt = 40; // should not be more then 48 hours
		private readonly RandomNumberGenerator m_random;
		private readonly IMemoryOwner<byte> m_currentSaltMemory;
		private readonly ISystemClock m_systemClock;
		private readonly ILogger<RotatingSaltProvider> m_logger;
		private DateTimeOffset m_saltGenerationTime;

		public RotatingSaltProvider(ISystemClock systemClock, ILogger<RotatingSaltProvider> logger)
		{
			m_random = new RNGCryptoServiceProvider();
			m_currentSaltMemory = MemoryPool<byte>.Shared.Rent(SaltLength);
			m_saltGenerationTime = DateTime.MinValue;
			m_systemClock = systemClock;
			m_logger = logger;
		}

		public ReadOnlySpan<byte> GetSalt()
		{
			DateTimeOffset currentTime = m_systemClock.UtcNow;
			Span<byte> saltSpan = m_currentSaltMemory.Memory.Span;

			if ((currentTime - m_saltGenerationTime).TotalHours > HoursToKeepSalt)
			{
				m_random.GetNonZeroBytes(saltSpan);
				m_saltGenerationTime = currentTime;

				// DO NOT ADD THE SALT TO THIS LOG STATEMENT. Doing so may violate compliance guarantees.
				m_logger.LogInformation(Tag.Create(), "New salt generated for UserIdentityMiddelware");
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
