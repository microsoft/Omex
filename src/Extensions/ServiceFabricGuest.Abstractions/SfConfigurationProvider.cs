// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	/// <summary>
	/// Configuration provider for service fabric environment variables
	/// </summary>
	public class SfConfigurationProvider
	{
		internal const string PublishAddressEvnVariableName = "Fabric_NodeIPOrFQDN";

		internal const string EndpointPortEvnVariableSuffix = "Fabric_Endpoint_";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">Name of the SF variable</param>
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
