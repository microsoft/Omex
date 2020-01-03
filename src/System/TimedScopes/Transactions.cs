// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Set of known transactions
	/// </summary>
	public static class Transactions
	{
		/// <summary>
		/// There is no running transaction
		/// </summary>
		public const uint None = 0;


		/// <summary>
		/// A transaction to exercise the pipeline is running
		/// </summary>
		/// <remarks>This runs through the pipeline for web services,
		/// without actually running each request processor
		/// </remarks>
		public const uint ExercisePipeline = 1;


		/// <summary>
		/// A transaction to build dependency graphs
		/// </summary>
		/// <remarks>Builds up dependency graphs rather than processing the request</remarks>
		public const uint BuildDependencyGraph = 2;


		/// <summary>
		/// Transaction Id specified by when monitoring by watchdogs/observers/CVTs to
		/// mark that a test transaction is ongoing.
		/// </summary>
		/// <remarks>If a specific behaviour is required for the transaction, use one
		/// of the transaction id's above instead</remarks>
		public const uint MonitorEndpoint = 9998;
	}
}
