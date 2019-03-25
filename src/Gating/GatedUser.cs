// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// A gated user.
	/// </summary>
	public class GatedUser
	{
		/// <summary>
		/// User identifier
		/// </summary>
		public string UserIdentifier { get; set; }


		/// <summary>
		/// Is the user a dogfood user or not
		/// </summary>
		public bool IsDogfoodUser { get; set; }
	}
}