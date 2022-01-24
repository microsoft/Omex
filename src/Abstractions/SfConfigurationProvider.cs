// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Configuration provider for service fabric environment variables
	/// </summary>
	public class SfConfigurationProvider
	{
		/// SF environment variables taken from https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-environment-variables-reference
		internal const string PublishAddressEvnVariableName = "Fabric_NodeIPOrFQDN";

		internal const string EndpointPortEvnVariableSuffix = "Fabric_Endpoint_";

		/// <summary>
		/// Get Service Fabric Variable from environment
		/// </summary>
		/// <param name="name">Variable name</param>
		public static string? GetSfVariable(string name) =>
			Environment.GetEnvironmentVariable(name); // Environment variables should be provided by SF

		/// <summary>
		/// The IP or FQDN of the node, as specified in the cluster manifest file
		/// </summary>
		public static string GetPublishAddress() =>
			GetSfVariable(PublishAddressEvnVariableName) ?? throw new SfConfigurationException(PublishAddressEvnVariableName);

		/// <summary>
		/// Port number for the endpoint
		/// </summary>
		/// <param name="endpointName">Service endpoint name</param>
		public static int GetEndpointPort(string endpointName)
		{
			string variableName = EndpointPortEvnVariableSuffix + endpointName;
			return int.TryParse(GetSfVariable(variableName), out int parseResult)
				? parseResult
				: throw new SfConfigurationException(variableName);
		}
	}
}
