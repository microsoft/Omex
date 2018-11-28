/**************************************************************************************************
	IGateDataSet.cs

	Interface handling the loading of gates.
**************************************************************************************************/

#region Using Directives

using System;
using System.Collections.Generic;
using Microsoft.Omex.Gating.Experimentation;

#endregion

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Interface for handling the loading of gates.
	/// </summary>
	public interface IGateDataSet
	{
		/// <summary>
		/// Get a gate by name
		/// </summary>
		/// <param name="gateName">name of gate</param>
		/// <returns>found gate, should return null if a gate matching the name could not be found</returns>
		IGate GetGate(string gateName);


		/// <summary>
		/// Lookup a gate based on its key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>Gate instance if found, otherwise null</returns>
		IGate GetGateByKey(int key);


		/// <summary>
		/// The timestamp of the last reload of the dataset
		/// </summary>
		DateTime LastReload { get; }


		/// <summary>
		/// Get the gate names that are part of the dataset
		/// </summary>
		IEnumerable<string> GateNames { get; }


		/// <summary>
		/// Get the gates that are part of the dataset
		/// </summary>
		IEnumerable<IGate> Gates { get; }


		/// <summary>
		/// Get the experiments that are part of the dataset
		/// </summary>
		IExperiments Experiments { get; }
	}
}
