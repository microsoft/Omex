// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;

namespace Services.Remoting
{
	[TestClass]
	public class OmexServiceRemotingClientFactoryTests
	{
		[TestMethod]
		public void GetRemotingMessageBodyFactory_PropagatesCalls()
		{
			IServiceRemotingMessageBodyFactory expectedResult = new Mock<IServiceRemotingMessageBodyFactory>().Object;
			Mock<IServiceRemotingClientFactory> factoryMock = new();
			factoryMock.Setup(f => f.GetRemotingMessageBodyFactory()).Returns(expectedResult);
			OmexServiceRemotingClientFactory wrapper = new(factoryMock.Object);

			IServiceRemotingMessageBodyFactory actualResult = wrapper.GetRemotingMessageBodyFactory();
			factoryMock.Verify(f => f.GetRemotingMessageBodyFactory());

			Assert.AreEqual(expectedResult, actualResult);
		}

		[TestMethod]
		public async Task GetClientAsync_UsingUri_WrapsClient()
		{
			Uri serviceUri = new("https://localhost");
			ServicePartitionKey partitionKey = new();
			TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.PrimaryReplica;
			string listenerName = nameof(GetClientAsync_UsingUri_WrapsClient);
			OperationRetrySettings retrySettings = new();
			CancellationToken cancellationToken = CancellationToken.None;

			Expression<Func<IServiceRemotingClientFactory, Task<IServiceRemotingClient>>> expression = f =>
				f.GetClientAsync(
					serviceUri,
					partitionKey,
					targetReplicaSelector,
					listenerName,
					retrySettings,
					cancellationToken);

			IServiceRemotingClient expectedResult = new Mock<IServiceRemotingClient>().Object;
			Mock<IServiceRemotingClientFactory> factoryMock = new();
			factoryMock.Setup(expression).Returns(Task.FromResult(expectedResult));
			OmexServiceRemotingClientFactory wrapper = new(factoryMock.Object);

			IServiceRemotingClient actualResult = await wrapper.GetClientAsync(
				serviceUri,
				partitionKey,
				targetReplicaSelector,
				listenerName,
				retrySettings,
				cancellationToken).ConfigureAwait(false);

			factoryMock.Verify(expression, Times.Once);

			Assert.IsInstanceOfType(actualResult, typeof(ServiceRemotingClientWrapper));
			Assert.AreEqual(expectedResult, OmexServiceRemotingClientFactory.Unwrap(actualResult));
		}

		[TestMethod]
		public async Task GetClientAsync_UsingPreviousRsp_WrapsClient()
		{
			ResolvedServicePartition previousRsp = MockQueryPartitionFactory.CreateResolvedServicePartition(
				new Uri("https://localhost"),
				new List<ResolvedServiceEndpoint>());
			TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.PrimaryReplica;
			string listenerName = nameof(GetClientAsync_UsingPreviousRsp_WrapsClient);
			OperationRetrySettings retrySettings = new();
			CancellationToken cancellationToken = CancellationToken.None;

			Expression<Func<IServiceRemotingClientFactory, Task<IServiceRemotingClient>>> expression = f =>
				f.GetClientAsync(
					previousRsp,
					targetReplicaSelector,
					listenerName,
					retrySettings,
					cancellationToken);

			IServiceRemotingClient expectedResult = new Mock<IServiceRemotingClient>().Object;
			Mock<IServiceRemotingClientFactory> factoryMock = new();
			factoryMock.Setup(expression).Returns(Task.FromResult(expectedResult));
			OmexServiceRemotingClientFactory wrapper = new(factoryMock.Object);

			IServiceRemotingClient actualResult = await wrapper.GetClientAsync(
				previousRsp,
				targetReplicaSelector,
				listenerName,
				retrySettings,
				cancellationToken).ConfigureAwait(false);

			factoryMock.Verify(expression, Times.Once);

			Assert.IsInstanceOfType(actualResult, typeof(ServiceRemotingClientWrapper));
			Assert.AreEqual(expectedResult, OmexServiceRemotingClientFactory.Unwrap(actualResult));
		}

		[TestMethod]
		public async Task ReportOperationExceptionAsync_PropagatesCalls()
		{
			IServiceRemotingClient client = new Mock<IServiceRemotingClient>().Object;
			ExceptionInformation exceptionInformation = new(new FileNotFoundException());
			OperationRetrySettings retrySettings = new();
			CancellationToken cancellationToken = CancellationToken.None;

			Expression<Func<IServiceRemotingClientFactory, Task<OperationRetryControl>>> expression = f =>
				f.ReportOperationExceptionAsync(
					client,
					exceptionInformation,
					retrySettings,
					cancellationToken);

			OperationRetryControl expectedResult = new();
			Mock<IServiceRemotingClientFactory> factoryMock = new();
			factoryMock.Setup(expression).Returns(Task.FromResult(expectedResult));
			OmexServiceRemotingClientFactory wrapper = new(factoryMock.Object);

			OperationRetryControl actualResult = await wrapper.ReportOperationExceptionAsync(
				client,
				exceptionInformation,
				retrySettings,
				cancellationToken).ConfigureAwait(false);

			factoryMock.Verify(expression, Times.Once);

			Assert.AreEqual(expectedResult, actualResult);
		}
	}
}
