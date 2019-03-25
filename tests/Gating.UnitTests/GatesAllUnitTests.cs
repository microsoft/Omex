// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Gating.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared;
using Xunit;

namespace Microsoft.Omex.Gating.UnitTests
{
	/// <summary>
	/// Unit tests for GatesAll.
	/// </summary>
	public sealed class GatesAllUnitTests : UnitTestBase
	{
		[Fact]
		public void IsApplicable_WhenNoGatesSpecified_NoApplicableGatesReturned()
		{
			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };

			GatesAll gateCombination = new GatesAll();

			Assert.False(gateCombination.IsApplicable(context, out IGate[] applicableGates), "No gates should be applicable");
			Assert.Null(applicableGates);
		}


		[Fact]
		public void IsApplicable_WithOneApplicableGate_NoApplicableGatesReturned()
		{
			Gate applicableGate = new Gate("applicable1");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new[] { applicableGate.Name };
			GatesAll gateCombination = new GatesAll(applicableGate);

			Assert.True(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be true.");
			Assert.NotNull(gates);
		}


		[Fact]
		public void IsApplicable_WitAllGatesApplicable_AllGatesReturned()
		{
			Gate applicableGate = new Gate("applicable1");
			Gate applicableGate2 = new Gate("applicable2");
			Gate applicableGate3 = new Gate("applicable3");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new[] { applicableGate.Name, applicableGate2.Name, applicableGate3.Name };
			GatesAll gateCombination = new GatesAll(applicableGate, applicableGate2, applicableGate3);

			Assert.True(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be true.");
			Assert.NotNull(gates);
			Assert.Same(gates[0], applicableGate);
			Assert.Same(gates[1], applicableGate2);
			Assert.Same(gates[2], applicableGate3);
		}


		[Fact]
		public void IsApplicable_WitOneGateNotApplicable_NoGatesReturned()
		{
			Gate applicableGate = new Gate("applicable1");
			Gate nonApplicableGate = new Gate("nonApplicable");
			Gate applicableGate2 = new Gate("applicable2");

			UnitTestGateContext context = new UnitTestGateContext { AlwaysReturnApplicable = false };
			context.ApplicableGates = new[] { applicableGate.Name, applicableGate2.Name };
			GatesAll gateCombination = new GatesAll(applicableGate, nonApplicableGate, applicableGate2);

			Assert.False(gateCombination.IsApplicable(context, out IGate[] gates), "Get applicable gates should be false.");
			Assert.Null(gates);
		}
	}
}