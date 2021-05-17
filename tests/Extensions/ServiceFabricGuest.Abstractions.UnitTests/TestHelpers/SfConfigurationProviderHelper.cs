// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions.UnitTests
{
	public static class SfConfigurationProviderHelper
	{
		public static void SetPortVariable(string name, int port) =>
			Environment.SetEnvironmentVariable(SfConfigurationProvider.EndpointPortEvnVariableSuffix + name, port.ToString(), EnvironmentVariableTarget.Process);

		public static void SetPublishAddress(string value = "localhost") =>
			Environment.SetEnvironmentVariable(SfConfigurationProvider.PublishAddressEvnVariableName, value, EnvironmentVariableTarget.Process);
	}
}
