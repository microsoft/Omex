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
		/// Set required compliance level for logging user identity
		/// </summary>
		public UserIdentiyComplianceLevel LoggingComlience { get; set; } = UserIdentiyComplianceLevel.OrgId;
	}
}
