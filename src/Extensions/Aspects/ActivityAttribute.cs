// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using System.Threading.Tasks;
using MethodBoundaryAspect.Fody.Attributes;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Aspects
{
	/// <summary>
	/// Wraps method into TimedScope
	/// </summary>
	public class ActivityAttribute : OnMethodBoundaryAspect
	{
		private static readonly ITimedScopeProvider s_scopeProvider = new SimpleScopeProvider();

		private readonly Type[] m_expectedExceptions;

		/// <summary>
		/// Creates attribute instance
		/// </summary>
		/// <param name="expectedExceptions">Exception that should threated as user error</param>
		public ActivityAttribute(params Type[] expectedExceptions)
		{
			Type exceptionType = typeof(Exception);
			foreach (Type type in expectedExceptions)
			{
				if (!exceptionType.IsAssignableFrom(type))
				{
					throw new ArgumentException($"Type '{type}' is not inherited from exception");
				}
			}

			m_expectedExceptions = expectedExceptions;
		}

		/// <summary>
		/// Executed on entry to the method
		/// </summary>
		public override void OnEntry(MethodExecutionArgs args)
		{
			args.MethodExecutionTag = s_scopeProvider.CreateAndStart(new TimedScopeDefinition(args.Method.Name));
		}

		/// <summary>
		/// Executed on exit from the method
		/// </summary>
		public override void OnExit(MethodExecutionArgs args)
		{
			TimedScope scope = (TimedScope)args.MethodExecutionTag;

			if (IsAwaitable(args.ReturnValue, out Task? task) && task != null)
			{
				task.ContinueWith(t =>
				{
					HandleResult(scope, t.Exception);
					scope.Stop();
				});
			}
			else
			{
				scope.SetResult(TimedScopeResult.Success)
					.Stop();
			}
		}

		/// <summary>
		/// Executed on exception in the method
		/// </summary>
		public override void OnException(MethodExecutionArgs args)
		{
			TimedScope scope = (TimedScope)args.MethodExecutionTag;
			HandleResult(scope, args.Exception);
		}

		private bool IsAwaitable(object obj, out Task? result)
		{
			result = null;
			if (obj == null)
			{
				return false;
			}

			if (obj is Task task)
			{
				result = task;
			}
			else if (obj is ValueTask valueTask)
			{
				result = valueTask.AsTask();
			}
			else
			{
				Type type = obj.GetType();
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
				{
					MethodInfo methodInfo = type.GetMethod("AsTask");
					result = methodInfo.Invoke(type, Array.Empty<object>()) as Task;
				}
			}

			return result != null;
		}

		private void HandleResult(TimedScope scope, Exception? exception)
		{
			TimedScopeResult result = TimedScopeResult.SystemError;

			if (exception == null)
			{
				result = TimedScopeResult.Success;
			}
			else
			{
				Type exceptionType = exception.GetType();
				foreach (Type exeption in m_expectedExceptions)
				{
					if (exeption.IsAssignableFrom(exceptionType))
					{
						result = TimedScopeResult.ExpectedError;
						break;
					}
				}
			}

			scope.SetResult(result);
		}
	}
}
