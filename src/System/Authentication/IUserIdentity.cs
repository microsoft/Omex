// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.Authentication
{
	/// <summary>
	/// Interface for UserIdentity
	/// </summary>
	public interface IUserIdentity
	{
		/// <summary>
		/// The currently authenticated user's email address, accessed via the 'memberName' property
		/// on the RPS ticket.  If the user does not possess a valid ticket, it will be returned as
		/// null.
		/// </summary>
		string EmailAddressSignIn { get; }


		/// <summary>
		/// The currently authenticated user's PREFERRED email address, accessed via the
		/// 'preferredEmail' property on the RPS ticket.  If the user does not possess a valid
		/// ticket, it will be returned as null.
		/// </summary>
		string EmailAddressPreferred { get; }


		/// <summary>
		/// IsAuthenticated
		/// </summary>
		bool IsAuthenticated { get; }


		/// <summary>
		/// Return a long version of the user's PUID if it's available.
		/// Returns default(ulong) otherwise.
		/// </summary>
		ulong PUID { get; }


		/// <summary>
		/// Is the user a dogfood user
		/// </summary>
		bool IsDogfoodEnabled { get; }
	}
}