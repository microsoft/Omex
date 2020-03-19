// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Extensions to TimedScopes class to set additional information
	/// </summary>
	public static class TimedScopesExtensions
	{
		/// <summary>
		/// Set result
		/// </summary>
		/// <remarks>This property won't be transfered to child activity or via web requests</remarks>
		public static TimedScope SetResult(this TimedScope timedScope, TimedScopeResult result)
		{
			timedScope.Activity.AddTag(ActivityTagKeys.Result, ActivityResultStrings.ResultToString(result));
			return timedScope;
		}

		/// <summary>
		/// Set sub type
		/// </summary>
		/// <remarks>This property won't be transfered to child activity or via web requests</remarks>
		public static TimedScope SetSubType(this TimedScope timedScope, string subType)
		{
			timedScope.Activity.AddTag(ActivityTagKeys.SubType, subType);
			return timedScope;
		}

		/// <summary>
		/// Set metadata
		/// </summary>
		/// <remarks>This property won't be transfered to child activity or via web requests</remarks>
		public static TimedScope SetMetadata(this TimedScope timedScope, string metadata)
		{
			timedScope.Activity.AddTag(ActivityTagKeys.Metadata, metadata);
			return timedScope;
		}
	}
}
