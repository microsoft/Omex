// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Monitors timing and test transactions
	/// </summary>
	class TransactionMonitor
	{
		/// <summary>
		/// The id of the running transaction
		/// </summary>
		/// <param name="correlation">current correlation</param>
		/// <returns>id of the running transaction</returns>
		public static uint RunningTransaction(CorrelationData correlation)
		{
			uint id = Transactions.None;
			if (correlation != null)
			{
				id = correlation.TransactionId;
			}
			return id;
		}
	}
}
