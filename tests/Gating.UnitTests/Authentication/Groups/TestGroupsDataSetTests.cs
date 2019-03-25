// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#region Using statements

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Omex.Gating.Authentication.Groups;
using Microsoft.Omex.Gating.Data;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared.Configuration;
using Xunit;

#endregion

namespace Microsoft.Omex.Gating.UnitTests.Authentication.Groups
{
	/// <summary>
	/// TestGroups DataSet Unit tests class
	/// </summary>
	public class TestGroupsDataSetTests : UnitTestBase
	{
		[Fact]
		public void Load_ValidTestGroupsXml_LoadsDataSet()
		{
			TestGroupsDataSet testGroupsDataSet = new TestGroupsDataSet(TestResourceName);
			// Expecting DataSet to be loaded and no errors
			if (!testGroupsDataSet.Load(GetCorrectData()))
			{
				Assert.Empty(GetDataSetLoadingErrors(testGroupsDataSet));
			}

			Assert.True(testGroupsDataSet.IsHealthy, "DataSet reports to be healthy");
		}


		[Fact]
		public void Load_ValidTestGroupsXml_LoadsUserGroups()
		{
			TestGroupsDataSet testGroupsDataSet = new TestGroupsDataSet(TestResourceName);
			if (!testGroupsDataSet.Load(GetCorrectData()))
			{
				Assert.Empty(GetDataSetLoadingErrors(testGroupsDataSet));
			}

			//Expecting correct groups returned for emails
			Assert.True(testGroupsDataSet.GetUserGroups("test1@mydomain.xx").Count() == 3, "Expected 3 groups for test1@mydomain.xx");
			Assert.True(testGroupsDataSet.GetUserGroups("test1@mydomain.xx").Contains("Test"), "Expected groups for test1@mydomain.xx to contain Test");
			Assert.True(testGroupsDataSet.GetUserGroups("test1@mydomain.xx").Contains("Automation"), "Expected groups for test1@mydomain.xx to contain Automation");
			Assert.True(testGroupsDataSet.GetUserGroups("test1@mydomain.xx").Contains("all"), "Expected groups for test1@mydomain.xx to contain group 'all'");

			Assert.True(testGroupsDataSet.GetUserGroups("test2@mydomain.xx").Count() == 2, "Expected 2 groups for test2@mydomain.xx");
			Assert.True(testGroupsDataSet.GetUserGroups("test2@mydomain.xx").Contains("Test"), "Expected groups for test2@mydomain.xx to contain Test");
			Assert.True(testGroupsDataSet.GetUserGroups("test2@mydomain.xx").Contains("all"), "Expected groups for test2@mydomain.xx to contain group 'all'");

			Assert.True(testGroupsDataSet.GetUserGroups("test3@mydomain.xx").Count() == 2, "Expected 2 groups for test3@mydomain.xx");
			Assert.True(testGroupsDataSet.GetUserGroups("test3@mydomain.xx").Contains("Automation"), "Expected groups for test3@mydomain.xx to contain Automation");
			Assert.True(testGroupsDataSet.GetUserGroups("test3@mydomain.xx").Contains("all"), "Expected groups for test3@mydomain.xx to contain group 'all'");

			//Expecting correct users returned for groups
			Assert.True(testGroupsDataSet.GetGroupUsers("Test").Count() == 4, "Expected 4 users for Test group");
			Assert.True(testGroupsDataSet.GetGroupUsers("Test").Contains("test1@mydomain.xx", StringComparer.OrdinalIgnoreCase), "Expected test1@mydomain.xx to be in Test");
			Assert.True(testGroupsDataSet.GetGroupUsers("Test").Contains("test2@mydomain.xx", StringComparer.OrdinalIgnoreCase), "Expected test2@mydomain.xx to be in Test");

			Assert.True(testGroupsDataSet.GetGroupUsers("Automation").Count() == 2, "Expected 2 users for Automation group");
			Assert.True(testGroupsDataSet.GetGroupUsers("Automation").Contains("test1@mydomain.xx", StringComparer.OrdinalIgnoreCase), "Expected test1@mydomain.xx to be in Automation");
			Assert.True(testGroupsDataSet.GetGroupUsers("Automation").Contains("test3@mydomain.xx", StringComparer.OrdinalIgnoreCase), "Expected test3@mydomain.xx to be in Automation");

			Assert.True(testGroupsDataSet.GetGroupUsers("all").Count() == 6, "Expected 6 users for 'all' group");
			Assert.True(testGroupsDataSet.GetGroupUsers("all").Contains("test1@mydomain.xx", StringComparer.OrdinalIgnoreCase), "Expected test1@mydomain.xx to be in 'all'");
			Assert.True(testGroupsDataSet.GetGroupUsers("all").Contains("test2@mydomain.xx", StringComparer.OrdinalIgnoreCase), "Expected test2@mydomain.xx to be in 'all'");
			Assert.True(testGroupsDataSet.GetGroupUsers("all").Contains("test3@mydomain.xx", StringComparer.OrdinalIgnoreCase), "Expected test3@mydomain.xx to be in 'all'");
		}


