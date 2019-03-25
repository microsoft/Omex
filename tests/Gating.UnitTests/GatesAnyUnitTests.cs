// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Gating.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared;
using Xunit;

namespace Microsoft.Omex.Gating.UnitTests
{
	/// <summary>
	/// Unit tests for GatesAny.
	/// </summary>
	public sealed class GatesAnyUnitTests : UnitTestBase
	{
		[Fact]
		public void IsApplicable_WhenNoGatesSpecified_NoApplicableGatesReturned()
		{
			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };

			GatesAny gateCombination = new GatesAny();

			Assert.False(gateCombination.IsApplicable(context, out IGate[] gates), "No gates should be applicable");
			Assert.Null(gates);
		}


		[Fact]
		public void IsApplicable_WithOneApplicableGate_ReturnsTheApplicableGate()
		{
			Gate applicableGate = new Gate("applicable1");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new string[] { applicableGate.Name };
			GatesAny gateCombination = new GatesAny(applicableGate);

			Assert.True(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be true");
			Assert.Same(applicableGate, gates[0]);
		}


		[Fact]
		public void IsApplicable_WithOneNotApplicable_ReturnsNoApplicableGate()
		{
			Gate notApplicableGate = new Gate("notApplicable1");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };

			GatesAny gateCombination = new GatesAny(notApplicableGate);

			Assert.False(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be false");
			Assert.Null(gates);
		}


		[Fact]
		public void IsApplicable_WithOneNotApplicableAndOneApplicableGate_ReturnsOneApplicableGate()
		{
			Gate notApplicableGate = new Gate("notApplicable1");
			Gate applicableGate = new Gate("applicable1");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new string[] { applicableGate.Name };
			GatesAny gateCombination = new GatesAny(notApplicableGate, applicableGate);

			Assert.True(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be true");
			Assert.Same(applicableGate, gates[0]);
		}


		[Fact]
		public void IsApplicable_WithOneApplicableAndOneNotApplicableGate_ReturnsOneApplicableGate()
		{
			Gate notApplicableGate = new Gate("notApplicable1");
			Gate applicableGate = new Gate("applicable1");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new string[] { applicableGate.Name };
			GatesAny gateCombination = new GatesAny(applicableGate, notApplicableGate);

			Assert.True(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be true");
			Assert.Same(applicableGate, gates[0]);
		}


		[Fact]
		public void IsApplicable_WithTwoApplicableGates_ReturnsFirstApplicableGate()
		{
			Gate applicableGate1 = new Gate("applicable1");
			Gate applicableGate2 = new Gate("applicable2");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new string[] { applicableGate1.Name, applicableGate2.Name };
			GatesAny gateCombination = new GatesAny(applicableGate1, applicableGate2);

			Assert.True(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be true");
			Assert.Same(applicableGate1, gates[0]);
		}


		[Fact]
		public void IsApplicable_WithTwoNotApplicableGates_ReturnsFailureForApplicableGates()
		{
			Gate notApplicableGate1 = new Gate("notApplicable1");
			Gate notApplicableGate2 = new Gate("notApplicable2");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new string[] { };
			GatesAny gateCombination = new GatesAny(notApplicableGate1, notApplicableGate2);

			Assert.False(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be false");
			Assert.Null(gates);
		}
	}
}