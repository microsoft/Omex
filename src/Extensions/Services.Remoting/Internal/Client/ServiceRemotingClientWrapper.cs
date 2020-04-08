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
	/// <remarks>
	/// Implemented simular to Asp .Net Core http request from here: https://github.com/dotnet/aspnetcore/blob/b6698757a85f9b6d6c63311e64dd6ac9e734ef56/src/Hosting/Hosting/src/Internal/HostingApplicationDiagnostics.cs
	/// </remarks>
	internal class ServiceRemotingClientWrapper : IServiceRemotingClient
	{
		/// <summary>
		/// Suffix for this class activities
		/// </summary>
		/// <remarks>
		/// Should end with RequestOut to be enabled by out telemetry
		/// </remarks>
		private const string RequestOutActivitySuffix = "RequestOut";

		private const string OneWayMessageActivityName = Diagnostics.AssemblyName + "OneWay" + RequestOutActivitySuffix;

		private const string RequestActivityName = Diagnostics.AssemblyName + RequestOutActivitySuffix;

		internal readonly IServiceRemotingClient Client;

		public ServiceRemotingClientWrapper(IServiceRemotingClient client)
		{
			Client = client;
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
			Activity? activity = Diagnostics.StartActivity(OneWayMessageActivityName);
			OmexRemotingHeaders.AttachActivityToOuthgoingRequest(activity, requestMessage);
			IServiceRemotingResponseMessage? responseMessage = null;

			try
			{
				responseMessage = await Client.RequestResponseAsync(requestMessage).ConfigureAwait(false);
				activity?.SetResult(TimedScopeResult.Success);
			}
			catch (Exception ex)
			{
				Diagnostics.ReportException(ex);
				throw;
			}
			finally
			{
				Diagnostics.StopActivity(activity);
			}

			return responseMessage;
		}

		public void SendOneWay(IServiceRemotingRequestMessage requestMessage)
		{
			Activity? activity = Diagnostics.StartActivity(RequestActivityName);
			OmexRemotingHeaders.AttachActivityToOuthgoingRequest(activity, requestMessage);

			try
			{
				Client.SendOneWay(requestMessage);
				activity?.SetResult(TimedScopeResult.Success);
			}
			catch (Exception ex)
			{
				Diagnostics.ReportException(ex);
				throw;
			}
			finally
			{
				Diagnostics.StopActivity(activity);
			}
		}
	}
}
