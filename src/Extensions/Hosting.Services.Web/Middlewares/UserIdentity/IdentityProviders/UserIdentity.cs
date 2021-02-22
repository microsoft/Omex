// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// User identity
	/// </summary>
	public class UserIdentity
	{
		/// <summary>
		/// Unhashed user id.
		/// </summary>
		public string User { get; set; } = string.Empty;

		/// <summary>
		/// User hash type.
		/// </summary>
		public UserIdentifierType UserHashType { get; set; }
	}
}
