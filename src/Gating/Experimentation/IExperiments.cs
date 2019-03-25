// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.Gating.Experimentation
{
	/// <summary>
	/// Maintains a list of experiments
	/// </summary>
	public interface IExperiments
	{
		/// <summary>
		/// Loads the list of experiments
		/// </summary>
		void Load(IDictionary<string, IExperiment> experiments);


		/// <summary>
		/// Is experiment valid
		/// </summary>
		/// <param name="experimentName">experiment name</param>
		/// <returns>true if the list of experiment contains the experiment</returns>
		bool IsValidExperiment(string experimentName);


		/// <summary>
		/// Gets the experiment corresponding to the experimentName
		/// </summary>
		/// <param name="experimentName">The experiment name</param>
		/// <returns>the experiment</returns>
		IExperiment GetExperiment(string experimentName);
	}
}