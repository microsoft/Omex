// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Omex.System.Authentication;
using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Gating.Authentication.Groups
{
	/// <summary>
	/// Test groups based access validation
	/// </summary>
	public class TestGroups : ITestGroups
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="testGroupsLoader">TestGroups DataSet loader</param>
		public TestGroups(IConfigurationDataSetLoader<ITestGroupsDataSet> testGroupsLoader)
		{
			m_testGroupsDataSetLoader = Code.ExpectsArgument(testGroupsLoader, nameof(testGroupsLoader), TaggingUtilities.ReserveTag(0x2380b693 /* tag_96l0t */));
		}


		/// <summary>
		/// Is one or more of the groups specified equal to a default?
		/// </summary>
		/// <param name="groupNames">A semicolon-delimited list of group names. This value cannot be null.</param>
		/// <returns>True if one or more of the groups specified is equal to a default; false
		/// otherwise.</returns>
		public bool IsDefault(string groupNames)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(groupNames, nameof(groupNames), TaggingUtilities.ReserveTag(0x2382109c /* tag_967c2 */)))
			{
				return false;
			}

			ITestGroupsDataSet testGroupsDataSet = GetTestGroupsDataSet();
			if (testGroupsDataSet == null)
			{
				return false;
			}

			IEnumerable<string> groupNamesList = groupNames.Split(s_listSeparatorArray, StringSplitOptions.RemoveEmptyEntries);
			if (groupNamesList.Intersect(testGroupsDataSet.DefaultGroups, StringComparer.OrdinalIgnoreCase).Any())
			{
				return true;
			}

			return false;
		}


		/// <summary>
		/// Retrieves all groups for the currently logged-in user.
		/// </summary>
		/// <param name="identity">The identity of the logged in user. This value cannot be null.</param>
		/// <returns>The list of user groups.</returns>
		public IEnumerable<string> GetUserGroups(IUserIdentity identity)
		{
			if (!Code.ValidateArgument(identity, nameof(identity), TaggingUtilities.ReserveTag(0x2382109d /* tag_967c3 */)))
			{
				return Enumerable.Empty<string>();
			}

			ITestGroupsDataSet testGroupsDataSet = GetTestGroupsDataSet();
			if (testGroupsDataSet == null)
			{
				return Enumerable.Empty<string>();
			}

			if (!identity.IsAuthenticated)
			{
				return testGroupsDataSet.DefaultGroups;
			}

			IEnumerable<string> result = new List<string>();
			if (!string.IsNullOrWhiteSpace(identity.EmailAddressSignIn))
			{
				result = result.Union(GetUserGroups(identity.EmailAddressSignIn), StringComparer.OrdinalIgnoreCase);
			}

			if (!string.IsNullOrWhiteSpace(identity.EmailAddressPreferred))
			{
				result = result.Union(GetUserGroups(identity.EmailAddressPreferred), StringComparer.OrdinalIgnoreCase);
			}

			return result;
		}


		/// <summary>
		/// Retrieves all groups for a user
		/// </summary>
		/// <param name="userEmail">User email. This value cannot be null.</param>
		/// <returns>List of user groups</returns>
		public IEnumerable<string> GetUserGroups(string userEmail)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(userEmail, nameof(userEmail), TaggingUtilities.ReserveTag(0x2382109e /* tag_967c4 */)))
			{
				return Enumerable.Empty<string>();
			}

			ITestGroupsDataSet testGroupsDataSet = GetTestGroupsDataSet();
			if (testGroupsDataSet == null)
			{
				return Enumerable.Empty<string>();
			}

			return testGroupsDataSet.GetUserGroups(userEmail);
		}


		/// <summary>
		/// Retrieves all groups for a deployment id
		/// </summary>
		/// <param name="deploymentId">Deployment id. This value cannot be null.</param>
		/// <returns>List of user groups</returns>
		public IEnumerable<string> GetDeploymentIdGroups(string deploymentId) => GetUserGroups(deploymentId);


		/// <summary>
		/// Retrieves all users for a group
		/// </summary>
		/// <remarks>This search is not meant to be efficient, to be used for diagnostics only</remarks>
		/// <param name="groupName">Group name. This value cannot be null.</param>
		/// <returns>List of group users</returns>
		public IEnumerable<string> GetGroupUsers(string groupName)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(groupName, nameof(groupName), TaggingUtilities.ReserveTag(0x2382109f /* tag_967c5 */)))
			{
				return Enumerable.Empty<string>();
			}

			ITestGroupsDataSet testGroupsDataSet = GetTestGroupsDataSet();
			if (testGroupsDataSet == null)
			{
				return Enumerable.Empty<string>();
			}

			return testGroupsDataSet.GetGroupUsers(groupName);
		}


		/// <summary>
		/// Checks if the logged-in user with the specified identity is in one or more of the
		/// specified groups.
		/// </summary>
		/// <param name="identity">The identity of the logged in user. This value cannot be null.</param>
		/// <param name="groupNames">A semicolon-delimited list of group names. This value cannot be null.</param>
		/// <returns>True if the user belongs to one or more of the groups, false otherwise.</returns>
		public bool IsUserInGroups(IUserIdentity identity, string groupNames)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(groupNames, nameof(groupNames), TaggingUtilities.ReserveTag(0x238210a0 /* tag_967c6 */)) ||
				!Code.ValidateArgument(identity, nameof(identity), TaggingUtilities.ReserveTag(0x238210a1 /* tag_967c7 */)))
			{
				return false;
			}

			if (!identity.IsAuthenticated)
			{
				return IsDefault(groupNames);
			}

			if (!string.IsNullOrWhiteSpace(identity.EmailAddressSignIn) && IsUserInGroups(identity.EmailAddressSignIn, groupNames))
			{
				return true;
			}

			if (!string.IsNullOrWhiteSpace(identity.EmailAddressPreferred) && IsUserInGroups(identity.EmailAddressPreferred, groupNames))
			{
				return true;
			}

			return false;
		}


		/// <summary>
		/// Checks if user is in one or more of the specified groups
		/// </summary>
		/// <param name="userEmail">User email. This value cannot be null.</param>
		/// <param name="groupNames">Group names. This value cannot be null.</param>
		/// <returns>true if the user belongs to one or more of the groups, false otherwise</returns>
		public bool IsUserInGroups(string userEmail, string groupNames)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(groupNames, nameof(groupNames), TaggingUtilities.ReserveTag(0x238210a2 /* tag_967c8 */)) ||
				!Code.ValidateNotNullOrWhiteSpaceArgument(userEmail, nameof(userEmail), TaggingUtilities.ReserveTag(0x238210a3 /* tag_967c9 */)))
			{
				return false;
			}

			IEnumerable<string> groupNamesList = groupNames.Split(s_listSeparatorArray, StringSplitOptions.RemoveEmptyEntries);
			if (groupNamesList.Intersect(GetUserGroups(userEmail), StringComparer.OrdinalIgnoreCase).Any())
			{
				return true;
			}

			return false;
		}


		/// <summary>
		/// Retrieves currently loaded TestGroups DataSet
		/// </summary>
		/// <returns>Currently loaded TestGroups DataSet</returns>
		private ITestGroupsDataSet GetTestGroupsDataSet()
		{
			ITestGroupsDataSet dataSet = m_testGroupsDataSetLoader.LoadedDataSet;
			if (dataSet == null)
			{
				ULSLogging.LogTraceTag(0x238210c0 /* tag_967da */, Categories.TestGroupsDataSet, Levels.Error,
					"TestGroupsDataSet is null, TestGroups authentication will fail.");
			}
			else if (!dataSet.IsHealthy)
			{
				ULSLogging.LogTraceTag(0x238210c1 /* tag_967db */, Categories.TestGroupsDataSet, Levels.Error,
					"TestGroupsDataSet is not healthy.");
			}

			return dataSet;
		}


		/// <summary>
		/// TestGroups DataSet loader
		/// </summary>
		private readonly IConfigurationDataSetLoader<ITestGroupsDataSet> m_testGroupsDataSetLoader;


		/// <summary>
		/// The array of characters separating elements in a list stored within an asset field.
		/// </summary>
		private static readonly string[] s_listSeparatorArray = { ";" };
	}
}
