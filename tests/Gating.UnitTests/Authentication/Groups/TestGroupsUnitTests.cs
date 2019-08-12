// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#region Using statements

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Omex.Gating.Authentication.Groups;
using Microsoft.Omex.Gating.Data;
using Microsoft.Omex.System.Caching;
using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Data.FileSystem;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared.Configuration.DataSets;
using Microsoft.Omex.System.UnitTests.Shared.Data;
using Microsoft.Omex.System.UnitTests.Shared.Data.FileSystem;
using Moq;
using Xunit;

#endregion

namespace Microsoft.Omex.Gating.UnitTests
{
	/// <summary>
	/// TestGroups Unit tests class
	/// </summary>
	public class TestGroupsUnitTests : UnitTestBase
	{
		[Fact]
		public void IsDefault_GroupNamesDoNotContainDefault_ReturnsFalse()
		{
			using (IConfigurationDataSetLoader<TestGroupsDataSet> loader =
				new UnitTestTestGroupsDataSetLoader(new UnitTestResourceMonitor()))
			{
				CheckTestGroupsDataSetLoader(loader);
				TestGroups testGroups = new TestGroups(loader);
				Assert.False(testGroups.IsDefault("group1;group2;group3;"),
					"IsDefault returns false if test groups do not contain default group.");
				Assert.False(testGroups.IsDefault("groupallgroup;allgroup;groupall;"),
					"IsDefault returns false if test groups contain 'all' as substring.");
			}
		}


		[Fact]
		public void IsDefault_GroupNamesContainDefault_ReturnsTrue()
		{
			using (IConfigurationDataSetLoader<TestGroupsDataSet> loader =
				new UnitTestTestGroupsDataSetLoader(new UnitTestResourceMonitor()))
			{
				CheckTestGroupsDataSetLoader(loader);
				TestGroups testGroups = new TestGroups(loader);
				Assert.True(testGroups.IsDefault("all;group2;group3;"),
					"IsDefault returns true if test groups contain default group in the beginning.");
				Assert.True(testGroups.IsDefault("group1;group2;all;"),
					"IsDefault returns true if test groups contain default group in the end followed by delimiter.");
				Assert.True(testGroups.IsDefault("group1;group2;all"),
					"IsDefault returns true if test groups contain default group in the end not followed by delimiter.");
				Assert.True(testGroups.IsDefault("group1;all;group3"),
					"IsDefault returns true if test groups contain default group in the middle.");
			}
		}


		[Fact]
		public void GetUserGroups_TestGroupsDataSetIsNull_ReturnsEmptyListAndLogs()
		{
			FailOnErrors = false;

			Mock<IDataSetFactory<TestGroupsDataSet>> mockFactory = new Mock<IDataSetFactory<TestGroupsDataSet>>();
			mockFactory.Setup(i => i.Create()).Returns(new TestGroupsDataSet());

			using (IConfigurationDataSetLoader<TestGroupsDataSet> loader =
				new UnitTestDataSetLoader<TestGroupsDataSet>(new LocalCache(), new UnitTestResourceMonitor(), mockFactory.Object))
			{
				TestGroups testGroups = new TestGroups(loader);

				LoggedEvents.Clear();
				Assert.False(testGroups.GetUserGroups("test1@mydomain.xx").Any(),
					"Returns empty list if currently loaded TestGroupsDataSet is null");
				IEnumerable<LogEventArgs> events = LoggedEvents;

				Assert.NotNull(events);
				Assert.Equal(1, events.Count(e => e.Level == Levels.Error));
				Assert.Equal(1, events.Count(e => e.Level == Levels.Warning));
			}
		}


		[Fact]
		public void GetDeploymentIdGroups_IdNotInDataSet_ReturnsDefaultGroups()
		{
			using (IConfigurationDataSetLoader<TestGroupsDataSet> loader =
				new UnitTestTestGroupsDataSetLoader(new UnitTestResourceMonitor()))
			{
				CheckTestGroupsDataSetLoader(loader);
				TestGroups testGroups = new TestGroups(loader);

				Assert.True(
					testGroups.GetDeploymentIdGroups(DeploymentIdNotInDataSet).Intersect(loader.LoadedDataSet.DefaultGroups).Count() ==
					loader.LoadedDataSet.DefaultGroups.Count(),
					"Returns default access groups list when deploymentId is not in DataSet.");
			}
		}


