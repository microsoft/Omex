// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// In-memory correlation handler
	/// </summary>
	public class MemoryCorrelationHandler : ICorrelationStorage
	{
		/// <summary>
		/// Start correlation
		/// </summary>
		/// <param name="data">existing correlation data</param>
		/// <returns>correlation data</returns>
		public CorrelationData CorrelationStart(CorrelationData data)
		{
			if (data == null)
			{
				data = new CorrelationData();
				data.VisibleId = Guid.NewGuid();
			}
			return data;
		}


		/// <summary>
		/// Add correlation data
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		/// <param name="data">existing correlation data</param>
		public void CorrelationAdd(string key, string value, CorrelationData data)
		{
			if (data != null)
			{
				data.AddData(key, value);
			}
		}


		/// <summary>
		/// End correlation
		/// </summary>
		/// <param name="data">correlation data</param>
		public void CorrelationEnd(CorrelationData data)
		{
		}
	}
}
