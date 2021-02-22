// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// The type of user identifier.
	/// </summary>
	public enum UserIdentifierType
	{
		/// <summary>
		/// Undefined type
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// User puid
		/// </summary>
		Puid = 1,

		/// <summary>
		/// AnonUid from anonymous cookie
		/// </summary>
		AnonUid = 2,

		/// <summary>
		/// Ip address.
		/// </summary>
		IpAddress = 3
	}
}
