// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

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
