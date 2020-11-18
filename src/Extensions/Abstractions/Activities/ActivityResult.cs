// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.Serialization;

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Defines the possible scope results
	/// </summary>
	/// <remarks>
	/// This enum is essential to OMEX monitoring solutions and generally should not change.
	/// If values are added or removed, TimedScopeResultExtensions should be updated as well.
	/// </remarks>
	[DataContract]
	public enum ActivityResult : int
	{
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
