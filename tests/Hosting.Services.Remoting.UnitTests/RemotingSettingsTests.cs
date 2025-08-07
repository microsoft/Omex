// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting.UnitTests
{
	[TestClass]
	public class RemotingSettingsTests
	{
		[TestMethod]
		[DataRow("")]
		[DataRow("TestEndpointName")]
		public void WithEndpoint_SetsProperValues(string value)
		{
			FabricTransportRemotingListenerSettings settings = RemotingSettings.WithEndpoint(value);
			Assert.AreEqual(value, settings.EndpointResourceName);
		}
	}
}
