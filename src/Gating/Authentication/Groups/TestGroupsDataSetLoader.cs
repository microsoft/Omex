/***************************************************************************
	TestGroupsDataSetLoader.cs

	TestGroups DataSet Loader
***************************************************************************/

#region Using directives

using System.Collections.Generic;
using Microsoft.Omex.System.Caching;
using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Logging;

#endregion

namespace Microsoft.Omex.Gating.Authentication.Groups
{
	/// <summary>
	/// TestGroups DataSet Loader
	/// </summary>
	public class TestGroupsDataSetLoader<T> : ConfigurationDataSetLoader<T> where T : class, ITestGroupsDataSet, new()
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="cache">The cache object.</param>
		/// <param name="resourceMonitor">Resource monitor</param>
		/// <param name="testGroups">The test groups resource.</param>
		public TestGroupsDataSetLoader(ICache cache, IResourceMonitor resourceMonitor, IResource testGroups)
			: base(cache, resourceMonitor)
		{
			Initialize(new List<IResource> { testGroups });
		}


		/// <summary>
		/// Called when the data set is loaded.
		/// </summary>
		/// <param name="fileDetails">The file details.</param>
		protected override void OnLoad(IList<ConfigurationDataSetLoadDetails> fileDetails)
		{
			ULSLogging.LogTraceTag(0x238503c4 /* tag_97qpe */, Categories.TestGroupsDataSet, Levels.Info,
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
			ULSLogging.LogTraceTag(0x238503c5 /* tag_97qpf */, Categories.TestGroupsDataSet, Levels.Info,
				FormatOnReloadMessage(oldFileDetails, newFileDetails));
		}
	}


	/// <summary>
	/// TestGroups DataSet Loader
	/// </summary>
	public class TestGroupsDataSetLoader : TestGroupsDataSetLoader<TestGroupsDataSet>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestGroupsDataSetLoader"/> class.
		/// </summary>
		/// <param name="cache">The cache object.</param>
		/// <param name="resourceMonitor">The resource monitor.</param>
		/// <param name="testGroups">The test groups.</param>
		public TestGroupsDataSetLoader(ICache cache, IResourceMonitor resourceMonitor, IResource testGroups)
			: base(cache, resourceMonitor, testGroups)
		{
		}
	}
}
