// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Omex.Extensions.Logging;

internal class OmexLoggerOptionsSetup : ConfigureFromConfigurationOptions<OmexLoggingOptions>
{
	[Obsolete("OmexLoggerOptionsSetup is deprecated and pending for removal on 1 July 2024", DiagnosticId = "OMEX188")]
	public OmexLoggerOptionsSetup(ILoggerProviderConfiguration<OmexLoggerProvider> providerConfiguration)
		: base(providerConfiguration.Configuration)
	{
	}
}
