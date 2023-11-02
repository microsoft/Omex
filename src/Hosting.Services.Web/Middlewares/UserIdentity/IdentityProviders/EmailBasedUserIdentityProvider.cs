// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	internal class EmailBasedUserIdentityProvider : IUserIdentityProvider
	{
		private const int MaxBytesPerChar = 4; // UTF-8
		private readonly ILogger<EmailBasedUserIdentityProvider> m_logger;
		public int MaxBytesInIdentity { get; } = 256 * MaxBytesPerChar; // maximum email address length plus max bytes per char

		private class UserEmail
		{
			public string Email { get; set; } = string.Empty;
		}

		public EmailBasedUserIdentityProvider(ILogger<EmailBasedUserIdentityProvider> logger)
		{
			m_logger = logger;
		}

		public async Task<(bool success, int bytesWritten)> TryWriteBytesAsync(HttpContext context, Memory<byte> memory)
		{
			int bytesWritten = -1;
			try
			{
				context.Request.Body.Seek(0, SeekOrigin.Begin);
				UserEmail? email;
				using (StreamReader reader = new(context.Request.Body))
				{
					email = await JsonSerializer.DeserializeAsync<UserEmail>(reader.BaseStream);
				}

				bool success = email != null;
				if (email != null)
				{
					bytesWritten = 0;
					foreach (char c in email.Email)
					{
						success = success && BitConverter.TryWriteBytes(memory.Span.Slice(bytesWritten, MaxBytesPerChar), c);
						bytesWritten += MaxBytesPerChar;
					}
				}
				else
				{
					m_logger.LogError(Tag.Create(), "Failed to read user email from request body");
				}

				if (!success)
				{
					m_logger.LogError(Tag.Create(), "Failed to write user email to memory");
				}

				return (success, bytesWritten);
			}
			catch
			{
				m_logger.LogError(Tag.Create(), "Encountered exception when deserializing user email from request body");
				return (false, -1);
			}
		}
	}
}