		[Fact]
		public void GetDeploymentIdGroups_IdInDataSet_ReturnsExpectedGroups()
		{
			using (IConfigurationDataSetLoader<TestGroupsDataSet> loader =
				new UnitTestTestGroupsDataSetLoader(new UnitTestResourceMonitor()))
			{
				CheckTestGroupsDataSetLoader(loader);
				TestGroups testGroups = new TestGroups(loader);

				List<string> userGroups = testGroups.GetDeploymentIdGroups(DeploymentId).ToList();
				Assert.Equal(3, userGroups.Count);
				Assert.True(userGroups.Contains("Test"), "Groups contain Test");
				Assert.True(userGroups.Contains("Probes"), "Groups contain Probes");
				Assert.True(userGroups.Contains("all"), "Groups contain group 'all'");
			}
		}


		[Fact]
		public void GetGroupUsers_TestGroupsDataSetIsNull_ReturnsEmptyListAndLogs()
		{
			FailOnErrors = false;

			Mock<IDataSetFactory<TestGroupsDataSet>> mockFactory = new Mock<IDataSetFactory<TestGroupsDataSet>>();
			mockFactory.Setup(i => i.Create()).Returns(new TestGroupsDataSet());

			using (IConfigurationDataSetLoader<TestGroupsDataSet> loader =
				new UnitTestDataSetLoader<TestGroupsDataSet>(new LocalCache(), new UnitTestResourceMonitor(), mockFactory.Object))
			{
				TestGroups testGroups = new TestGroups(loader);

				LoggedEvents.Clear();
				Assert.False(testGroups.GetGroupUsers("Automation").Any(),
					"Returns empty list if currently loaded TestGroupsDataSet is null");
				IEnumerable<LogEventArgs> events = LoggedEvents;

				Assert.NotNull(events);
				Assert.Equal(1, events.Count(e => e.Level == Levels.Error));
				Assert.Equal(1, events.Count(e => e.Level == Levels.Warning));
			}
		}


		[Fact]
		public void GetGroupUsers_GroupNotInDataSet_ReturnsEmptyList()
		{
			using (IConfigurationDataSetLoader<TestGroupsDataSet> loader =
				new UnitTestTestGroupsDataSetLoader(new UnitTestResourceMonitor()))
			{
				CheckTestGroupsDataSetLoader(loader);
				TestGroups testGroups = new TestGroups(loader);

				Assert.False(testGroups.GetGroupUsers("GroupNotInTestGroups").Any(),
					"Returns empty list when group is not in DataSet.");
			}
		}


		[Fact]
		public void GetGroupUsers_GroupInDataSet_ReturnsUsers()
		{
			using (IConfigurationDataSetLoader<TestGroupsDataSet> loader =
				new UnitTestTestGroupsDataSetLoader(new UnitTestResourceMonitor()))
			{
				CheckTestGroupsDataSetLoader(loader);
				TestGroups testGroups = new TestGroups(loader);

				List<string> groupUsers = testGroups.GetGroupUsers("Automation").ToList();
				Assert.True(groupUsers.Count == 2, "Returns 2 users for Automation group.");
				Assert.True(groupUsers.Contains("test3@mydomain.xx"), "Group contains test3@mydomain.xx");
				Assert.True(groupUsers.Contains("test1@mydomain.xx"), "Group contains test1@mydomain.xx");
			}
		}


		[Fact]
		public void IsUserInGroups_InputInDifferentCasing_ReturnsCorrectResult()
		{
			using (IConfigurationDataSetLoader<TestGroupsDataSet> loader =
				new UnitTestTestGroupsDataSetLoader(new UnitTestResourceMonitor()))
			{
				CheckTestGroupsDataSetLoader(loader);
				TestGroups testGroups = new TestGroups(loader);

				Assert.True(testGroups.IsUserInGroups("TesT1@mydomain.xx", "aUTOMATION"),
					"Expected IsUserInGroups search to be case insensitive");
			}
		}


