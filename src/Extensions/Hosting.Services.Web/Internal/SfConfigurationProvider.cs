// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	internal class SfConfigurationProvider
	{
		internal const string PublishAddressEvnVariableName = "Fabric_NodeIPOrFQDN";

		internal const string EndpointPortEvnVariableSuffix = "Fabric_Endpoint_";

		private static string? GetSfVariable(string name) =>
			Environment.GetEnvironmentVariable(name); // Environment variables should be provided by SF

		public static string GetPublishAddress() =>
			GetSfVariable(PublishAddressEvnVariableName) ?? throw new SfConfigurationException(PublishAddressEvnVariableName);

		public static int GetEndpointPort(string endpointName)
		{
			string variableName = EndpointPortEvnVariableSuffix + endpointName;
			return int.TryParse(GetSfVariable(variableName), out int parseResult)
				? parseResult
				: throw new SfConfigurationException(variableName);
		}
	}
}
