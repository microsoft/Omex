// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Event arguments used with correlation events
	/// </summary>
	public class CorrelationEventArgs : EventArgs
	{
		/// <summary>
		/// Correlation data
		/// </summary>
		public CorrelationData Correlation { get; private set; }

		/// <summary>
		/// If the correlation data has changed, the key of the data that changed
		/// </summary>
		public string ChangedKey { get; private set; }

		/// <summary>
		/// If the correlation data has changed, the old value of the data
		/// </summary>
		public string OldData { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="data">correlation data</param>
		/// <param name="key">changed key</param>
		/// <param name="oldData">the previous value of the data</param>
		public CorrelationEventArgs(CorrelationData data, string key, string oldData)
		{
			Correlation = data;
			ChangedKey = key;
			OldData = oldData;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="data">correlation data</param>
		public CorrelationEventArgs(CorrelationData data)
		{
			Correlation = data;
			ChangedKey = null;
			OldData = null;
		}
	}
}
