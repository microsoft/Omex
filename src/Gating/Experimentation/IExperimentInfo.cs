﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Gating.Experimentation
{
	/// <summary>
	/// Interface that represents the experiment info for a gate
	/// </summary>
	public interface IExperimentInfo
	{
		/// <summary>
		/// Name of the experiment the gate is a part of
		/// </summary>
		string ExperimentName { get; }

		/// <summary>
		/// Weight of the gate in the experiment
		/// </summary>
		uint ExperimentWeight { get; }
	}
}
