// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#region Using Directives

using System;

#endregion

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Class for handling transaction data
	/// </summary>
	public class TransactionData
	{
		/// <summary>
		/// Transaction id
		/// </summary>
		public uint TransactionId { get; set; }


		/// <summary>
		/// Transaction Context Id
		/// </summary>
		public uint TransactionContextId { get; set; }


		/// <summary>
		/// Transaction Step Number
		/// </summary>
		public uint TransactionStep { get; set; }


		/// <summary>
		/// Originator
		/// </summary>
		public string Originator { get; set; }


		/// <summary>
		/// Start tick
		/// </summary>
		public long StartTick { get; set; }


		/// <summary>
		/// Correlation Id
		/// </summary>
		public Guid CorrelationId { get; set; }


		/// <summary>
		/// Tracks the depth of the call through proxying layers
		/// Original caller is Depth 0, subsequent hops are +1 each time
		/// </summary>
		/// <remarks>
		/// WARNING: this field is used internally by the telemetry infrastructure and should
		/// NOT be set to anything except 0 (i.e. leave as default!) by consumers making a service call
		/// </remarks>
		public uint CallDepth { get; set; }


		/// <summary>
		/// logging sequence number used for guarenteed ordering of correlation events across machine boundaries
		/// </summary>
		/// <remarks>
		/// WARNING: this field is used internally by the telemetry infrastructure and should
		/// NOT be set to anything except 0 (i.e. leave as default!) by consumers making a service call
		/// </remarks>
		public long EventSequenceNumber { get; set; }


		/// <summary>
		/// Result
		/// </summary>
		public uint Result { get; set; }


		/// <summary>
		/// User hash for the current correlation.
		/// Used to propagate the value between internal S2S calls.
		/// </summary>
		/// <remarks>
		/// <see cref="CorrelationData.UserHash"/> for details on how the field is computed.
		/// </remarks>
		public string UserHash { get; set; }


		/// <summary>
		/// The TestScenarioId for Response Injection
		/// </summary>
		public string TestScenarioId { get; set; }


		/// <summary>
		/// The TestScenarioRecordingState for Response Injection
		/// </summary>
		/// <remarks>
		/// Values include 'play' and 'record'
		/// </remarks>
		public string TestScenarioRecordingState { get; set; }


		/// <summary>
		/// The TestSceanrio failure message, if any, from use of Response Injection
		/// </summary>
		public string TestScenarioFailureMessage { get; set; }


		/// <summary>
		/// Is this a fallback call
		/// </summary>
		public bool IsFallbackCall { get; set; }


		/// <summary>
		/// Construct an empty transaction data
		/// </summary>
		public TransactionData()
		{
		}
	}
}