		/// <summary>
		/// Merges all load errors into one string for easier unit test output
		/// </summary>
		/// <param name="errors">The list of errors from the test.</param>
		/// <returns>String with all loading errors</returns>
		private static string GetDataSetLoadingErrors(IList<string> errors)
		{
			if (errors == null || errors.Count == 0)
			{
				return "No errors reported.";
			}

			StringBuilder stringBuilder = new StringBuilder(500);
			foreach (string error in errors)
			{
				stringBuilder.AppendLine(error);
			}

			return stringBuilder.ToString();
		}


		/// <summary>
		/// Test resource folder
		/// </summary>
		private const string TestResourceFolder = "TestFolder";


		/// <summary>
		/// Test resource name
		/// </summary>
		private const string TestResourceName = ResourceNames.TestGroups;


		/// <summary>
		/// Deployment id
		/// </summary>
		private const string DeploymentId = "{3D5ABF05-6148-4594-9BBE-A3CB2A34EBF8}";


		/// <summary>
		/// Deployment id not in DataSet
		/// </summary>
		private const string DeploymentIdNotInDataSet = "{3D5ABF05-6148-4594-9BBE-A3CB2A34E000}";


		/// <summary>
		/// Checks the test groups data set loader
		/// </summary>
		/// <param name="loader">Test groups data set loader</param>
		private void CheckTestGroupsDataSetLoader(IConfigurationDataSetLoader<TestGroupsDataSet> loader)
		{
			Assert.NotNull(loader.LoadedDataSet);
			Assert.True(loader.LoadedDataSet.IsHealthy, GetDataSetLoadingErrors(loader.LoadedDataSet.Errors));
		}


		/// <summary>
		/// TestGroups DataSet Loader
		/// </summary>
		public class UnitTestTestGroupsDataSetLoader : ConfigurationDataSetLoader<TestGroupsDataSet>
		{
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="resourceMonitor">Resource monitor</param>
			/// <param name="dataSet">data set</param>
			public UnitTestTestGroupsDataSetLoader(IResourceMonitor resourceMonitor, TestGroupsDataSet dataSet = null)
				: base(new LocalCache(), resourceMonitor, TestGateDataSetDefaultFactory, dataSet ?? new TestGroupsDataSet(ResourceNames.TestGroups))
			{
				Initialize(new List<IResource>
								{
									new FileResource(new UnitTestTextFile(true, true, ValidDataRaw),
										TestResourceFolder, TestResourceName)
								});
			}


			/// <summary>
			/// Called when the data set is loaded.
			/// </summary>
			/// <param name="fileDetails">The file details.</param>
			protected override void OnLoad(IList<ConfigurationDataSetLoadDetails> fileDetails)
			{
			}


			/// <summary>
			/// Called when the data set is reloaded.
			/// </summary>
			/// <param name="oldFileDetails">The previous file details.</param>
			/// <param name="newFileDetails">The new file details.</param>
			protected override void OnReload(IList<ConfigurationDataSetLoadDetails> oldFileDetails,
				IList<ConfigurationDataSetLoadDetails> newFileDetails)
			{
			}


			private static IDataSetFactory<TestGroupsDataSet> TestGateDataSetDefaultFactory
			{
				get
				{
					Mock<IDataSetFactory<TestGroupsDataSet>> mockFactory = new Mock<IDataSetFactory<TestGroupsDataSet>>();
					mockFactory.Setup(i => i.Create()).Returns(new TestGroupsDataSet());
					return mockFactory.Object;
				}
			}
	}


		/// <summary>
		/// Valid test groups xml data
		/// </summary>
		private const string ValidDataRaw =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestGroups xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<TestGroup name=""Test"">
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
		<Member email=""test2@mydomain.xx"" alias=""myalias""/>
		<Member deploymentId=""{E5C91678-399E-46ef-B5BF-D619AEACD050}"" alias=""myalias""/>
		<Member deploymentId=""{3D5ABF05-6148-4594-9BBE-A3CB2A34EBF8}"" alias=""myalias""/>
	</TestGroup>
	<TestGroup name=""Automation"">
		<Member email=""test3@mydomain.xx"" alias=""myalias""/>
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
	</TestGroup>
	<TestGroup name=""Probes"">
		<Member deploymentId=""{3D5ABF05-6148-4594-9BBE-A3CB2A34EBF8}"" alias=""myalias""/>
		<Member deploymentId=""{8499ED16-258C-49d4-A1E0-C9E1B1460FDC}"" alias=""myalias""/>
	</TestGroup>
</TestGroups>";
	}
}