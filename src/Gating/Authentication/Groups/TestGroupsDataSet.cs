// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Omex.Gating.Data;
using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Extensions;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;
using Configuration = Gating.TestGroups.Configuration;

namespace Microsoft.Omex.Gating.Authentication.Groups
{
	/// <summary>
	/// Test groups data set
	/// </summary>
	public class TestGroupsDataSet<T> : ConfigurationDataSet<T>, ITestGroupsDataSet where T : ConfigurationDataSetLogging, new()
	{
		/// <summary>
		/// The Test Groups resource name
		/// </summary>
		private string TestGroupsResourceName { get; }


		/// <summary>
		/// Constructor
		/// </summary>
		public TestGroupsDataSet()
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="TestGroupsDataSet"/> class.
		/// </summary>
		/// <param name="testGroupsResourceName">Name of the Test Groups resource.</param>
		public TestGroupsDataSet(string testGroupsResourceName)
		{
			TestGroupsResourceName = Code.ExpectsNotNullOrWhiteSpaceArgument(testGroupsResourceName, nameof(testGroupsResourceName), TaggingUtilities.ReserveTag(0));

			DefaultGroups = new List<string> { "all" };
		}


		/// <summary>
		/// Load the DataSet
		/// </summary>
		/// <param name="resources">Resources to load the DataSet from</param>
		/// <returns>true if load was successful, false otherwise</returns>
		public override bool Load(IDictionary<string, IResourceDetails> resources)
		{
			if (!base.Load(resources))
			{
				return false;
			}

			bool resourceStatus = LoadResource(resources, TestGroupsResourceName, out byte[] file);
			if (!resourceStatus)
			{
				ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
					true, "Failed to retrieve resource content for TestGroupsDataSet.");
			}
			else if (file == null || file.Length == 0)
			{
				ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
					true, "Null or empty resource data encountered in TestGroupsDataSet loading method");
			}
			else
			{
				Load(file);
			}

