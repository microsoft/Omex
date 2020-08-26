// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// Options for UserIdentiyMiddleware
	/// </summary>
	public class UserIdentiyMiddlewareOptions
	{
		/// <summary>
		/// Set required complience level for logging user identity
		/// </summary>
		public UserIdentiyComlienceLevel LoggingComlience { get; set; } = UserIdentiyComlienceLevel.OrgId;
	}

	/// <summary>
	/// Level of complience for user indetity logging
	/// </summary>
	public enum UserIdentiyComlienceLevel
	{
		/// <summary>
		/// User indentity would be changed after some period (usualy 40 hours not more then 48 hours) to avoid tracking of user information
		/// </summary>
		OrgId,

		/// <summary>
		/// Constant user indertity in logs that could be correlated until logs are stored
		/// </summary>
		LiveId
	}
}
