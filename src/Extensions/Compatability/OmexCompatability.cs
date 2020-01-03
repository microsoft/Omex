// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Compatability.Logger;
using Microsoft.Omex.Extensions.Compatability.Validation;

namespace Microsoft.Omex.Extensions.Compatability
{
	/// <summary>Class to register dependencies for Compatability classes</summary>
	public static class OmexCompatability
	{
		/// <summary>Initialize depriacated static classes lile Code and ULSLogger</summary>
		public static IServiceProvider InitializeOmexCompatabilityClasses(this IServiceProvider serviceProvider)
		{
			ILoggerFactory factory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
#pragma warning disable CS0618 // Supress warning since classes should be initialized for proper usage
			Code.Initialize(factory);
			ULSLogging.Initialize(factory);
#pragma warning restore CS0618
			return serviceProvider;
		}
	}
}
