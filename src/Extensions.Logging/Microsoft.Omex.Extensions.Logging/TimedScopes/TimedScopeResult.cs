// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Omex.Extensions.Logging.TimedScopes
{
	/// <summary>
	/// Defines the possible scope results
	/// </summary>
	/// <remarks>
	/// This enum is essential to OMEX monitoring solutions and generally should not change.
	/// If values are added or removed, TimedScopeResultExtensions should be updated as well.
	/// </remarks>
	[DataContract]
	public enum TimedScopeResult : int
	{
		/// <summary>
		/// Result is unknown (default)
		/// </summary>
		/// <remarks>Result should always be set to one of the other values explicitly. Unknown causes an error to be logged, and the scope is assumed failed.</remarks>
		[EnumMember]
		[Obsolete("Default value, not to be used explicitly", error: true)]
		Unknown = 0,


		/// <summary>
		/// Success
		/// </summary>
		[EnumMember]
		Success = 1,


		/// <summary>
		/// System Error
		/// </summary>
		[EnumMember]
		SystemError = 2,


		/// <summary>
		/// Expected Error (consolidating old UserError and PayloadError)
		/// </summary>
		[EnumMember]
		ExpectedError = 6
	}
}
