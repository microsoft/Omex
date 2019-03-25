// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Gating.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared;
using Xunit;

namespace Microsoft.Omex.Gating.UnitTests
{
	/// <summary>
	/// Unit tests for GatesNone.
	/// </summary>
	public sealed class GatesNoneUnitTests : UnitTestBase
	{
		[Fact]
		public void IsApplicable_WhenNoGatesSpecified_NoApplicableGatesReturned()
		{
			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };

			GatesNone gateCombination = new GatesNone();

			Assert.True(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be true");
			Assert.Null(gates);
		}


		[Fact]
		public void IsApplicable_WithOneApplicableGate_ReturnsNoApplicableGate()
		{
			Gate applicableGate = new Gate("applicable1");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new string[] { applicableGate.Name };
			GatesNone gateCombination = new GatesNone(applicableGate);

			Assert.False(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be false");
			Assert.Null(gates);
		}


		[Fact]
		public void IsApplicable_WithOneNotApplicable_ReturnsSuccessForApplicableGate()
		{
			Gate notApplicableGate = new Gate("notApplicable1");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };

			GatesNone gateCombination = new GatesNone(notApplicableGate);

			Assert.True(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be true");
			Assert.Null(gates);
		}


		[Fact]
		public void IsApplicable_WithOneNotApplicableAndOneApplicableGate_ReturnsNoApplicableGate()
		{
			Gate notApplicableGate = new Gate("notApplicable1");
			Gate applicableGate = new Gate("applicable1");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new string[] { applicableGate.Name };
			GatesNone gateCombination = new GatesNone(notApplicableGate, applicableGate);

			Assert.False(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be false");
			Assert.Null(gates);

		}


		[Fact]
		public void IsApplicable_WithOneApplicableAndOneNotApplicableGate_ReturnsNoApplicableGate()
		{
			Gate notApplicableGate = new Gate("notApplicable1");
			Gate applicableGate = new Gate("applicable1");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new string[] { applicableGate.Name };
			GatesNone gateCombination = new GatesNone(applicableGate, notApplicableGate);

			Assert.False(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be false");
			Assert.Null(gates);
		}


		[Fact]
		public void IsApplicable_WithTwoApplicableGates_ReturnsNoApplicableGate()
		{
			Gate applicableGate1 = new Gate("applicable1");
			Gate applicableGate2 = new Gate("applicable2");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new string[] { applicableGate1.Name, applicableGate2.Name };
			GatesNone gateCombination = new GatesNone(applicableGate1, applicableGate2);

			Assert.False(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be false");
			Assert.Null(gates);
		}


		[Fact]
		public void IsApplicable_WithTwoNotApplicableGates_ReturnsSuccessForApplicableGates()
		{
			Gate notApplicableGate1 = new Gate("notApplicable1");
			Gate notApplicableGate2 = new Gate("notApplicable2");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new string[] { };
			GatesNone gateCombination = new GatesNone(notApplicableGate1, notApplicableGate2);

			Assert.True(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be true");
			Assert.Null(gates);
		}
	}
}