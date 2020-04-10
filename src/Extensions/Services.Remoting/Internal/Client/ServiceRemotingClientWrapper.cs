// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;

namespace Microsoft.Omex.Extensions.Services.Remoting.Client
{
	/// <summary>
	/// Wrapper for adding Omex headers to Remoting request
	/// </summary>
	internal class ServiceRemotingClientWrapper : IServiceRemotingClient
	{
		/// <summary>
		/// Suffix for this class activities
		/// </summary>
		/// <remarks>
		/// Should end with RequestOut to be enabled by our telemetry
		/// </remarks>
		private const string RequestOutActivitySuffix = "RequestOut";

		private const string OneWayMessageActivityName = Diagnostics.ListenerName + "OneWay" + RequestOutActivitySuffix;

		private const string RequestActivityName = Diagnostics.ListenerName + RequestOutActivitySuffix;

		private readonly DiagnosticListener m_diagnosticListener;

		internal readonly IServiceRemotingClient Client;

		public ServiceRemotingClientWrapper(IServiceRemotingClient client, DiagnosticListener? diagnosticListener = null)
		{
			Client = client;
			m_diagnosticListener = diagnosticListener ?? Diagnostics.DefaultListener;
		}

		public ResolvedServicePartition ResolvedServicePartition
		{
			get => Client.ResolvedServicePartition;
			set => Client.ResolvedServicePartition = value;
		}

		public string ListenerName
		{
			get => Client.ListenerName;
			set => Client.ListenerName = value;
		}

		public ResolvedServiceEndpoint Endpoint
		{
			get => Client.Endpoint;
			set => Client.Endpoint = value;
		}

		public async Task<IServiceRemotingResponseMessage> RequestResponseAsync(IServiceRemotingRequestMessage requestMessage)
		{
			Activity? activity = m_diagnosticListener.CreateAndStartActivity(OneWayMessageActivityName);
			requestMessage.AttachActivityToOutgoingRequest(activity);
			IServiceRemotingResponseMessage? responseMessage = null;

			try
			{
				responseMessage = await Client.RequestResponseAsync(requestMessage).ConfigureAwait(false);
				activity?.SetResult(TimedScopeResult.Success);
			}
			catch (Exception ex)
			{
				m_diagnosticListener.ReportException(ex);
				throw;
			}
			finally
			{
				m_diagnosticListener.StopActivityIfExist(activity);
			}

			return responseMessage;
		}

		public void SendOneWay(IServiceRemotingRequestMessage requestMessage)
		{
			Activity? activity = m_diagnosticListener.CreateAndStartActivity(RequestActivityName);
			requestMessage.AttachActivityToOutgoingRequest(activity);

			try
			{
				Client.SendOneWay(requestMessage);
				activity?.SetResult(TimedScopeResult.Success);
			}
			catch (Exception ex)
			{
				m_diagnosticListener.ReportException(ex);
				throw;
			}
			finally
			{
				m_diagnosticListener.StopActivityIfExist(activity);
			}
		}
	}
}
