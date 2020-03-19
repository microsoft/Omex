// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Interface for handling correlation
	/// </summary>
	public interface ICorrelationStorage
	{
		/// <summary>
		/// Start correlation
		/// </summary>
		/// <param name="data">existing correlation data</param>
		/// <returns>correlation data</returns>
		CorrelationData CorrelationStart(CorrelationData data);

		/// <summary>
		/// Add correlation data
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		/// <param name="data">existing correlation data</param>
		void CorrelationAdd(string key, string value, CorrelationData data);

		/// <summary>
		/// End correlation
		/// </summary>
		/// <param name="data">correlation data</param>
		void CorrelationEnd(CorrelationData data);
	}
}
