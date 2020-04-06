﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class OmexServiceFabricContextTests
	{
		[TestMethod]
		public void SetContext_SetsStatelessContext() =>
			TestContextSet(MockStatelessServiceContextFactory.Default);

		[TestMethod]
		public void SetContext_StatefulContext() =>
			TestContextSet(MockStatefulServiceContextFactory.Default);

		public void TestContextSet<TContext>(TContext serviceContext)
			where TContext : ServiceContext
		{
			Accessor<TContext> accessor = new Accessor<TContext>();
			IAccessorSetter<TContext> setter = accessor;
			IServiceContext context = new OmexServiceFabricContext(accessor);

			Assert.AreEqual(default, context.PartitionId);
			Assert.AreEqual(default, context.ReplicaOrInstanceId);

			setter.SetContext(serviceContext);

			Assert.AreEqual(serviceContext.PartitionId, context.PartitionId);
			Assert.AreEqual(serviceContext.ReplicaOrInstanceId, context.ReplicaOrInstanceId);
		}
	}
}
