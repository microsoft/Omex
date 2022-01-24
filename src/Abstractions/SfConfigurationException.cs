// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Exception of getting Service Fabric configuration parameters from environment variables
	/// </summary>
	public class SfConfigurationException : ArgumentException
	{
		private const string ErrorMessageFormat =
			"Failed to get or parse Service Fabric configuration value '{0}' from environment variables. "
			+ "Make sure service started by Service Fabric or set it with mock value for other cases.";

		internal SfConfigurationException(string variable)
			: base(string.Format(CultureInfo.InvariantCulture, ErrorMessageFormat, variable)) { }
	}
}
