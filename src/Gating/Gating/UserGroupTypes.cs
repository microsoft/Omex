/**************************************************************************************************
	UserGroupTypes.cs

	Enumeration of the user group types.
**************************************************************************************************/

#region Using Directives

using System;

#endregion

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// User group types
	/// </summary>
	[Flags]
	public enum UserGroupTypes
	{
		/// <summary>
		/// No users
		/// </summary>
		None = 0,


		/// <summary>
		/// Unspecified
		/// </summary>
		Unspecified = 0x1,


		/// <summary>
		/// Dogfood user group
		/// </summary>
		Dogfood = 0x2,


		/// <summary>
		/// Custom user groups
		/// </summary>
		CustomGroup = 0x4,
	}
}
