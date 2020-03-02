// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class OmexServiceFabricContextTests
	{
		[TestMethod]
		public void TestStateless() =>
			TestContextSet(ServiceFabric.Mocks.MockStatelessServiceContextFactory.Default);


		[TestMethod]
		public void TestStatefulWithValue() =>
			TestContextSet(ServiceFabric.Mocks.MockStatefulServiceContextFactory.Default);


		public void TestContextSet<TContext>(TContext serviceContext)
			where TContext : ServiceContext
		{
			ServiceContextAccessor<TContext> accessor = new ServiceContextAccessor<TContext>();
			IServiceContext context = new OmexServiceFabricContext(accessor);

			Assert.AreEqual(default, context.PartitionId);
			Assert.AreEqual(default, context.ReplicaOrInstanceId);

			accessor.SetContext(serviceContext);

			Assert.AreEqual(serviceContext.PartitionId, context.PartitionId);
			Assert.AreEqual(serviceContext.ReplicaOrInstanceId, context.ReplicaOrInstanceId);
		}
	}
}
