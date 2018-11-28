/***************************************************************************************************
	ExperimentInfo.cs

	Class that represents the experiment info for a gate
***************************************************************************************************/

using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Gating.Experimentation
{
	/// <summary>
	/// Class that represents the experiment info for a gate
	/// </summary>
	public class ExperimentInfo : IExperimentInfo
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="experimentName">name of the experiment the gate is a part of</param>
		/// <param name="experimentWeight">weight of the gate in the experiment</param>
		public ExperimentInfo(string experimentName, uint experimentWeight)
		{
			ExperimentName = Code.ExpectsNotNullOrWhiteSpaceArgument(experimentName, nameof(experimentName), TaggingUtilities.ReserveTag(0x2384f212 /* tag_97pis */));
			ExperimentWeight = experimentWeight;
		}


		/// <summary>
		/// Name of the experiment the gate is a part of
		/// </summary>
		public string ExperimentName { get; }


		/// <summary>
		/// Weight of the gate in the experiment
		/// </summary>
		public uint ExperimentWeight { get; }
	}
}
