// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Omex.System.Caching;
using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Logging;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Gate DataSet loader
	/// </summary>
	public class GateDataSetLoader : ConfigurationDataSetLoader<GateDataSet>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GateDataSetLoader" /> class.
		/// </summary>
		/// <param name="cache">The cache object.</param>
		/// <param name="resourceMonitor">Resource monitor</param>
		/// <param name="gates">The gates.</param>
		/// <param name="testGroups">The test groups.</param>
		public GateDataSetLoader(ICache cache, IResourceMonitor resourceMonitor, IResource gates, IResource testGroups)
			: base(cache, resourceMonitor)
		{
			List<IResource> resources = new List<IResource>();
			if (gates != null)
			{
				resources.Add(gates);
			}

			if (testGroups != null)
			{
				resources.Add(testGroups);
			}

			Initialize(resources);
		}


		/// <summary>
		/// Called when the data set is loaded.
		/// </summary>
		/// <param name="fileDetails">The file details.</param>
		protected override void OnLoad(IList<ConfigurationDataSetLoadDetails> fileDetails)
		{
			ULSLogging.LogTraceTag(0x23821048 /* tag_967bi */, Categories.GateDataSet, Levels.Info,
				FormatOnLoadMessage(fileDetails));
		}


		/// <summary>
		/// Called when the data set is reloaded.
		/// </summary>
		/// <param name="oldFileDetails">The previous file details.</param>
		/// <param name="newFileDetails">The new file details.</param>
		protected override void OnReload(IList<ConfigurationDataSetLoadDetails> oldFileDetails,
			IList<ConfigurationDataSetLoadDetails> newFileDetails)
		{
			ULSLogging.LogTraceTag(0x23821049 /* tag_967bj */, Categories.GateDataSet, Levels.Info,
				FormatOnReloadMessage(oldFileDetails, newFileDetails));
		}
	}
}