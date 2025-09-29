// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// Provides user identity to create user hash
	/// </summary>
	public interface IUserIdentityProvider
	{
		/// <summary>
		/// Max bytes that user identity could take
		/// </summary>
		int MaxBytesInIdentity { get; }

		/// <summary>
		/// Tries to extract user information for HttpContext to create user hash
		/// </summary>
		Task<(bool success, int bytesWritten)> TryWriteBytesAsync(HttpContext context, Memory<byte> memory);
	}
}
