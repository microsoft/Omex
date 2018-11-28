﻿/***************************************************************************************************
	Experiment.cs

	Class that represents an experiment
***************************************************************************************************/

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
			if (!Code.ValidateArgument(gate, nameof(gate), TaggingUtilities.ReserveTag(0x238507c5 /* tag_97q5f */)) ||
				!Code.ValidateArgument(gate.ExperimentInfo, nameof(gate.ExperimentInfo), TaggingUtilities.ReserveTag(0x238507c6 /* tag_97q5g */)) ||
				!Code.ValidateNotNullOrWhiteSpaceArgument(
					gate.ExperimentInfo.ExperimentName,
					nameof(gate.ExperimentInfo.ExperimentName),
					TaggingUtilities.ReserveTag(0x238507c7 /* tag_97q5h */)))
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
