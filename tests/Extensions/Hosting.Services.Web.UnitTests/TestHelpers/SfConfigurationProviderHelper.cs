// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests
{
	internal static class SfConfigurationProviderHelper
	{
		internal static void SetPortVariable(string name, int port) =>
			Environment.SetEnvironmentVariable(SfConfigurationProvider.EndpointPortEvnVariableSuffix + name, port.ToString(), EnvironmentVariableTarget.Process);

		internal static void SetPublishAddress(string value = "localhost") =>
			Environment.SetEnvironmentVariable(SfConfigurationProvider.PublishAddressEvnVariableName, value, EnvironmentVariableTarget.Process);
	}
}
