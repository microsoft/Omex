// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;

namespace Services.Remoting
{
	[TestClass]
	public class ServiceRemotingClientWrapperTests
	{
		[TestMethod]
		public void Constructor_SetClientProperties()
		{
			string listenerName = nameof(Constructor_SetClientProperties);
			ResolvedServiceEndpoint endpoint = MockQueryPartitionFactory.CreateResolvedServiceEndpoint(string.Empty);
			ResolvedServicePartition partition = MockQueryPartitionFactory.CreateResolvedServicePartition(
				new Uri("https://localhost"),
				new List<ResolvedServiceEndpoint>());
			Mock<IServiceRemotingClient> clientMock = new();
			clientMock.SetupGet(c => c.ListenerName).Returns(listenerName);
			clientMock.SetupGet(c => c.Endpoint).Returns(endpoint);
			clientMock.SetupGet(c => c.ResolvedServicePartition).Returns(partition);

			ServiceRemotingClientWrapper wrapper = new(clientMock.Object);
			Assert.AreEqual(clientMock.Object, wrapper.Client);
			Assert.AreEqual(listenerName, wrapper.ListenerName);
			Assert.AreEqual(endpoint, wrapper.Endpoint);
			Assert.AreEqual(partition, wrapper.ResolvedServicePartition);
		}

		[TestMethod]
		public void Constructor_SendOneWay_PropagateCalls()
		{
			IServiceRemotingRequestMessage messsageMock = CreateMessage();
			Mock<IServiceRemotingClient> clientMock = new();
			ServiceRemotingClientWrapper wrapper = new(clientMock.Object);

			wrapper.SendOneWay(messsageMock);

			clientMock.Verify(c => c.SendOneWay(messsageMock), Times.Once);
		}

		[TestMethod]
		public async Task Constructor_RequestResponseAsync_PropagateCalls()
		{
			IServiceRemotingResponseMessage expectedResponce = new Mock<IServiceRemotingResponseMessage>().Object;
			IServiceRemotingRequestMessage messsageMock = CreateMessage();
			Mock<IServiceRemotingClient> clientMock = new();
			clientMock.Setup(c => c.RequestResponseAsync(messsageMock)).Returns(Task.FromResult(expectedResponce));

			ServiceRemotingClientWrapper wrapper = new(clientMock.Object);

			IServiceRemotingResponseMessage responce = await wrapper.RequestResponseAsync(messsageMock);

			clientMock.Verify(c => c.RequestResponseAsync(messsageMock), Times.Once);
			Assert.AreEqual(expectedResponce, responce);
		}

		[TestMethod]
		public void Constructor_SendOneWay_ReportsException()
		{
			ArithmeticException exception = new();
			IServiceRemotingRequestMessage messsageMock = CreateMessage();
			Mock<IServiceRemotingClient> clientMock = new();
			clientMock.Setup(c => c.SendOneWay(messsageMock)).Throws(exception);
			(DiagnosticListener listener, MockObserver observer) = MockObserver.CreateListener();
			ServiceRemotingClientWrapper wrapper = new(clientMock.Object, listener);

			Assert.ThrowsException<ArithmeticException>(() => wrapper.SendOneWay(messsageMock));
			observer.AssertException(exception);
		}

		[TestMethod]
		public async Task Constructor_RequestResponseAsync_ReportsException()
		{
			ArithmeticException exception = new();
			IServiceRemotingRequestMessage messsageMock = CreateMessage();
			Mock<IServiceRemotingClient> clientMock = new();
			clientMock.Setup(c => c.RequestResponseAsync(messsageMock)).Throws(exception);
			(DiagnosticListener listener, MockObserver observer) = MockObserver.CreateListener();
			ServiceRemotingClientWrapper wrapper = new(clientMock.Object, listener);

			await Assert.ThrowsExceptionAsync<ArithmeticException>(() => wrapper.RequestResponseAsync(messsageMock));
			observer.AssertException(exception);
		}

		private IServiceRemotingRequestMessage CreateMessage()
		{
			Mock<IServiceRemotingRequestMessage> messsageMock = new();
			messsageMock.Setup(m => m.GetHeader()).Returns(new Mock<IServiceRemotingRequestMessageHeader>().Object);
			return messsageMock.Object;
		}
	}
}