		[Fact]
		public void GetGroupUsers_InputInDifferentCasing_ReturnsCorrectResult()
		{
			TestGroupsDataSet testGroupsDataSet = new TestGroupsDataSet(TestResourceName);

			if (!testGroupsDataSet.Load(GetCorrectData()))
			{
				Assert.True(false, GetDataSetLoadingErrors(testGroupsDataSet));
			}

			Assert.True(testGroupsDataSet.GetGroupUsers("AutomatION").Count() == 2, "Expected GetGroupUsers search to be case insensitive");
		}


		[Fact]
		public void GetUserGroups_InputInDifferentCasing_ReturnsCorrectResult()
		{
			TestGroupsDataSet testGroupsDataSet = new TestGroupsDataSet(TestResourceName);

			if (!testGroupsDataSet.Load(GetCorrectData()))
			{
				Assert.True(false, GetDataSetLoadingErrors(testGroupsDataSet));
			}

			Assert.True(testGroupsDataSet.GetUserGroups("TesT1@mydomain.xx").Count() == 3, "Expected GetUserGroups search to be case insensitive");
		}


		[Fact]
		public void GetUserGroups_WhenUserNotInDataSet_ReturnsDefaultGroup()
		{
			TestGroupsDataSet loadedDataSet = new TestGroupsDataSet(TestResourceName);
			loadedDataSet.Load(GetCorrectData());

			// Expecting a user to be in 'all' group
			Assert.True(loadedDataSet.GetUserGroups("randomuser@mydomain.xx").Count() == 1, "Expected 1 group for randomuser@mydomain.xx");
			Assert.True(loadedDataSet.GetUserGroups("randomuser@mydomain.xx").Contains("all"), "Expected groups for randomuser@mydomain.xx to contain group 'all'");
		}


		[Fact]
		public void GetUserGroups_WhenDataSetIsEmpty_ReturnsDefaultGroup()
		{
			FailOnErrors = false;

			TestGroupsDataSet emptyDataSet = new TestGroupsDataSet(TestResourceName);
			emptyDataSet.Load(GetIncorrectData());

			// Expecting a user to be in 'all' group in empty DataSet
			Assert.True(emptyDataSet.GetUserGroups("randomuser@mydomain.xx").Count() == 1, "Expected 1 group for randomuser@mydomain.xx");
			Assert.True(emptyDataSet.GetUserGroups("randomuser@mydomain.xx").Contains("all"), "Expected groups for randomuser@mydomain.xx to contain group 'all'");
		}


		[Fact]
		public void Load_IncorrectTestGroupsXml_FailsToDeserialise() => LoadDataSetWithOneError(IncorrectDataRaw);


		[Fact]
		public void Load_GroupWithNoMembers_LoadsWithError() => LoadDataSetWithOneError(IncorrectDataNoMembers);


		[Fact]
		public void Load_EmptyMemberEmail_LoadsWithError() => LoadDataSetWithOneError(IncorrectDataEmptyEmail);


		[Fact]
		public void Load_EmptyMemberDeploymentId_LoadsWithError() => LoadDataSetWithOneError(IncorrectDataEmptyDeploymentId);


		[Fact]
		public void Load_GroupWithEmptyName_LoadsWithError() => LoadDataSetWithOneError(IncorrectDataEmptyGroupName);


		[Fact]
		public void Load_GroupWithSemicolonInName_LoadsWithError() => LoadDataSetWithOneError(IncorrectDataSemicolonInGroupName);


		[Fact]
		public void Load_GroupWithReservedName_LoadsWithError() => LoadDataSetWithOneError(IncorrectDataReservedGroupName);


		[Fact]
		public void Load_EmptyXml_FailsToDeserialise() => LoadDataSetWithOneError(string.Empty);


