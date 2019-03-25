// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Omex.System.Configuration.DataSets;

namespace Microsoft.Omex.Gating.Authentication.Groups
{
	/// <summary>
	/// Interface for TestGroups DataSet
	/// </summary>
	public interface ITestGroupsDataSet : IConfigurationDataSet
	{
		/// <summary>
		/// Default access groups
		/// </summary>
		IEnumerable<string> DefaultGroups { get; }


		/// <summary>
		/// Retrieves all groups for a given user email or deployment id
		/// </summary>
		/// <param name="user">User email or deployment id</param>
		/// <returns>List of user groups</returns>
		IEnumerable<string> GetUserGroups(string user);


		/// <summary>
		/// Get group users (email or deployment identifiers)
		/// </summary>
		/// <param name="groupName">Group name</param>
		/// <returns>List of group user emails or deployment ids</returns>
		IEnumerable<string> GetGroupUsers(string groupName);
	}
}