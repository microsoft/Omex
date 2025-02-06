// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.Extensions.Hosting.Services.UnitTests;
using Microsoft.Omex.Extensions.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting.UnitTests
{
	[TestClass]
	public class GenericRemotingListenerBuilderTests
	{
		[TestMethod]
		public void GenericRemotingListenerBuilder_NoTransportSettigns_ThrowsException()
		{
			string name = "TestName";
			MockService mockService = new();
			IServiceProvider mockProvider = new Mock<IServiceProvider>().Object;
			OmexStatefulService omexStatefulService = MockServiceFabricServices.MockOmexStatefulService;

			Assert.ThrowsException<InsecureRemotingUnsupportedException>(() => new GenericRemotingListenerBuilder<OmexStatefulService>(name, mockProvider,
					(p, s) =>
					{
						Assert.AreEqual(omexStatefulService, s);
						Assert.AreEqual(mockProvider, p);
						return mockService;
					}));
		}

		[TestMethod]
		public void GenericRemotingListenerBuilder_WithTransportSettigns_PropagatesParameters()
		{
			string name = "TestName";
			MockService mockService = new();
			IServiceProvider mockProvider = new Mock<IServiceProvider>().Object;
			OmexStatefulService omexStatefulService = MockServiceFabricServices.MockOmexStatefulService;
			FabricTransportRemotingListenerSettings transportSettings = new();

			GenericRemotingListenerBuilder<OmexStatefulService> builder =
				new(name, mockProvider,
					(p, s) =>
					{
						Assert.AreEqual(omexStatefulService, s);
						Assert.AreEqual(mockProvider, p);
						return mockService;
					},
					transportSettings);

			Assert.AreEqual(name, builder.Name);

			IService service = builder.BuildService(omexStatefulService);

			Assert.AreEqual(mockService, service);
		}
	}
}
