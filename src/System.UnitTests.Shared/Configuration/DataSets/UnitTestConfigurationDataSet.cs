// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#region Using directives

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Data;

#endregion

namespace Microsoft.Omex.System.UnitTests.Shared.Configuration.DataSets
{
	/// <summary>
	/// Unit Tests ConfigurationDataSet
	/// </summary>
	public class UnitTestConfigurationDataSet : IConfigurationDataSet
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UnitTestConfigurationDataSet"/> class.
		/// </summary>
		public UnitTestConfigurationDataSet() => IsHealthy = true;


		/// <summary>
		/// Is DataSet loaded successfully
		/// </summary>
		public bool IsHealthy { get; set; }


		/// <summary>
		/// Last reload DateTime
		/// </summary>
		public DateTime LastReload { get; set; }


		/// <summary>
		/// Gets the details.
		/// </summary>
		public IList<ConfigurationDataSetLoadDetails> LoadDetails { get; private set; }


		/// <summary>
		/// Loading errors
		/// </summary>
		public IList<string> Errors { get; private set; }


		/// <summary>
		/// Loads the DataSet
		/// </summary>
		/// <param name="resources">Resources to use for loading</param>
		/// <returns>true if load was successful, false otherwise</returns>
		public bool Load(IDictionary<string, IResourceDetails> resources)
		{
			// Needed as in unit tests loading and reloading of datasets can happen within one tick
			Thread.Sleep(1);

			LastReload = DateTime.UtcNow;
			LoadDetails = new List<ConfigurationDataSetLoadDetails>();

			foreach (KeyValuePair<string, IResourceDetails> resource in resources)
			{
				LoadDetails.Add(new ConfigurationDataSetLoadDetails(resource.Key, DateTime.UtcNow, 10, OmexSystemUnsecureKeys.ConfigurationDataSetFileSha));
			}

			return IsHealthy;
		}
	}
}