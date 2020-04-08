// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;

namespace Microsoft.Omex.Extensions.Services.Remoting.Client
{
	/// <summary>
	/// A factory for creating <see cref="IServiceRemotingClientFactory" />
	/// </summary>
	internal class OmexServiceRemotingClientFactory : IServiceRemotingClientFactory
	{
		private readonly IServiceRemotingClientFactory m_serviceRemotingClientFactory;

		public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>>? ClientConnected;

		public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>>? ClientDisconnected;

		public OmexServiceRemotingClientFactory(IServiceRemotingClientFactory serviceRemotingClientFactory)
		{
			m_serviceRemotingClientFactory = serviceRemotingClientFactory;
			m_serviceRemotingClientFactory.ClientConnected += ClientConnected;
			m_serviceRemotingClientFactory.ClientDisconnected += ClientDisconnected;
		}

		public async Task<IServiceRemotingClient> GetClientAsync(
			Uri serviceUri,
			ServicePartitionKey partitionKey,
			TargetReplicaSelector targetReplicaSelector,
			string listenerName,
			OperationRetrySettings retrySettings,
			CancellationToken cancellationToken)
		{
			IServiceRemotingClient client = await m_serviceRemotingClientFactory.GetClientAsync(
				serviceUri,
				partitionKey,
				targetReplicaSelector,
				listenerName,
				retrySettings,
				cancellationToken).ConfigureAwait(false);

			return new ServiceRemotingClientWrapper(client);
		}

		public async Task<IServiceRemotingClient> GetClientAsync(
			ResolvedServicePartition previousRsp,
			TargetReplicaSelector targetReplicaSelector,
			string listenerName,
			OperationRetrySettings retrySettings,
			CancellationToken cancellationToken)
		{
			IServiceRemotingClient client = await m_serviceRemotingClientFactory.GetClientAsync(
				previousRsp,
				targetReplicaSelector,
				listenerName,
				retrySettings,
				cancellationToken).ConfigureAwait(false);

			return new ServiceRemotingClientWrapper(client);
		}

		public IServiceRemotingMessageBodyFactory GetRemotingMessageBodyFactory() =>
			m_serviceRemotingClientFactory.GetRemotingMessageBodyFactory();

		public Task<OperationRetryControl> ReportOperationExceptionAsync(
			IServiceRemotingClient client,
			ExceptionInformation exceptionInformation,
			OperationRetrySettings retrySettings,
			CancellationToken cancellationToken) =>
				m_serviceRemotingClientFactory.ReportOperationExceptionAsync(
					Unwrap(client),
					exceptionInformation,
					retrySettings,
					cancellationToken);

		private IServiceRemotingClient Unwrap(IServiceRemotingClient client) =>
			client is ServiceRemotingClientWrapper wrapper
				? wrapper.Client
				: client;
	}
}
