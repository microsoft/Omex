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
			HttpRequest request = context.Request;
            TextReader reader = new StreamReader(request.Body);
            UserEmail? email = JsonSerializer.Deserialize<UserEmail>(reader.ReadToEnd());
            reader.Close();

            bool success = email != null;
            if (email != null)
            {
                bytesWritten = 0;
                foreach (char c in email.Email)
                    {
                        try {
                            success = success && BitConverter.TryWriteBytes(memory.Span.Slice(bytesWritten, MaxBytesPerChar), c);
                            bytesWritten += MaxBytesPerChar;
                        }
                        catch {
                            return Task.FromResult((false, -1));
                        }
                    }
            }
			return Task.FromResult((success, bytesWritten));
		}
	}
}
