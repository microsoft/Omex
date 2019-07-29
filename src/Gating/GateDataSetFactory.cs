// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Gate data set factory
	/// </summary>
	public class GateDataSetFactory : IDataSetFactory<GateDataSet>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="gatesResourceName">The resource name for loading gates</param>
		/// <param name="testGroupsResourceName">The resource name for loading test groups</param>
		public GateDataSetFactory(string gatesResourceName, string testGroupsResourceName)
		{
			GatesResourceName = Code.ExpectsNotNullOrWhiteSpaceArgument(gatesResourceName, nameof(gatesResourceName), TaggingUtilities.ReserveTag(0));
			TestGroupsResourceName = Code.ExpectsNotNullOrWhiteSpaceArgument(testGroupsResourceName, nameof(testGroupsResourceName), TaggingUtilities.ReserveTag(0));
		}


		private string GatesResourceName { get; }


		private string TestGroupsResourceName { get; }


		/// <summary>
		/// Creates a new gate data set with the specified resources in the constructor.
		/// </summary>
		/// <returns>New gate data set instance</returns>
		public GateDataSet Create()
		{
			return new GateDataSet(GatesResourceName, TestGroupsResourceName);
		}
	}
}
