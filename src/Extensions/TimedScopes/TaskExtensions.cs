// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>
	/// Extensions for Task to wrap them in TimedScope
	/// </summary>
	public static class TaskExtensions
	{
		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static async ValueTask<TResult> WithTimedScope<TResult>(this ValueTask<TResult> task, ITimedScopeProvider provider, TimedScopeDefinition definition)
		{
			using TimedScope timedScope = provider.CreateAndStart(definition, TimedScopeResult.SystemError);
			TResult result = await task.ConfigureAwait(false);
			timedScope.Result = TimedScopeResult.Success; // set TimedScope result to success in case if task completed properly (without exception)
			return result;
		}


		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static async ValueTask WithTimedScope(this ValueTask task, ITimedScopeProvider provider, TimedScopeDefinition definition) =>
			// await required to convert ValueTask<T> to ValueTask https://github.com/microsoft/Omex/pull/153#discussion_r389761929
			await ConvertToTaskWithResultValue(task).WithTimedScope(provider, definition).ConfigureAwait(false);


		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static ValueTask<TResult> WithTimedScope<TResult>(this Task<TResult> task, ITimedScopeProvider provider, TimedScopeDefinition definition) =>
			new ValueTask<TResult>(task).WithTimedScope(provider, definition);


		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static ValueTask WithTimedScope(this Task task, ITimedScopeProvider provider, TimedScopeDefinition definition) =>
			new ValueTask(task).WithTimedScope(provider, definition);


		private static async ValueTask<bool> ConvertToTaskWithResultValue(ValueTask task)
		{
			await task.ConfigureAwait(false);
			return true;
		}
	}
}
