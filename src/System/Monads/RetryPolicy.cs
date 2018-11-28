/***************************************************************************************************
	RetryPolicy.cs

	Retry policy class.
***************************************************************************************************/

using System;

namespace Microsoft.Omex.System.Monads
{
	/// <summary>
	/// Retry policy class.
	/// </summary>
	public class RetryPolicy
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="backoffPolicy">BackOff Policy</param>
		/// <param name="factor">Parameter used to calculate backoff</param>
		/// <param name="retryLimit">Retry Limit</param>
		/// <param name="timeLimit">Time Limit</param>
		public RetryPolicy(IBackoffPolicy backoffPolicy, int factor, int retryLimit, TimeSpan? timeLimit = default(TimeSpan?))
		{
			BackoffPolicy = backoffPolicy;
			Factor = factor;
			RetryLimit = retryLimit;
			TimeLimit = timeLimit;
		}


		/// <summary>
		/// Backoff Policy
		/// </summary>
		public virtual IBackoffPolicy BackoffPolicy { get; }


		/// <summary>
		/// Parameter used to calculate backoff. Usually plays role as a factor in delay calculation expression.
		/// </summary>
		/// <remarks>
		/// Here are examples of the parameter role depending on selected BackoffPolicy implementation.
		/// ExponentialBackoffPolicy: BackoffDelay = 2 ^ (AttemptNumber) * Factor
		/// LinearBackoffPolicy: BackoffDelay = 1 * Factor
		/// </remarks>
		public virtual int Factor { get; }


		/// <summary>
		/// Number of attempts allowed
		/// </summary>
		public virtual int RetryLimit { get; }


		/// <summary>
		/// Total time allowed for retrying
		/// </summary>
		public virtual TimeSpan? TimeLimit { get; }


		/// <summary>
		/// Don't use a retry policy
		/// </summary>
		public static RetryPolicy None { get; } = new RetryPolicy(null, 0, 0);
	}
}
