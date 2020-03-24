// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Extensions to TimedScopes class to set additional information
	/// </summary>
	/// <remarks>
	/// This extensions class propagates calls of some activity extensions methods, to avoid making Activity property public,
	/// since it may lead to calls to Activity.Stop that won't be properly logged untill net core 5
	/// </remarks>
	public static class TimedScopesExtensions
	{
		/// <summary>
		/// Set result
		/// </summary>
		/// <remarks>This property won't be transfered to child activity or via web requests</remarks>
		public static TimedScope SetResult(this TimedScope timedScope, TimedScopeResult result)
		{
			timedScope.Activity.SetResult(result);
			return timedScope;
		}

		/// <summary>
		/// Set sub type
		/// </summary>
		/// <remarks>This property won't be transfered to child activity or via web requests</remarks>
		public static TimedScope SetSubType(this TimedScope timedScope, string subType)
		{
			timedScope.Activity.SetSubType(subType);
			return timedScope;
		}

		/// <summary>
		/// Set metadata
		/// </summary>
		/// <remarks>This property won't be transfered to child activity or via web requests</remarks>
		public static TimedScope SetMetadata(this TimedScope timedScope, string metadata)
		{
			timedScope.Activity.SetMetadata(metadata);
			return timedScope;
		}
	}
}
