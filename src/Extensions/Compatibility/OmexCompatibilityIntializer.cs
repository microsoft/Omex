﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Compatibility.Logger;
using Microsoft.Omex.Extensions.Compatibility.Validation;

namespace Microsoft.Omex.Extensions.Compatibility
{
	/// <summary>
	/// Class to initialize static dependencies from this project
	/// </summary>
	public static class OmexCompatibilityIntializer
	{
		/// <summary>
		/// Initialize compatibility classes with provided instances
		/// </summary>
		public static void Initialize(ILoggerFactory factory, ActivitySource scopeProvider)
		{
			TimedScope.Initialize(scopeProvider);
			Code.Initialize(factory);
			ULSLogging.Initialize(factory);
		}

		/// <summary>
		/// Initialize compatibility classes with simple implementation that might be used for logging
		/// </summary>
		public static void InitializeWithStubs() =>
			Initialize(new NullLoggerFactory(), new ActivitySource("StubActivitySource"));
	}
}
