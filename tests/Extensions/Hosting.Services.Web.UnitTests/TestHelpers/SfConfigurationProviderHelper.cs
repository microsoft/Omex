// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	internal static class SfConfigurationProviderHelper
	{
		/// SF environment variables taken from https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-environment-variables-reference
		internal const string PublishAddressEvnVariableName = "Fabric_NodeIPOrFQDN";
		internal const string EndpointPortEvnVariableSuffix = "Fabric_Endpoint_";

		internal static void SetPortVariable(string name, int port) =>
			Environment.SetEnvironmentVariable(EndpointPortEvnVariableSuffix + name, port.ToString(), EnvironmentVariableTarget.Process);

		internal static void SetPublishAddress(string value = "localhost") =>
			Environment.SetEnvironmentVariable(PublishAddressEvnVariableName, value, EnvironmentVariableTarget.Process);
	}
}