			LastReload = DateTime.UtcNow;
			return IsHealthy;
		}


		/// <summary>
		/// Retrieves all groups for a given group email or deployment id
		/// </summary>
		/// <param name="user">User email or deployment id</param>
		/// <returns>List of group groups</returns>
		public IEnumerable<string> GetUserGroups(string user)
		{
			if (m_userGroups == null)
			{
				ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
					false, "Test groups DataSet is not loaded.");
				return DefaultGroups;
			}

			if (m_userGroups.TryGetValue(user.Trim(), out List<string> userGroups))
			{
				return userGroups;
			}
			else
			{
				return DefaultGroups;
			}
		}


		/// <summary>
		/// Get group users (email or deployment identifiers)
		/// </summary>
		/// <param name="groupName">Group name</param>
		/// <returns>List of user group emails or deployment ids</returns>
		public IEnumerable<string> GetGroupUsers(string groupName)
		{
			List<string> emptyList = new List<string>();
			if (string.IsNullOrWhiteSpace(groupName))
			{
				ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
					false, "Null or empty group passed to TestGroupsDataSet.GetGroupUsers");
				return emptyList;
			}

			if (m_groupUsers == null)
			{
				ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
					false, "TestGroupsDataSet is not loaded.");
				return emptyList;
			}

			string group = groupName.Trim();
			if (DefaultGroups.Contains(group))
			{
				return m_userGroups.Keys.ToList();
			}

			if (m_groupUsers.TryGetValue(group, out List<string> groupUsers))
			{
				return groupUsers;
			}

			return emptyList;
		}


		/// <summary>
		/// DataSet member data (group groups)
		/// </summary>
		private Dictionary<string, List<string>> m_userGroups;


		/// <summary>
		/// DataSet member data (user users)
		/// </summary>
		private Dictionary<string, List<string>> m_groupUsers;


		/// <summary>
		/// The name of the TestGroups Schema
		/// </summary>
		private const string GroupsSchema = ResourceNames.TestGroupsConfiguration;


		/// <summary>
		/// Load the test groups DataSet
		/// </summary>
		/// <param name="file">file to load the groups DataSet from</param>
		private void Load(byte[] file)
		{
			try
			{
				Configuration.TestGroups testGroups;
				using (MemoryStream stream = new MemoryStream(file, false))
				{
					testGroups = stream.Read<Configuration.TestGroups>(GroupsSchema, null);
				}

				LoadGroupsData(testGroups, out m_userGroups, out m_groupUsers);
			}
			catch (InvalidOperationException exception)
			{
				ULSLogging.ReportExceptionTag(0, Categories.TestGroupsDataSet, exception,
					true, "Failed to load test groups DataSet due to exception.");
			}
		}


		/// <summary>
		/// Load the Test Groups Data
		/// </summary>
		/// <param name="testGroupsRaw">Raw test groups data</param>
		/// <param name="userGroups">User groups</param>
		/// <param name="groupUsers">Group users</param>
		private void LoadGroupsData(Configuration.TestGroups testGroupsRaw,
			out Dictionary<string, List<string>> userGroups, out Dictionary<string, List<string>> groupUsers)
		{
			if (testGroupsRaw == null || testGroupsRaw.TestGroup == null)
			{
				ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
					true, "Null raw data has been passed to TestGroupsDataSet.LoadGroupsData.");
				userGroups = null;
				groupUsers = null;
				return;
			}

			userGroups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			groupUsers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

			foreach (Configuration.TestGroupsTestGroup testGroup in testGroupsRaw.TestGroup)
			{
				string testGroupName = testGroup.name.Trim();
				if (string.IsNullOrWhiteSpace(testGroupName))
				{
					ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
						true, "A test user with empty name found, skipping.");
					continue;
				}

				if (testGroupName.Contains(ListSeparator))
				{
					ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
						true, "Test user name '{0}' contains a semicolon. This name cannot be used as the semicolon is used to delimit groups within the asset field. Skipping.",
							testGroupName);
					continue;
				}

				if (DefaultGroups.Contains(testGroupName))
				{
					ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
						true, "Test user name '{0}' is a restricted user name and cannot be manually specified. Skipping.", testGroupName);
					continue;
				}

				if (testGroup.Member == null)
				{
					ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
						true, "Skipping test user '{0}' : the user has no members", testGroupName);
					continue;
				}

				if (!groupUsers.ContainsKey(testGroupName))
				{
					groupUsers.Add(testGroupName, new List<string>());
				}

				foreach (Configuration.TestGroupsTestGroupMember member in testGroup.Member)
				{
					if (member.email != null)
					{
						UpdateGroups(userGroups, groupUsers, member.email, testGroupName);
					}

					if (member.deploymentId != null)
					{
						UpdateGroups(userGroups, groupUsers, member.deploymentId, testGroupName);
					}
				}
			}
		}


		/// <summary>
		/// Connects user to a group
		/// </summary>
		/// <param name="userGroups">User groups dictionary</param>
		/// <param name="groupUsers">Group users dictionary</param>
		/// <param name="user">group</param>
		/// <param name="groupName">Group name to add</param>
		private void UpdateGroups(Dictionary<string, List<string>> userGroups, Dictionary<string, List<string>> groupUsers, string user, string groupName)
		{
			if (!string.IsNullOrWhiteSpace(user))
			{
				user = user.Trim();
				// Connect group to user
				if (userGroups.TryGetValue(user, out List<string> groups))
				{
					if (!groups.Contains(groupName))
					{
						groups.Add(groupName);
					}
				}
				else
				{
					groups = new List<string>(DefaultGroups)
					{
						groupName
					};
					userGroups.Add(user, groups);
				}

				// Connect user to group
				if (groupUsers.TryGetValue(groupName, out List<string> users))
				{
					if (!users.Contains(user))
					{
						users.Add(user);
					}
				}
				else
				{
					users = new List<string> { user };
					groupUsers.Add(groupName, users);
				}
			}
			else
			{
				ULSLogging.LogCodeErrorTag(0, Categories.TestGroupsDataSet, false,
					true, "Empty user data found for group '{0}', skipping.", groupName);
			}
		}


		/// <summary>
		/// User groups any group belongs to
		/// </summary>
		public IEnumerable<string> DefaultGroups { get; }


		/// <summary>
		/// The character separating elements in a list stored within an asset field.
		/// </summary>
		private const string ListSeparator = ";";
	}


	/// <summary>
	/// Test groups data set
	/// </summary>
	public class TestGroupsDataSet : TestGroupsDataSet<ConfigurationDataSetLogging>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestGroupsDataSet"/> class.
		/// </summary>
		public TestGroupsDataSet()
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="TestGroupsDataSet"/> class.
		/// </summary>
		/// <param name="testGroupsResourceName">Name of the test groups resource.</param>
		public TestGroupsDataSet(string testGroupsResourceName)
			: base(testGroupsResourceName)
		{
		}
	}
}