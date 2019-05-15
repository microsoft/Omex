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
			if (!Code.ValidateArgument(experiments, nameof(experiments), TaggingUtilities.ReserveTag(0x23821082 /* tag_967cc */)))
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
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(experimentName, nameof(experimentName), TaggingUtilities.ReserveTag(0x23821083 /* tag_967cd */)))
			{
				return false;
			}

			IDictionary<string, IExperiment> temporaryReadDictionary = ExperimentsDictionary;

			if (temporaryReadDictionary == null)
			{
				ULSLogging.LogTraceTag(0x23821084 /* tag_967ce */, Categories.Experimentation, Levels.Error,
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
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(experimentName, nameof(experimentName), TaggingUtilities.ReserveTag(0x23821085 /* tag_967cf */)))
			{
				return null;
			}

			//This is to ensure threading
			IDictionary<string, IExperiment> temporaryReadDictionary = ExperimentsDictionary;

			if (temporaryReadDictionary == null)
			{
				ULSLogging.LogTraceTag(0x23821086 /* tag_967cg */, Categories.Experimentation, Levels.Error,
					"Experiment dictionary is being returned null");
				return null;
			}

			if (!temporaryReadDictionary.ContainsKey(experimentName))
			{
				ULSLogging.LogTraceTag(0x23821087 /* tag_967ch */, Categories.Experimentation, Levels.Error,
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