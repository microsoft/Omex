/*****************************************************************************
	RetryCounterUnitTests.cs

	Unit tests for RetryCounter class.
******************************************************************************/

#region Using directives

using System;
using System.Threading;
using Microsoft.Omex.System.Monads;
using Microsoft.Omex.System.UnitTests.Shared;
using Xunit;

#endregion

namespace Microsoft.Omex.System.UnitTests.Monads
{
	/// <summary>
	/// Unit tests for RetryCounter class.
	/// </summary>
	public sealed class RetryCounterUnitTests : UnitTestBase
	{
		[Fact]
		public void RetryCounter_CounterMakesExpectedNumberOfAttempts()
		{
			int exponent = 5;
			int retryLimit = 3;
			RetryPolicy policy = new RetryPolicy(new LinearBackoffPolicy(), exponent, retryLimit);
			RetryCounter counter = new RetryCounter(policy);
			DoNumberOfCallsTesting(counter, retryLimit + 1);
		}


		[Fact]
		public void RetryCounter_NegativeLimit_PerformsNoAttempts()
		{
			int exponent = 5;
			int retryLimit = -1;
			RetryPolicy policy = new RetryPolicy(new LinearBackoffPolicy(), exponent, retryLimit);
			RetryCounter counter = new RetryCounter(policy);
			DoNumberOfCallsTesting(counter, 1);
		}


		[Fact]
		public void RetryCounter_ZeroLimit_PerformsNoAttempts()
		{
			int exponent = 5;
			int retryLimit = 0;
			RetryPolicy policy = new RetryPolicy(new LinearBackoffPolicy(), exponent, retryLimit);
			RetryCounter counter = new RetryCounter(policy);
			DoNumberOfCallsTesting(counter, 1);
		}


		[Fact]
		public void RetryCounter_AttemptsShouldStopOnSuccessfullCall()
		{
			int retryLimit = 10;
			RetryPolicy policy = new RetryPolicy(null, 0, retryLimit);
			RetryCounter counter = new RetryCounter(policy);
			DoNumberOfCallsTesting(counter, SuccessfulCallNumber);
		}


		private void DoNumberOfCallsTesting(RetryCounter retryCounter, int expectedNumbersOfCall)
		{
			AtomicCounter callsCount = new AtomicCounter();
			int result = retryCounter.Run(() => TestFunc(callsCount));
			Assert.Equal(expectedNumbersOfCall, result);
		}


		private static Tuple<bool, int> TestFunc(AtomicCounter counter)
		{
			counter.Increment();
			return Tuple.Create(counter.Count == SuccessfulCallNumber, counter.Count);
		}


		private class AtomicCounter
		{
			public int Count => m_counter;


			public void Increment() => Interlocked.Increment(ref m_counter);


			private int m_counter;
		}


		private const int SuccessfulCallNumber = 5;
	}
}
