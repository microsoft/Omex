// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Omex.Gating.Data;
using Microsoft.Omex.Gating.Experimentation;
using Microsoft.Omex.Gating.UnitTests.Shared;
using Microsoft.Omex.System.Caching;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared.Configuration;
using Microsoft.Omex.System.UnitTests.Shared.Configuration.DataSets;
using Microsoft.Omex.System.UnitTests.Shared.Data;
using Xunit;

namespace Microsoft.Omex.Gating.UnitTests
{
	/// <summary>
	/// Unit tests for GateExtensions.
	/// </summary>
	public sealed class GateExtensionsUnitTests : UnitTestBase
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public GateExtensionsUnitTests()
		{
			UnitTestDataSetLoader<GateDataSet> loader = new UnitTestDataSetLoader<GateDataSet>(new LocalCache(), new UnitTestResourceMonitor());

			UnitTestGateDataSet dataSetOverride = new UnitTestGateDataSet(ResourceNames.Gates, ResourceNames.TestGroups);
			dataSetOverride.Load(new Dictionary<string, IResourceDetails>(2, StringComparer.OrdinalIgnoreCase)
				{
					{ ResourceNames.Gates, EmbeddedResources.GetEmbeddedResourceAsResourceDetails(ResourceNames.Gates, GetType()) },
					{ ResourceNames.TestGroups, EmbeddedResources.GetEmbeddedResourceAsResourceDetails(ResourceNames.TestGroups, GetType()) }
				});

			loader.OverrideLoadedDataSet(dataSetOverride);

			Gate gate = new Gate("ActiveGate1");
			dataSetOverride.AddGateOverride(gate.Name, gate);

			gate = new Gate("ActiveGate2");
			dataSetOverride.AddGateOverride(gate.Name, gate);

			gate = new Gate("InactiveGate1")
			{
				UserTypes = UserGroupTypes.None
			};
			dataSetOverride.AddGateOverride(gate.Name, gate);

			gate = new Gate("InactiveGate2")
			{
				UserTypes = UserGroupTypes.None
			};
			dataSetOverride.AddGateOverride(gate.Name, gate);
			Gates = new Gates(loader);
		}

		private Gates Gates { get; }

		#region Gate Extensions

