// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Omex.System.Authentication;

namespace Microsoft.Omex.Gating.Authentication.Groups
{
	/// <summary>
	/// Interface for test groups based access validation
	/// </summary>
	public interface ITestGroups
	{
		/// <summary>
		/// Is one or more of the groups specified equal to a default?
		/// </summary>
		/// <param name="groupNames">A semicolon-delimited list of group names. This value cannot be null.</param>
		/// <returns>True if one or more of the groups specified is equal to a default; false
		/// otherwise.</returns>
		bool IsDefault(string groupNames);


		/// <summary>
		/// Retrieves all groups for the currently logged-in user.
		/// </summary>
		/// <param name="identity">The identity of the logged in user. This value cannot be null.</param>
		/// <returns>The list of user groups.</returns>
		IEnumerable<string> GetUserGroups(IUserIdentity identity);


		/// <summary>
		/// Retrieves all groups for a user
		/// </summary>
		/// <param name="userEmail">User email. This value cannot be null.</param>
		/// <returns>List of user groups</returns>
		IEnumerable<string> GetUserGroups(string userEmail);


		/// <summary>
		/// Retrieves all groups for a deployment id
		/// </summary>
		/// <param name="deploymentId">Deployment id. This value cannot be null.</param>
		/// <returns>List of user groups</returns>
		IEnumerable<string> GetDeploymentIdGroups(string deploymentId);


		/// <summary>
		/// Retrieves all users for a group
		/// </summary>
		/// <remarks>This search is not meant to be efficient, to be used for diagnostics only</remarks>
		/// <param name="groupName">Group name. This value cannot be null.</param>
		/// <returns>List of group users</returns>
		IEnumerable<string> GetGroupUsers(string groupName);


		/// <summary>
		/// Checks if the logged-in user with the specified identity is in one or more of the
		/// specified groups.
		/// </summary>
		/// <param name="identity">The identity of the logged in user. This value cannot be null.</param>
		/// <param name="groupNames">A semicolon-delimited list of group names. This value cannot be null.</param>
		/// <returns>True if the user belongs to one or more of the groups, false otherwise.</returns>
		bool IsUserInGroups(IUserIdentity identity, string groupNames);


		/// <summary>
		/// Checks if user is in one or more of the specified groups
		/// </summary>
		/// <param name="userEmail">User email. This value cannot be null.</param>
		/// <param name="groupNames">Group names. This value cannot be null.</param>
		/// <returns>true if the user belongs to one or more of the groups, false otherwise</returns>
		bool IsUserInGroups(string userEmail, string groupNames);
	}
}