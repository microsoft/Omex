// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	internal class EmptySaltProvider : ISaltProvider
	{
		public Span<byte> GetSalt() => Span<byte>.Empty;

		public void Dispose() { }
	}
}
