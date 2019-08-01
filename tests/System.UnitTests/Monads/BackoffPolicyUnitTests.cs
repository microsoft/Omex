// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.Monads;
using Microsoft.Omex.System.UnitTests.Shared;
using Xunit;

namespace Microsoft.Omex.System.UnitTests.Monads
{
	/// <summary>
	/// Unit tests for IBackoffPolicy implementation classes.
	/// </summary>
	public sealed class BackoffPolicyUnitTests : UnitTestBase
	{
		[Fact]
		public void NegativeExponent_ReturnsZero()
		{
			FailOnErrors = false;

			int exponent = -5;
			int factor = 3;
			LinearBackoffPolicy policy = new LinearBackoffPolicy();
			TimeSpan backoffTime = policy.CalculateBackoff(exponent, factor);
			Assert.Equal(0, backoffTime.Milliseconds);
		}


		[Fact]
		public void NegativeFactor_ReturnsZero()
		{
			FailOnErrors = false;

			int exponent = 5;
			int factor = -3;
			LinearBackoffPolicy policy = new LinearBackoffPolicy();
			TimeSpan backoffTime = policy.CalculateBackoff(exponent, factor);
			Assert.Equal(0, backoffTime.Milliseconds);
		}


		[Fact]
		public void LinearBackoff_ReturnsFactor()
		{
			int exponent = 5;
			int factor = 3;
			LinearBackoffPolicy policy = new LinearBackoffPolicy();
			TimeSpan backoffTime = policy.CalculateBackoff(exponent, factor);
			Assert.Equal(factor, backoffTime.Milliseconds);
		}
	}
}