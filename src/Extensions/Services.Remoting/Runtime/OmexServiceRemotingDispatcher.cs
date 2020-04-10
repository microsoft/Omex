// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace Microsoft.Omex.Extensions.Services.Remoting.Runtime
{
	/// <summary>
	/// Wrapper on top of <see cref="ServiceRemotingMessageDispatcher" /> to support omex correlation
	/// </summary>
	public class OmexServiceRemotingDispatcher : ServiceRemotingMessageDispatcher
	{
		/// <summary>
		/// Suffix for this class activities
		/// </summary>
		/// <remarks>
		/// Should end with RequestIn to be enabled by our telemetry
		/// </remarks>
		private const string RequestInActivitySuffix = "RequestIn";

		private const string OneWayMessageActivityName = Diagnostics.ListenerName + "OneWay" + RequestInActivitySuffix;

		private const string RequestActivityName = Diagnostics.ListenerName + RequestInActivitySuffix;

		private readonly DiagnosticListener m_diagnosticListener;

		/// <inheritdoc />
		public OmexServiceRemotingDispatcher(
			ServiceContext serviceContext,
			IService serviceImplementation,
			IServiceRemotingMessageBodyFactory? serviceRemotingMessageBodyFactory = null,
			DiagnosticListener? diagnosticListener = null)
			: base(serviceContext, serviceImplementation, serviceRemotingMessageBodyFactory)
		{
			m_diagnosticListener = diagnosticListener ?? Diagnostics.DefaultListener;
		}

		/// <inheritdoc />
		public override void HandleOneWayMessage(IServiceRemotingRequestMessage requestMessage)
		{
			Activity? activity = m_diagnosticListener.CreateAndStartActivity(OneWayMessageActivityName);

			try
			{
				requestMessage.ExtractActivityFromIncomingRequest(activity);
				base.HandleOneWayMessage(requestMessage);
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


		/// <inheritdoc />
		public override async Task<IServiceRemotingResponseMessage> HandleRequestResponseAsync(
			IServiceRemotingRequestContext requestContext,
			IServiceRemotingRequestMessage requestMessage)
		{
			Activity? activity = m_diagnosticListener.CreateAndStartActivity(RequestActivityName);
			IServiceRemotingResponseMessage? responseMessage = null;

			try
			{
				requestMessage.ExtractActivityFromIncomingRequest(activity);
				responseMessage = await base.HandleRequestResponseAsync(requestContext, requestMessage).ConfigureAwait(false);
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
	}
}
