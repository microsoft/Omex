// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// Level of compliance for user identity logging
	/// </summary>
	public enum UserIdentiyComplianceLevel
	{
		/// <summary>
		/// User identity would be changed after some period (usually 40 hours not more then 48 hours) to avoid tracking of user information
		/// Also could be applied for LiveId users
		/// </summary>
		OrgId,

		/// <summary>
		/// Constant user identity in logs that could be correlated until logs are stored
		/// </summary>
		LiveId
	}
}
