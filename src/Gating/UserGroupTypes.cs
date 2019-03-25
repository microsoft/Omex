// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

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