		[Fact]
		public void PerformAction_WithMultipleActiveAndInactiveGates_ShouldPerformScopedActionForFirstActiveGate()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(Gates.GetGate("InactiveGate1"),
					() => { Assert.False(true, "Should not perform action for inactive gate."); }),
				new GatedAction(Gates.GetGate("ActiveGate1"),
					() => { count++; }),
				new GatedAction(Gates.GetGate("ActiveGate2"),
					() => { Assert.False(true, "Should not perform action for second active gate."); }),
				new GatedAction(Gates.GetGate("InactiveGate2"),
					() => { Assert.False(true, "Should not perform action for inactive gate."); }),
				new GatedAction(
					() => { Assert.False(true, "Should not perform action for base line."); }));

			Assert.Equal(count, 1);
			Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected one gate to be activated");
		}

		[Fact]
		public void PerformAction_WithUnscopedModeAndMultipleActiveAndInactiveGates_ShouldPerformUnscopedActionForFirstActiveGate()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(GatedCode.Modes.None,
				new GatedAction(Gates.GetGate("InactiveGate1"),
					() => { Assert.False(true, "Should not perform action for inactive gate."); }),
				new GatedAction(Gates.GetGate("ActiveGate1"),
					() => { count++; }),
				new GatedAction(Gates.GetGate("ActiveGate2"),
					() => { Assert.False(true, "Should not perform action for second active gate."); }),
				new GatedAction(Gates.GetGate("InactiveGate2"),
					() => { Assert.False(true, "Should not perform action for inactive gate."); }),
				new GatedAction(
					() => { Assert.False(true, "Should not perform action for base line."); }));

			Assert.Equal(count, 1);
			Assert.False(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected one gate not to be reported as activated");
		}

		[Fact]
		public void PerformAction_SingleInactiveGate_ShouldNotPerformScopedActionForInactiveGate()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(Gates.GetGate("InactiveGate1"), () => { count++; });
			Assert.Equal(count, 0);
			Assert.Equal(gateContext.ActivatedGates.Count(), 0);
		}

		[Fact]
		public void PerformAction_SingleActiveGate_ShouldPerformScopedActionForActiveGate()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(Gates.GetGate("ActiveGate1"), () => { count++; });
			Assert.Equal(count, 1);
			Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected one gate to be activated");
		}

		[Fact]
		public void PerformAsyncAction_SingleInactiveGate_ShouldNotPerformScopedActionForInactiveGate()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformAction(Gates.GetGate("InactiveGate1"),
					async () =>
					{
						count++;
						await Task.Delay(10);
					});
				Assert.Equal(count, 0);
				Assert.Equal(gateContext.ActivatedGates.Count(), 0);
			});
		}

		[Fact]
		public void PerformAsyncAction_SingleActiveGate_ShouldPerformScopedActionForActiveGate()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformAction(Gates.GetGate("ActiveGate1"),
					async () =>
					{
						count++;
						await Task.Delay(10);
					});
				Assert.Equal(count, 1);
				Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")));
			});
		}

		[Fact]
		public void PerformFunction_WithMultipleActiveAndInactiveGates_ShouldPerformScopedFunctionForFirstActiveGate()
		{
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			int count = gateContext.PerformFunction<int>(
				new GatedFunc<int>(Gates.GetGate("InactiveGate1"),
					() => { Assert.False(true, "Should not perform function for inactive gate."); return 1; }),
				new GatedFunc<int>(Gates.GetGate("ActiveGate1"),
					() => { return 2; }),
				new GatedFunc<int>(Gates.GetGate("ActiveGate2"),
					() => { Assert.False(true, "Should not perform function for second active gate."); return 3; }),
				new GatedFunc<int>(Gates.GetGate("InactiveGate2"),
					() => { Assert.False(true, "Should not perform function for inactive gate."); return 4; }),
				new GatedFunc<int>(
					() => { Assert.False(true, "Should not perform function for base line."); return 5; }));

			Assert.Equal(count, 2);
			Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected one gate to be activated");
		}

		[Fact]
		public void PerformFunction_WithUnscopedModeAndMultipleActiveAndInactiveGates_ShouldPerformUnscopedFunctionForFirstActiveGate()
		{
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			int count = gateContext.PerformFunction<int>(GatedCode.Modes.None,
				new GatedFunc<int>(Gates.GetGate("InactiveGate1"),
					() => { Assert.False(true, "Should not perform function for inactive gate."); return 1; }),
				new GatedFunc<int>(Gates.GetGate("ActiveGate1"),
					() => { return 2; }),
				new GatedFunc<int>(Gates.GetGate("ActiveGate2"),
					() => { Assert.False(true, "Should not perform function for second active gate."); return 3; }),
				new GatedFunc<int>(Gates.GetGate("InactiveGate2"),
					() => { Assert.False(true, "Should not perform function for inactive gate."); return 4; }),
				new GatedFunc<int>(
					() => { Assert.False(true, "Should not perform function for base line."); return 5; }));

			Assert.Equal(count, 2);
			Assert.False(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected no Gates reported as activated");
		}

		[Fact]
		public void PerformAsyncFunction_WithMultipleActiveAndInactiveGates_ShouldPerformScopedFunctionForFirstActiveGate()
		{
			VerifyAsync(async () =>
			{
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				int count = await gateContext.PerformFunction<int>(
					new GatedAsyncFunc<int>(Gates.GetGate("InactiveGate1"),
						() => { Assert.False(true, "Should not perform function for inactive gate."); return Task.FromResult(1); }),
					new GatedAsyncFunc<int>(Gates.GetGate("ActiveGate1"),
						() => Task.FromResult(2)),
					new GatedAsyncFunc<int>(Gates.GetGate("ActiveGate2"),
						() => { Assert.False(true, "Should not perform function for second active gate."); return Task.FromResult(3); }),
					new GatedAsyncFunc<int>(Gates.GetGate("InactiveGate2"),
						() => { Assert.False(true, "Should not perform function for inactive gate."); return Task.FromResult(4); }),
					new GatedAsyncFunc<int>(
						() => { Assert.False(true, "Should not perform function for base line."); return Task.FromResult(5); }));

				Assert.Equal(count, 2);
				Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected one gate to be activated");
			});
		}

		[Fact]
		public void PerformAsyncFunction_WithUnscopedModeAndMultipleActiveAndInactiveGates_ShouldPerformUnscopedFunctionForFirstActiveGate()
		{
			VerifyAsync(async () =>
			{
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				int count = await gateContext.PerformFunction<int>(GatedCode.Modes.None,
					new GatedAsyncFunc<int>(Gates.GetGate("InactiveGate1"),
						() => { Assert.False(true, "Should not perform function for inactive gate."); return Task.FromResult(1); }),
					new GatedAsyncFunc<int>(Gates.GetGate("ActiveGate1"),
						() => Task.FromResult(2)),
					new GatedAsyncFunc<int>(Gates.GetGate("ActiveGate2"),
						() => { Assert.False(true, "Should not perform function for second active gate."); return Task.FromResult(3); }),
					new GatedAsyncFunc<int>(Gates.GetGate("InactiveGate2"),
						() => { Assert.False(true, "Should not perform function for inactive gate."); return Task.FromResult(4); }),
					new GatedAsyncFunc<int>(
						() => { Assert.False(true, "Should not perform function for base line."); return Task.FromResult(5); }));

				Assert.Equal(count, 2);
				Assert.False(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected no Gates reported as activated");
			});
		}

		[Fact]
		public void PerformEachAction_WithMultipleActiveAndInactiveGates_ShouldPerformScopedActionsForAllActiveGates()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformEachAction(
				new GatedAction(Gates.GetGate("InactiveGate1"),
					() => { Assert.False(true, "Should not perform action for inactive gate."); }),
				new GatedAction(Gates.GetGate("ActiveGate1"),
					() => { count++; }),
				new GatedAction(Gates.GetGate("ActiveGate2"),
					() => { count++; }),
				new GatedAction(Gates.GetGate("InactiveGate2"),
					() => { Assert.False(true, "Should not perform action for inactive gate."); }),
				new GatedAction(
					() => { count++; }));

			Assert.Equal(count, 3);
			Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected gate1 to be activated");
			Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate2")), "Expected gate2 to be activated");
		}

		[Fact]
		public void PerformEachAction_WithUnscopedModeAndWithMultipleActiveAndInactiveGates_ShouldPerformUnscopedActionsForAllActiveGates()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformEachAction(GatedCode.Modes.None,
				new GatedAction(Gates.GetGate("InactiveGate1"),
					() => { Assert.False(true, "Should not perform action for inactive gate."); }),
				new GatedAction(Gates.GetGate("ActiveGate1"),
					() => { count++; }),
				new GatedAction(Gates.GetGate("ActiveGate2"),
					() => { count++; }),
				new GatedAction(Gates.GetGate("InactiveGate2"),
					() => { Assert.False(true, "Should not perform action for inactive gate."); }),
				new GatedAction(
					() => { count++; }));

			Assert.Equal(count, 3);
			Assert.False(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected gate1 not to be reported as activated");
			Assert.False(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate2")), "Expected gate2 not to be reported as activated");
		}

		[Fact]
		public void PerformEachFunction_WithMultipleActiveAndInactiveGates_ShouldPerformScopedFunctionForAllActiveGates()
		{
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			int count = gateContext.PerformEachFunction<int>(0,
				new GatedFunc<int, int>(Gates.GetGate("InactiveGate1"),
					(i) => { Assert.False(true, "Should not perform function for inactive gate."); return ++i; }),
				new GatedFunc<int, int>(Gates.GetGate("ActiveGate1"),
					(i) => { return ++i; }),
				new GatedFunc<int, int>(Gates.GetGate("ActiveGate2"),
					(i) => { return ++i; }),
				new GatedFunc<int, int>(Gates.GetGate("InactiveGate2"),
					(i) => { Assert.False(true, "Should not perform function for inactive gate."); return ++i; }),
				new GatedFunc<int, int>(
					(i) => { return ++i; }));

			Assert.Equal(count, 3);
			Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected gate1 to be activated");
			Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate2")), "Expected gate2 to be activated");
		}

		[Fact]
		public void PerformEachFunction_UnscopedWithMultipleActiveAndInactiveGates_ShouldPerformUnscopedFunctionForAllActiveGates()
		{
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			int count = gateContext.PerformEachFunction<int>(GatedCode.Modes.None, 0,
				new GatedFunc<int, int>(Gates.GetGate("InactiveGate1"),
					(i) => { Assert.False(true, "Should not perform function for inactive gate."); return ++i; }),
				new GatedFunc<int, int>(Gates.GetGate("ActiveGate1"),
					(i) => { return ++i; }),
				new GatedFunc<int, int>(Gates.GetGate("ActiveGate2"),
					(i) => { return ++i; }),
				new GatedFunc<int, int>(Gates.GetGate("InactiveGate2"),
					(i) => { Assert.False(true, "Should not perform function for inactive gate."); return ++i; }),
				new GatedFunc<int, int>(
					(i) => { return ++i; }));

			Assert.Equal(count, 3);
			Assert.False(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected gate1 not to be reported as activated");
			Assert.False(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate2")), "Expected gate2 not to be reported as activated");
		}

		[Fact]
		public void PerformFunction_WithOnlyInactiveGates_ShouldReturnDefaultOfType()
		{
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			int count = gateContext.PerformFunction<int>(
				new GatedFunc<int>(Gates.GetGate("InactiveGate1"),
					() => { Assert.False(true, "Should not perform function for inactive gate."); return 1; }),
				new GatedFunc<int>(Gates.GetGate("InactiveGate2"),
					() => { Assert.False(true, "Should not perform function for inactive gate."); return 4; }));

			Assert.Equal(count, default(int));
		}

		[Fact]
		public void PerformAsyncFunction_WithOnlyInactiveGates_ShouldReturnDefaultOfType()
		{
			VerifyAsync(async () =>
			{
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				int count = await gateContext.PerformFunction<int>(
					new GatedAsyncFunc<int>(Gates.GetGate("InactiveGate1"),
						() => { Assert.False(true, "Should not perform function for inactive gate."); return Task.FromResult(1); }),
					new GatedAsyncFunc<int>(Gates.GetGate("InactiveGate2"),
						() => { Assert.False(true, "Should not perform function for inactive gate."); return Task.FromResult(4); }));

				Assert.Equal(count, default(int));
			});
		}

		[Fact]
		public void PerformEachFunction_WithOnlyInactiveGates_ShouldReturnInput()
		{
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			int count = gateContext.PerformEachFunction<int>(1,
				new GatedFunc<int, int>(Gates.GetGate("InactiveGate1"),
					(i) => { Assert.False(true, "Should not perform function for inactive gate."); return ++i; }),
				new GatedFunc<int, int>(Gates.GetGate("InactiveGate2"),
					(i) => { Assert.False(true, "Should not perform function for inactive gate."); return ++i; }));

			Assert.Equal(count, 1);
		}

		[Fact]
		public void PerformFunction_SingleInactiveGate_ShouldNotPerformScopedFunctionForInactiveGate()
		{
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			int result = gateContext.PerformFunction(Gates.GetGate("InactiveGate1"), () => { return 1; });
			Assert.Equal(result, 0);
			Assert.Equal(gateContext.ActivatedGates.Count(), 0);
		}

		[Fact]
		public void PerformFunction_SingleActiveGate_ShouldPerformScopedFunctionForActiveGate()
		{
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			int result = gateContext.PerformFunction(Gates.GetGate("ActiveGate1"), () => { return 1; });
			Assert.Equal(result, 1);
			Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected one gate to be activated");
		}

		[Fact]
		public void PerformConditionalFunction_WithValidGates_ShouldPerformTheFirstGatedFunction()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformConditionalFunction(
				() =>
				{
					return true;
				},
				new GatedFunc<int>(
					Gates.GetGate("ActiveGate1"),
					() =>
					{
						count += 1;
						return count;
					}),
				new GatedFunc<int>(
					Gates.GetGate("ActiveGate2"),
					() =>
					{
						count += 2;
						return count;
					}));

			Assert.Equal(count, 1);
		}

		[Fact]
		public void PerformConditionalFunction_WithValidAndNonValidGates_ShouldPerformTheOnlyGatedFunction()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformConditionalFunction(
				() =>
				{
					return true;
				},
				new GatedFunc<int>(
					Gates.GetGate("InactiveGate1"),
					() =>
					{
						count += 1;
						return count;
					}),
				new GatedFunc<int>(
					Gates.GetGate("ActiveGate2"),
					() =>
					{
						count += 2;
						return count;
					}));

			Assert.Equal(count, 2);
		}

		[Fact]
		public void PerformConditionalFunction_NonValidGates_ShouldNotPerformAnyGatedFunction()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformConditionalFunction(
				() =>
				{
					return true;
				},
				new GatedFunc<int>(
					Gates.GetGate("InactiveGate1"),
					() =>
					{
						count += 1;
						return count;
					}),
				new GatedFunc<int>(
					Gates.GetGate("InactiveGate2"),
					() =>
					{
						count += 2;
						return count;
					}));

			Assert.Equal(count, 0);
		}

		[Fact]
		public void PerformConditionalFunction_WithFailedCondition_ShouldNotPerformAnyGatedFunction()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformConditionalFunction(
				() =>
				{
					return false;
				},
				new GatedFunc<int>(
					Gates.GetGate("ActiveGate1"),
					() =>
					{
						count += 1;
						return count;
					}),
				new GatedFunc<int>(
					Gates.GetGate("ActiveGate2"),
					() =>
					{
						count += 2;
						return count;
					}));

			Assert.Equal(count, 0);
		}

		[Fact]
		public void PerformConditionalFunction_WithFailedCondition_ShouldPerformDefaultFunction()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformConditionalFunction(
				() =>
				{
					return false;
				},
				new GatedFunc<int>(
					Gates.GetGate("ActiveGate1"),
					() =>
					{
						count += 1;
						return count;
					}),
				new GatedFunc<int>(
					() =>
					{
						count += 2;
						return count;
					}));

			Assert.Equal(count, 2);
		}

		[Fact]
		public void PerformAsyncFunction_SingleInactiveGate_ShouldNotPerformScopedFunctionForInactiveGate()
		{
			VerifyAsync(async () =>
			{
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				int result = await gateContext.PerformFunction(
					Gates.GetGate("InactiveGate1"),
					() => Task.FromResult(1));
				Assert.Equal(result, 0);
				Assert.Equal(gateContext.ActivatedGates.Count(), 0);
			});
		}

		[Fact]
		public void PerformAsyncFunction_SingleActiveGate_ShouldPerformScopedFunctionForActiveGate()
		{
			VerifyAsync(async () =>
			{
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				int result = await gateContext.PerformFunction(
					Gates.GetGate("ActiveGate1"),
					() => Task.FromResult(1));
				Assert.Equal(result, 1);
				Assert.True(gateContext.ActivatedGates.Contains(Gates.GetGate("ActiveGate1")), "Expected one gate to be activated");
			});
		}

		[Fact]
		public void PerformConditionalAsyncFunction_WithValidGates_ShouldPerformTheFirstGatedFunction()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformConditionalFunction(
					() => true,
					new GatedAsyncFunc<int>(
						Gates.GetGate("ActiveGate1"),
						() =>
						{
							count += 1;
							return Task.FromResult(count);
						}),
					new GatedAsyncFunc<int>(
						Gates.GetGate("ActiveGate2"),
						() =>
						{
							count += 2;
							return Task.FromResult(count);
						}));

				Assert.Equal(count, 1);
			});
		}

		[Fact]
		public void PerformConditionalAsyncFunction_WithValidAndNonValidGates_ShouldPerformTheOnlyGatedFunction()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformConditionalFunction(
					() => true,
					new GatedAsyncFunc<int>(
						Gates.GetGate("InactiveGate1"),
						() =>
						{
							count += 1;
							return Task.FromResult(count);
						}),
					new GatedAsyncFunc<int>(
						Gates.GetGate("ActiveGate2"),
						() =>
						{
							count += 2;
							return Task.FromResult(count);
						}));

				Assert.Equal(count, 2);
			});
		}

		[Fact]
		public void PerformConditionalAsyncFunction_NonValidGates_ShouldNotPerformAnyGatedFunction()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformConditionalFunction(
					() => true,
					new GatedAsyncFunc<int>(
						Gates.GetGate("InactiveGate1"),
						() =>
						{
							count += 1;
							return Task.FromResult(count);
						}),
					new GatedAsyncFunc<int>(
						Gates.GetGate("InactiveGate2"),
						() =>
						{
							count += 2;
							return Task.FromResult(count);
						}));

				Assert.Equal(count, 0);
			});
		}

		[Fact]
		public void PerformConditionalAsyncFunction_WithFailedCondition_ShouldNotPerformAnyGatedFunction()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformConditionalFunction(
					() => false,
					new GatedAsyncFunc<int>(
						Gates.GetGate("ActiveGate1"),
						() =>
						{
							count = 1;
							return Task.FromResult(count);
						}),
					new GatedAsyncFunc<int>(
						Gates.GetGate("ActiveGate2"),
						() =>
						{
							count = 2;
							return Task.FromResult(count);
						}));

				Assert.Equal(count, 0);
			});
		}

		[Fact]
		public void PerformConditionalAsyncFunction_WithFailedCondition_ShouldPerformDefaultFunction()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformConditionalFunction(
					() => false,
					new GatedAsyncFunc<int>(
						Gates.GetGate("ActiveGate1"),
						() =>
						{
							count = 1;
							return Task.FromResult(count);
						}),
					new GatedAsyncFunc<int>(
						() =>
						{
							count = 2;
							return Task.FromResult(count);
						}));

				Assert.Equal(count, 2);
			});
		}

		[Fact]
		public void PerformConditionalAction_WithValidGates_ShouldPerformTheFirstGateAction()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformConditionalAction(
				() =>
				{
					return true;
				},
				new GatedAction(Gates.GetGate("ActiveGate1"), () => { count++; }),
				new GatedAction(Gates.GetGate("ActiveGate2"), () => { count++; })
			);

			Assert.Equal(count, 1);
		}

		[Fact]
		public void PerformConditionalAction_WithValidAndNonValidGates_ShouldPerformTheOnlyAvailableGatedAction()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformConditionalAction(
				() =>
				{
					return true;
				},
				new GatedAction(Gates.GetGate("InactiveGate1"), () => { count++; }),
				new GatedAction(Gates.GetGate("ActiveGate2"), () => { count++; })
			);

			Assert.Equal(count, 1);
		}

		[Fact]
		public void PerformConditionalAction_WithNonValidGates_ShouldNotPerformAnyGatedAction()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformConditionalAction(
				() =>
				{
					return true;
				},
				new GatedAction(Gates.GetGate("InactiveGate1"), () => { count++; }),
				new GatedAction(Gates.GetGate("InactiveGate2"), () => { count++; })
			);

			Assert.Equal(count, 0);
		}

		[Fact]
		public void PerformConditionalAction_WithValidGates_ShouldPerformTheFirstAvailableGate()
		{
			int count = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformConditionalAction(
				() =>
				{
					return false;
				},
				new GatedAction(Gates.GetGate("ActiveGate1"), () => { count++; }),
				new GatedAction(Gates.GetGate("ActiveGate2"), () => { count++; })
			);

			Assert.Equal(count, 0);
		}

		[Fact]
		public void PerformConditionalAction_WithValidGates_ShouldPerformDefaultGatedAction()
		{
			int count1 = 0;
			int count2 = 0;
			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformConditionalAction(
				() =>
				{
					return false;
				},
				new GatedAction(Gates.GetGate("ActiveGate1"), () => { count1++; }),
				new GatedAction(() => { count2++; })
			);

			Assert.Equal(count1, 0);
			Assert.Equal(count2, 1);
		}

		[Fact]
		public void PerformConditionalAsyncAction_WithValidGates_ShouldPerformTheFirstGateAction()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformConditionalAction(
					() => true,
					new GatedAsyncAction(Gates.GetGate("ActiveGate1"),
						async () => { count++; await Task.Delay(10); }),
					new GatedAsyncAction(Gates.GetGate("ActiveGate2"),
						async () => { count++; await Task.Delay(10); }));

				Assert.Equal(count, 1);
			});
		}

		[Fact]
		public void PerformConditionalAsyncAction_WithValidAndNonValidGates_ShouldPerformTheOnlyAvailableGatedAction()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformConditionalAction(
					() => true,
					new GatedAsyncAction(Gates.GetGate("InactiveGate1"),
						async () => { count++; await Task.Delay(10); }),
					new GatedAsyncAction(Gates.GetGate("ActiveGate2"),
						async () => { count++; await Task.Delay(10); }));

				Assert.Equal(count, 1);
			});
		}

		[Fact]
		public void PerformConditionalAsyncAction_WithNonValidGates_ShouldNotPerformAnyGatedAction()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformConditionalAction(
					() => true,
					new GatedAsyncAction(Gates.GetGate("InactiveGate1"),
						async () => { count++; await Task.Delay(10); }),
					new GatedAsyncAction(Gates.GetGate("InactiveGate2"),
						async () => { count++; await Task.Delay(10); }));

				Assert.Equal(count, 0);
			});
		}

		[Fact]
		public void PerformConditionalAsyncAction_WithValidGates_ShouldPerformTheFirstAvailableGate()
		{
			VerifyAsync(async () =>
			{
				int count = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformConditionalAction(
					() => false,
					new GatedAsyncAction(Gates.GetGate("ActiveGate1"),
						async () => { count++; await Task.Delay(10); }),
					new GatedAsyncAction(Gates.GetGate("ActiveGate2"),
						async () => { count++; await Task.Delay(10); }));

				Assert.Equal(count, 0);
			});
		}

		[Fact]
		public void PerformConditionalAsyncAction_WithValidGates_ShouldPerformDefaultGatedAction()
		{
			VerifyAsync(async () =>
			{
				int count1 = 0;
				int count2 = 0;
				IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
				await gateContext.PerformConditionalAction(
					() => false,
					new GatedAsyncAction(Gates.GetGate("ActiveGate1"),
						async () => { count1++; await Task.Delay(10); }),
					new GatedAsyncAction(
						async () => { count2++; await Task.Delay(10); }));

				Assert.Equal(count1, 0);
				Assert.Equal(count2, 1);
			});
		}

		#endregion
	}
}