		/// <summary>
		/// Verifies that DataSet loads with one parsing error from given resource
		/// </summary>
		/// <param name="resourceString">resource content as a string</param>
		private void LoadDataSetWithOneError(string resourceString)
		{
			FailOnErrors = false;

			IDictionary<string, IResourceDetails> resources =
				new Dictionary<string, IResourceDetails>(1, StringComparer.OrdinalIgnoreCase)
			{
				{ TestResourceName, EmbeddedResources.CreateResourceDetails(resourceString) }
			};

			TestGroupsDataSet testGroupsDataSet = new TestGroupsDataSet(TestResourceName);

			// Expecting DataSet load to return false
			Assert.False(testGroupsDataSet.Load(resources), "Expecting DataSet load to return false.");
			// Expecting DataSet to be loaded
			Assert.False(testGroupsDataSet.IsHealthy, "Expecting DataSet to be not healthy.");
			// Expecting one error
			Assert.Equal(1, testGroupsDataSet.Errors.Count);
		}


		/// <summary>
		/// Merges all load errors into one string for easier unit test output
		/// </summary>
		/// <param name="dataSet">Test Groups DataSet</param>
		/// <returns>String with all loading errors</returns>
		private static string GetDataSetLoadingErrors(TestGroupsDataSet dataSet) => GetDataSetLoadingErrors(dataSet.Errors);


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
		/// Test resource name
		/// </summary>
		private const string TestResourceName = ResourceNames.TestGroups;


		/// <summary>
		/// Xml Data to be loaded into DataSet for testing - incorrect data
		/// </summary>
		/// <returns>file resource</returns>
		private static IDictionary<string, IResourceDetails> GetIncorrectData()
		{
			return new Dictionary<string, IResourceDetails>(1, StringComparer.OrdinalIgnoreCase)
			{
				{ TestResourceName, EmbeddedResources.CreateResourceDetails(IncorrectDataRaw) }
			};
		}


		/// <summary>
		/// Xml Data to be loaded into DataSet for testing - correct data
		/// </summary>
		/// <returns>file resource</returns>
		private static IDictionary<string, IResourceDetails> GetCorrectData()
		{
			return new Dictionary<string, IResourceDetails>(1, StringComparer.OrdinalIgnoreCase)
			{
				{ TestResourceName, EmbeddedResources.CreateResourceDetails(ValidDataRaw) }
			};
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


		/// <summary>
		/// Incorrect test groups xml data - no members in Test group
		/// </summary>
		private const string IncorrectDataNoMembers =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestGroups xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<TestGroup name=""Test"">
	</TestGroup>
	<TestGroup name=""Automation"">
		<Member email=""test3@mydomain.xx"" alias=""myalias""/>
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
	</TestGroup>
</TestGroups>";


		/// <summary>
		/// Incorrect test groups xml data - empty email
		/// </summary>
		private const string IncorrectDataEmptyEmail =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestGroups xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<TestGroup name=""Automation"">
		<Member email="""" alias=""myalias""/>
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
	</TestGroup>
</TestGroups>";


		/// <summary>
		/// Incorrect test groups xml data - empty deployment id
		/// </summary>
		private const string IncorrectDataEmptyDeploymentId =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestGroups xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<TestGroup name=""Automation"">
		<Member deploymentId=""   ""/>
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
	</TestGroup>
</TestGroups>";


		/// <summary>
		/// Incorrect test groups xml data - empty group name
		/// </summary>
		private const string IncorrectDataEmptyGroupName =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestGroups xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<TestGroup name=""   "">
		<Member email=""test3@mydomain.xx"" alias=""myalias""/>
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
	</TestGroup>
</TestGroups>";


		/// <summary>
		/// Incorrect test groups XML data, featuring a group name containing a semicolon.
		/// </summary>
		private const string IncorrectDataSemicolonInGroupName =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestGroups xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<TestGroup name=""Test;Automation"">
		<Member email=""test3@mydomain.xx"" alias=""myalias""/>
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
	</TestGroup>
</TestGroups>";


		/// <summary>
		/// Incorrect test groups XML data, featuring a reserved group name.
		/// </summary>
		private const string IncorrectDataReservedGroupName =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestGroups xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<TestGroup name=""all"">
		<Member email=""test3@mydomain.xx"" alias=""myalias""/>
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
	</TestGroup>
</TestGroups>";


		/// <summary>
		/// Incorrect test groups xml data, closing bracket is missing after ""Test""
		/// </summary>
		private const string IncorrectDataRaw =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestGroups xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<TestGroup name=""Test""
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
		<Member email=""test2@mydomain.xx"" alias=""myalias""/>
	</TestGroup>
	<TestGroup name=""Automation"">
		<Member email=""test3@mydomain.xx"" alias=""myalias""/>
	</TestGroup>
</TestGroups>";
	}
}