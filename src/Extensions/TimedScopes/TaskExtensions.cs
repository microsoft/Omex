// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

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
		public static async ValueTask<T> WithTimedScope<T>(this ValueTask<T> task, ITimedScopeProvider provider, string name)
		{
			using TimedScope timedScope = provider.Start(name, TimedScopeResult.SystemError);
			T result = await task.ConfigureAwait(false);
			timedScope.Result = TimedScopeResult.Success;
			return result;
		}


		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static async ValueTask WithTimedScope(this ValueTask task, ITimedScopeProvider provider, string name) =>
			await ConvetrToTaskWithResultValue(task).WithTimedScope(provider, name).ConfigureAwait(false);


		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static ValueTask<T> WithTimedScope<T>(this Task<T> task, ITimedScopeProvider provider, string name) =>
			new ValueTask<T>(task).WithTimedScope(provider, name);


		/// <summary>
		/// Starts TimedScopes before task execution and finish after it's completion
		/// </summary>
		public static ValueTask WithTimedScope(this Task task, ITimedScopeProvider provider, string name) =>
			new ValueTask(task).WithTimedScope(provider, name);


		private static async ValueTask<bool> ConvetrToTaskWithResultValue(ValueTask task)
		{
			await task.ConfigureAwait(false);
			return true;
		}
	}
}
