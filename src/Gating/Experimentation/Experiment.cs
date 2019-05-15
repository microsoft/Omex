// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Gating.Experimentation
{
	/// <summary>
	/// Class that represents an experiment
	/// </summary>
	public class Experiment : IExperiment
	{
		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public Experiment() => m_gates = new List<IGate>();

		#endregion


		/// <summary>
		/// Adds a gate
		/// </summary>
		/// <param name="gate">The gate</param>
		/// <returns>true if gate added successfully</returns>
		public bool Add(IGate gate)
		{
			if (!Code.ValidateArgument(gate, nameof(gate), TaggingUtilities.ReserveTag(0x23821089 /* tag_967cj */)) ||
				!Code.ValidateArgument(gate.ExperimentInfo, nameof(gate.ExperimentInfo), TaggingUtilities.ReserveTag(0x2382108a /* tag_967ck */)) ||
				!Code.ValidateNotNullOrWhiteSpaceArgument(
					gate.ExperimentInfo.ExperimentName,
					nameof(gate.ExperimentInfo.ExperimentName),
					TaggingUtilities.ReserveTag(0x2382108b /* tag_967cl */)))
			{
				return false;
			}

			m_gates.Add(gate);
			return true;
		}


		#region Member variables

		/// <summary>
		/// All gates in the DataSet
		/// </summary>
		public IEnumerable<IGate> Gates => m_gates;


		/// <summary>
		/// All gates in the DataSet
		/// </summary>
		private readonly IList<IGate> m_gates;
		#endregion
	}
}