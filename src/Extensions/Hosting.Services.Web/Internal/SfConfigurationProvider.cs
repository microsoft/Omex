// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	internal class SfConfigurationProvider
	{
		private const string PublishAddressEvnVariableName = "Fabric_NodeIPOrFQDN";

		private const string EndpointPortEvnVariableSuffix = "Fabric_Endpoint_";

		private static string? GetSfVariable(string name) =>
			Environment.GetEnvironmentVariable(name); // Environment variables should be provided by SF

		private static Exception CreateException(string value) =>
			new ArgumentException($"Failed to get {value} from environment variables");

		public static string GetPublishAddress() =>
			GetSfVariable(PublishAddressEvnVariableName) ?? throw CreateException("PublishAddress");

		public static int GetEndpointPort(string endpointName) =>
			int.TryParse(GetSfVariable(EndpointPortEvnVariableSuffix + endpointName), out int parseResult)
				? parseResult
				: throw CreateException("EndpointPort");
	}
}
