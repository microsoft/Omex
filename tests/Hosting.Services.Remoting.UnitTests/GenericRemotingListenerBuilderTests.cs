// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.Extensions.Hosting.Services.UnitTests;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting.UnitTests
{
	[TestClass]
	public class GenericRemotingListenerBuilderTests
	{
		[TestMethod]
		public void GenericRemotingListenerBuilder_PropagatesParameters()
		{
			string name = "TestName";
			MockService mockService = new MockService();
			IServiceProvider mockProvider = new Mock<IServiceProvider>().Object;
			OmexStatefulService omexStatefulService = MockServiceFabricServices.MockOmexStatefulService;

			GenericRemotingListenerBuilder<OmexStatefulService> builder =
				new GenericRemotingListenerBuilder<OmexStatefulService>(name, mockProvider,
					(p, s) =>
					{
						Assert.AreEqual(omexStatefulService, s);
						Assert.AreEqual(mockProvider, p);
						return mockService;
					});

			Assert.AreEqual(name, builder.Name);

			IService service = builder.BuildService(omexStatefulService);

			Assert.AreEqual(mockService, service);
		}
	}
}
