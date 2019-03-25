// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Gating.Experimentation
{
	/// <summary>
	/// Class that maintains a list of experiments
	/// </summary>
	public class Experiments : IExperiments
	{
		/// <summary>
		/// Loads the list of experiments
		/// </summary>
		public void Load(IDictionary<string, IExperiment> experiments)
		{
			if (!Code.ValidateArgument(experiments, nameof(experiments), TaggingUtilities.ReserveTag(0x2384e4dd /* tag_97ot3 */)))
			{
				return;
			}

			ExperimentsDictionary = experiments;
		}


		/// <summary>
		/// Is experiment valid
		/// </summary>
		/// <param name="experimentName">experiment name</param>
		/// <returns>true if the list of experiment contains the experiment</returns>
		public bool IsValidExperiment(string experimentName)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(experimentName, nameof(experimentName), TaggingUtilities.ReserveTag(0x2384f211 /* tag_97pir */)))
			{
				return false;
			}

			IDictionary<string, IExperiment> temporaryReadDictionary = ExperimentsDictionary;

			if (temporaryReadDictionary == null)
			{
				ULSLogging.LogTraceTag(0x2384e4de /* tag_97ot4 */, Categories.Experimentation, Levels.Error,
					"Experiment dictionary is being returned null");
				return false;
			}

			return temporaryReadDictionary.ContainsKey(experimentName);
		}


		/// <summary>
		/// Gets the experiment corresponding to the experimentName
		/// </summary>
		/// <param name="experimentName">The experiment name</param>
		/// <returns>the experiment</returns>
		public IExperiment GetExperiment(string experimentName)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(experimentName, nameof(experimentName), TaggingUtilities.ReserveTag(0x2384e4df /* tag_97ot5 */)))
			{
				return null;
			}

			//This is to ensure threading
			IDictionary<string, IExperiment> temporaryReadDictionary = ExperimentsDictionary;

			if (temporaryReadDictionary == null)
			{
				ULSLogging.LogTraceTag(0x2384e4e0 /* tag_97ot6 */, Categories.Experimentation, Levels.Error,
					"Experiment dictionary is being returned null");
				return null;
			}

			if (!temporaryReadDictionary.ContainsKey(experimentName))
			{
				ULSLogging.LogTraceTag(0x2384e4e1 /* tag_97ot7 */, Categories.Experimentation, Levels.Error,
					"Trying to get gates for an unknown experiment '{0}'", experimentName);
				return null;
			}

			return temporaryReadDictionary[experimentName];
		}


		#region Member variables

		/// <summary>
		/// All gates in the DataSet
		/// </summary>
		private IDictionary<string, IExperiment> ExperimentsDictionary { get; set; }

		#endregion
	}
}