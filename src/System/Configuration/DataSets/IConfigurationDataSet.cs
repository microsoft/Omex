// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Data;

namespace Microsoft.Omex.System.Configuration.DataSets
{
	/// <summary>
	/// Interface for Configuration DataSet
	/// </summary>
	public interface IConfigurationDataSet
	{
		/// <summary>
		/// Is DataSet loaded successfully
		/// </summary>
		bool IsHealthy { get; }

		/// <summary>
		/// Last reload DateTime
		/// </summary>
		DateTime LastReload { get; }

		/// <summary>
		/// Gets the details.
		/// </summary>
		IList<ConfigurationDataSetLoadDetails> LoadDetails { get; }

		/// <summary>
		/// Loading errors
		/// </summary>
		IList<string> Errors { get; }

		/// <summary>
		/// Loads the DataSet
		/// </summary>
		/// <param name="resources">Resources to use for loading</param>
		/// <returns>true if load was successful, false otherwise</returns>
		bool Load(IDictionary<string, IResourceDetails> resources);
	}
}
