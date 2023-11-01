// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	internal class EmailBasedUserIdentityProvider : IUserIdentityProvider
	{
        private static int MaxBytesPerChar { get; } = 4; // UTF-8
		public int MaxBytesInIdentity { get; } = 256 * MaxBytesPerChar; // maximum email address length plus max bytes per char

        public class UserEmail
        {
            public string Email { get; set; } = string.Empty;
        }

		public Task<(bool success, int bytesWritten)> TryWriteBytesAsync(HttpContext context, Memory<byte> memory)
		{
			int bytesWritten = -1;
            try{
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                TextReader reader = new StreamReader(context.Request.Body);
UserEmail? email = await JsonSerializer.DeserializeAsync<UserEmail>(reader);
UserEmail? email;
using (TextReader reader = new StreamReader(context.Request.Body))
{
                email = await JsonSerializer.DeserializeAsync<UserEmail>(reader);
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
                return Task.FromResult((success, bytesWritten));
            }
            catch {
                return Task.FromResult((false, -1));
            }
		}
	}
}
