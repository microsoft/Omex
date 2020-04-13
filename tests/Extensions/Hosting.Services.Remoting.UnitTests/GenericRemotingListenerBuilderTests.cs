// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Omex.Extensions.Hosting.Services.UnitTests;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;

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
			StatefulServiceContext omexStatefulService = MockStatefulServiceContextFactory.Default;

			GenericRemotingListenerBuilder<StatefulServiceContext> builder =
				new GenericRemotingListenerBuilder<StatefulServiceContext>(name, mockProvider,
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
