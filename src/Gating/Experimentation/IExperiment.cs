/***************************************************************************************************
	IExperiment.cs

	Interface that represents an experiment
***************************************************************************************************/

#region Using directives

using System.Collections.Generic;

#endregion

namespace Microsoft.Omex.Gating.Experimentation
{
	/// <summary>
	/// Interface that represents an experiment
	/// </summary>
	public interface IExperiment
	{
		/// <summary>
		/// Adds a gate
		/// </summary>
		/// <param name="gate">The gate</param>
		/// <returns>true if gate added successfully</returns>
		bool Add(IGate gate);


		/// <summary>
		/// All gates in the DataSet
		/// </summary>
		IEnumerable<IGate> Gates { get; }
	}
}
