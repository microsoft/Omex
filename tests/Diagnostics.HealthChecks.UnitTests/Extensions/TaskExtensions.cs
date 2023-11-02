// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

// Imported from https://github.com/dotnet/aspnetcore/blob/main/src/Shared/TaskExtensions.cs

#pragma warning disable IDE0005 // Remove unnecessary using directives

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests.Extensions
{
	public static class TaskExtensions
	{
		private static string CreateMessage(TimeSpan timeout, string filePath, int lineNumber)
		=> string.IsNullOrEmpty(filePath)
		? $"The operation timed out after reaching the limit of {timeout.TotalMilliseconds}ms."
		: $"The operation at {filePath}:{lineNumber} timed out after reaching the limit of {timeout.TotalMilliseconds}ms.";

		public static async Task TimeoutAfter(this Task task, TimeSpan timeout,
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = default)
		{
			// Don't create a timer if the task is already completed
			// or the debugger is attached
			if (task.IsCompleted || Debugger.IsAttached)
			{
				await task.ConfigureAwait(false);
				return;
			}
#if NET6_0_OR_GREATER
			try
			{
				await task.WaitAsync(timeout).ConfigureAwait(false);
			}
			catch (TimeoutException ex) when (ex.Source == typeof(TaskExtensions).Namespace)
			{
				throw new TimeoutException(CreateMessage(timeout, filePath, lineNumber));
			}
#else
			CancellationTokenSource cts = new CancellationTokenSource();
			if (task == await Task.WhenAny(task, Task.Delay(timeout, cts.Token)).ConfigureAwait(false))
			{
				cts.Cancel();
				await task.ConfigureAwait(false);
			}
			else
			{
				throw new TimeoutException(CreateMessage(timeout, filePath, lineNumber));
			}
#endif
		}
	}
}
