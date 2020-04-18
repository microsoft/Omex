// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Monads
{
	/// <summary>
	/// Retry counter class.
	/// </summary>
	public class RetryCounter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="policy">Retry policy</param>
		public RetryCounter(RetryPolicy policy)
		{
			RetryPolicy = Code.ExpectsArgument(policy, nameof(policy), 0);
		}

		/// <summary>
		/// Retry policy
		/// </summary>
		private RetryPolicy RetryPolicy { get; }

		/// <summary>
		/// Retries func RetryLimit times.
		/// </summary>
		/// <param name="func">Function to run.</param>
		/// <returns>Result of the function.</returns>
		public T Run<T>(Func<Tuple<bool, T>> func)
		{
			if (func == null)
			{
				return default(T);
			}

			Stopwatch runStopWatch = null;
			if (RetryPolicy.TimeLimit.HasValue)
			{
				runStopWatch = new Stopwatch();
				runStopWatch.Start();
			}

			SpinWait spinWait = new SpinWait();
			int attempts = 0;
			while (true)
			{
				Tuple<bool, T> iterationResult = func();
				if (iterationResult.Item1 || !TryAgain(runStopWatch?.Elapsed, ref attempts, out TimeSpan timeToWait))
				{
					return iterationResult.Item2;
				}

				if (timeToWait.TotalMilliseconds > 0)
				{
					Thread.Sleep(timeToWait);
				}
				else
				{
					spinWait.SpinOnce();
				}
			}
		}

		/// <summary>
		/// Check for possibility of next attempt.
		/// </summary>
		/// <remarks>
		/// Increments the attempts counter, if attempt is allowed.
		/// Calculates time to wait corresponding to BackoffPolicy before next try.
		/// </remarks>
		/// <param name="elapsedTime">Time eplased since first attempt</param>
		/// <param name="attempts">Attempts counter</param>
		/// <param name="timeToWait">Calculated time to wait before next attempt</param>
		/// <returns>True if the try limit is not reached yet, false otherwise.</returns>
		protected bool TryAgain(TimeSpan? elapsedTime, ref int attempts, out TimeSpan timeToWait)
		{
			timeToWait = TimeSpan.Zero;

			if (attempts >= RetryPolicy.RetryLimit)
			{
				return false;
			}

			TimeSpan nextTimeToWait = RetryPolicy.BackoffPolicy != null ? RetryPolicy.BackoffPolicy.CalculateBackoff(attempts + 1, RetryPolicy.Factor) : TimeSpan.Zero;

			if (elapsedTime?.Add(nextTimeToWait) >= RetryPolicy.TimeLimit)
			{
				return false;
			}

			timeToWait = nextTimeToWait;
			++attempts;
			return true;
		}
	}
}
