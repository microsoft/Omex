// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Compatibility
{
	internal class OmexCompatibilityInitializationException : Exception
	{
		internal OmexCompatibilityInitializationException()
			: base("Compatibility classes were not initialized. Please call AddOmexCompatibilityServices() on HostBuilder")
		{ }
	}
}
