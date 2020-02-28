// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
		public void TestStatelessWithValue()
		{
			Mock<IServiceContextAccessor<StatelessServiceContext>> mockContext = new Mock<IServiceContextAccessor<StatelessServiceContext>>();
			mockContext.SetupGet(c => c.ServiceContext).Returns(ServiceFabric.Mocks.MockStatelessServiceContextFactory.Default);
			IServiceContext context = new OmexServiceFabricContext(mockContext.Object);
			CheckCashedValue(context, mockContext);
		}


		[TestMethod]
		public void TestStatelessWithoutValue()
		{
			Mock<IServiceContextAccessor<StatelessServiceContext>> mockContext = new Mock<IServiceContextAccessor<StatelessServiceContext>>();
			mockContext.SetupGet(c => c.ServiceContext).Returns((StatelessServiceContext?)null);
			IServiceContext context = new OmexServiceFabricContext(mockContext.Object);
			CheckNotCashedValue(context, mockContext);
		}


		[TestMethod]
		public void TestStatefulWithValue()
		{
			Mock<IServiceContextAccessor<StatefulServiceContext>> mockContext = new Mock<IServiceContextAccessor<StatefulServiceContext>>();
			mockContext.SetupGet(c => c.ServiceContext).Returns(ServiceFabric.Mocks.MockStatefulServiceContextFactory.Default);
			IServiceContext context = new OmexServiceFabricContext(mockContext.Object);
			CheckCashedValue(context, mockContext);
		}


		[TestMethod]
		public void TestStatefulWithoutValue()
		{
			Mock<IServiceContextAccessor<StatefulServiceContext>> mockContext = new Mock<IServiceContextAccessor<StatefulServiceContext>>();
			mockContext.SetupGet(c => c.ServiceContext).Returns((StatefulServiceContext?)null);
			IServiceContext context = new OmexServiceFabricContext(mockContext.Object);
			CheckNotCashedValue(context, mockContext);
		}


		private void CheckNotCashedValue<TContext>(IServiceContext context, Mock<IServiceContextAccessor<TContext>> mock)
			where TContext : ServiceContext
		{
			context.GetPartitionId();
			context.GetPartitionId();
			context.GetPartitionId();

			mock.VerifyGet(c => c.ServiceContext, Times.Exactly(3));

			context.GetReplicaOrInstanceId();
			context.GetReplicaOrInstanceId();
			context.GetReplicaOrInstanceId();
			context.GetReplicaOrInstanceId();

			mock.VerifyGet(c => c.ServiceContext, Times.Exactly(4));
		}


		private void CheckCashedValue<TContext>(IServiceContext context, Mock<IServiceContextAccessor<TContext>> mock)
			where TContext : ServiceContext
		{
			context.GetPartitionId();
			context.GetPartitionId();
			context.GetPartitionId();

			mock.VerifyGet(c => c.ServiceContext, Times.Once);

			context.GetReplicaOrInstanceId();
			context.GetReplicaOrInstanceId();
			context.GetReplicaOrInstanceId();
			context.GetReplicaOrInstanceId();

			mock.VerifyGet(c => c.ServiceContext, Times.Once);
		}
	}
}
