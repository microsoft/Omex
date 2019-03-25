// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Omex.Gating.Data;
using Microsoft.Omex.Gating.Experimentation;
using Microsoft.Omex.Gating.UnitTests.Shared;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.Model.Types;
using Microsoft.Omex.System.UnitTests.Shared;
using Xunit;

namespace Microsoft.Omex.Gating.UnitTests
{
	/// <summary>
	/// Unit tests for gates.
	/// </summary>
	public sealed class GateUnitTests : UnitTestBase
	{
		private UnitTestGateDataSet UnitTestGates { get; } = new UnitTestGateDataSet(ResourceNames.Gates, ResourceNames.TestGroups);


		#region No restrictions
		[Fact]
		public void PerformAction_WhenNoRestrictionsAreSpecified_RunsAction()
		{
			Gate gate = new Gate("default");
			UnitTestGates.AddGateOverride(gate.Name, gate);

			bool result = false;
			IGate foundGate = UnitTestGates.GetGate(gate.Name);

			IGateContext gateContext = new GateContext(new UnitTestGatedRequest(), new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
					new GatedAction(foundGate,
						() => { result = true; }));

			Assert.True(result, "Expected action to be run.");
			Assert.True(gateContext.ActivatedGates.Contains(gate), "Expected gate to have been registered as activated.");
		}
		#endregion


		#region Market restrictions
		[Fact]
		public void PerformAction_WhenMarketRestrictionIsSpecifiedAndMarketExistsInRequest_RunsAction()
		{
			Gate gate = new Gate("markets")
			{
				Markets = new HashSet<string>(new string[] { "en-us" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				Market = "en-us"
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			// ASSERT
			Assert.True(result, "Expected action to be run.");
			Assert.True(gateContext.ActivatedGates.Contains(gate), "Expected gate to have been registered as activated.");
		}


		/// <summary>
		/// When multiple market restrictions are specified and one market exists in the request the action should be performed
		/// </summary>
		[Fact]
		public void PerformAction_WhenMultipleMarketRestrictionsAreSpecifiedAndMarketExistsInRequest_RunsAction()
		{
			Gate gate = new Gate("markets")
			{
				Markets = new HashSet<string>(new string[] { "en-us", "fr-fr" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				Market = "en-us"
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.True(result, "Expected action to be run.");
			Assert.True(gateContext.ActivatedGates.Contains(gate), "Expected gate to be activated.");
		}


		/// <summary>
		/// When market restrictions are specified and the market does not exist in the request the action should not be performed
		/// </summary>
		[Fact]
		public void PerformAction_WhenMarketRestrictionIsSpecifiedAndMarketDoesNotExistInRequest_DoesNotRunAction()
		{
			Gate gate = new Gate("markets")
			{
				Markets = new HashSet<string>(new string[] { "en-us" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				Market = "fr-fr"
			};

			// ACT
			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			// ASSERT
			Assert.False(result, "Did not expected action to be run.");
			Assert.False(gateContext.ActivatedGates.Contains(gate), "Did not expected gate to be activated.");
		}


		/// <summary>
		/// When multiple market restrictions are specified and none of the markets exist in the request the action should not be performed
		/// </summary>
		[Fact]
		public void PerformAction_WhenMultipleMarketRestrictionsAreSpecifiedAndMarketDoesNotExistInRequest_DoesNotRunAction()
		{
			Gate gate = new Gate("markets")
			{
				Markets = new HashSet<string>(new string[] { "en-us", "es-es" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				Market = "fr-fr"
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.False(result, "Did not expected action to be run.");
			Assert.False(gateContext.ActivatedGates.Contains(gate), "Did not expected gate to be activated.");
		}


		/// <summary>
		/// When market restrictions are specified and no market exist in the request the action should not be performed
		/// </summary>
		[Fact]
		public void PerformAction_WhenMarketRestrictionIsSpecifiedAndMarketIsNotSpecifiedInRequest_DoesNotRunAction()
		{
			Gate gate = new Gate("markets")
			{
				Markets = new HashSet<string>(new string[] { "en-us" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest();

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.False(result, "Did not expected action to be run.");
			Assert.False(gateContext.ActivatedGates.Contains(gate), "Did not expected gate to be activated.");
		}
		#endregion


		#region Gate ID Overrides
		[Fact]
		public void PerformAction_WhenMarketRestrictionIsNotMetAndOverrideIsSpecifiedInContext_RunsAction()
		{
			Gate gate = new Gate("override")
			{
				Markets = new HashSet<string>(new string[] { "en-us" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				RequestedGateIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
				{
					gate.Name
				}
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.True(result, "Expected action to be run.");
			Assert.True(gateContext.ActivatedGates.Contains(gate), "Expected gate to be activated.");
		}


		[Fact]
		public void PerformAction_WhenMarketRestrictionIsNotMetAndOverrideIsIncorrectlySpecified_DoesNotRunAction()
		{
			Gate gate = new Gate("override")
			{
				Markets = new HashSet<string>(new string[] { "en-us" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				RequestedGateIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
				{
					"nooverride"
				}
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.False(result, "Did not expected action to be run.");
			Assert.False(gateContext.ActivatedGates.Contains(gate), "Did not expected gate to be activated.");
		}


		[Fact]
		public void PerformAction_WhenMarketRestrictionIsNotMetAndMultipleOverridesAreSpecified_RunsFirstMatchingAction()
		{
			// ARRANGE
			Gate gate = new Gate("override")
			{
				Markets = new HashSet<string>(new string[] { "en-us" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			Gate secondGate = new Gate("secondOverride")
			{
				Markets = new HashSet<string>(new string[] { "en-us" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(secondGate.Name, secondGate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				RequestedGateIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
				{
					"override",
					"secondOverride"
				}
			};

			// ACT
			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }),
				new GatedAction(UnitTestGates.GetGate(secondGate.Name),
					() => { result = false; }));

			// ASSERT
			Assert.True(result, "Expected action to be run.");
			Assert.True(gateContext.ActivatedGates.Contains(gate), "Expected gate to be activated.");
			Assert.False(gateContext.ActivatedGates.Contains(secondGate), "Expected second gate not to be activated.");
		}


		[Fact]
		public void PerformAction_WhenMarketRestrictionIsNotMetAndOneOverrideIsSpecified_RunsActionAgainstSpecifiedOverride()
		{
			Gate gate = new Gate("override")
			{
				Markets = new HashSet<string>(new string[] { "en-us" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			Gate secondGate = new Gate("secondOverride")
			{
				Markets = new HashSet<string>(new string[] { "en-us" }, StringComparer.OrdinalIgnoreCase)
			};
			UnitTestGates.AddGateOverride(secondGate.Name, secondGate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				RequestedGateIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
				{
					"secondOverride"
				}
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }),
				new GatedAction(UnitTestGates.GetGate(secondGate.Name),
					() => { result = false; }));

			Assert.False(result, "Did not expected action to be run.");
			Assert.False(gateContext.ActivatedGates.Contains(gate), "Did not expected gate to be activated.");
			Assert.True(gateContext.ActivatedGates.Contains(secondGate), "Expected second gate to be activated.");
		}
		#endregion


		#region Client Versions
		[Fact]
		public void PerformAction_WhenClientVersionIsSpecifiedAndRequestHasMatchingClientVersion_RunsAction()
		{
			Gate gate = new Gate("client")
			{
				ClientVersions = new SortedDictionary<string, RequiredClient>(StringComparer.OrdinalIgnoreCase)
			};
			RequiredClient requiredClient = new RequiredClient() { Name = "Sharepoint", MinVersion = new ProductVersion(16, 0), MaxVersion = null };
			gate.ClientVersions.Add(requiredClient.Name, requiredClient);
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				CallingClient = new GatedClient() { Name = "Sharepoint", Version = new ProductVersion(16, 1) }
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.True(result, "Expected action to be run.");
			Assert.True(gateContext.ActivatedGates.Contains(gate), "Expected gate to be activated.");
		}


		[Fact]
		public void PerformAction_WhenClientVersionIsSpecifiedAndRequestHasSameClientVersion_RunsAction()
		{
			Gate gate = new Gate("client")
			{
				ClientVersions = new SortedDictionary<string, RequiredClient>(StringComparer.OrdinalIgnoreCase)
			};
			RequiredClient requiredClient = new RequiredClient() { Name = "Sharepoint", MinVersion = new ProductVersion(16, 0), MaxVersion = null };
			gate.ClientVersions.Add(requiredClient.Name, requiredClient);
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				CallingClient = new GatedClient() { Name = "Sharepoint", Version = new ProductVersion(16, 0) }
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.True(result, "Expected action to be run.");
			Assert.True(gateContext.ActivatedGates.Contains(gate), "Expected gate to be activated.");
		}


		[Fact]
		public void PerformAction_WhenClientVersionIntervalIsSpecifiedAndRequestHasClientVersionWithinInterval_RunsAction()
		{
			Gate gate = new Gate("client")
			{
				ClientVersions = new SortedDictionary<string, RequiredClient>(StringComparer.OrdinalIgnoreCase)
			};
			RequiredClient requiredClient = new RequiredClient() { Name = "Sharepoint", MinVersion = new ProductVersion(16, 0), MaxVersion = new ProductVersion(16, 2) };
			gate.ClientVersions.Add(requiredClient.Name, requiredClient);
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				CallingClient = new GatedClient() { Name = "Sharepoint", Version = new ProductVersion(16, 1) }
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.True(result, "Expected action to be run.");
			Assert.True(gateContext.ActivatedGates.Contains(gate), "Expected gate to be activated.");
		}


		[Fact]
		public void PerformAction_WhenClientVersionIsSpecifiedAndRequestDoesNotHaveMatchingClientVersion_DoesNotRunAction()
		{
			Gate gate = new Gate("client")
			{
				ClientVersions = new SortedDictionary<string, RequiredClient>(StringComparer.OrdinalIgnoreCase)
			};
			RequiredClient requiredClient = new RequiredClient() { Name = "Sharepoint", MinVersion = new ProductVersion(16, 2), MaxVersion = null };
			gate.ClientVersions.Add(requiredClient.Name, requiredClient);
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				CallingClient = new GatedClient() { Name = "Sharepoint", Version = new ProductVersion(16, 1) }
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.False(result, "Did not expected action to be run.");
			Assert.False(gateContext.ActivatedGates.Contains(gate), "Did not expected gate to be activated.");
		}


		[Fact]
		public void PerformAction_WhenClientVersionIntervalIsSpecifiedAndRequestHasClientVersionOutsideInterval_DoesNotRunAction()
		{
			Gate gate = new Gate("client")
			{
				ClientVersions = new SortedDictionary<string, RequiredClient>(StringComparer.OrdinalIgnoreCase)
			};
			RequiredClient requiredClient = new RequiredClient() { Name = "Sharepoint", MinVersion = new ProductVersion(16, 0), MaxVersion = new ProductVersion(16, 2) };
			gate.ClientVersions.Add(requiredClient.Name, requiredClient);
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				CallingClient = new GatedClient() { Name = "Sharepoint", Version = new ProductVersion(16, 3) }
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.False(result, "Did not expected action to be run.");
			Assert.False(gateContext.ActivatedGates.Contains(gate), "Did not expected gate to be activated.");
		}


		[Fact]
		public void PerformAction_WhenClientVersionIntervalIsSpecifiedAndRequestHasClientMaxVersionExactlyOnInterval_DoesNotRunAction()
		{
			Gate gate = new Gate("client")
			{
				ClientVersions = new SortedDictionary<string, RequiredClient>(StringComparer.OrdinalIgnoreCase)
			};
			RequiredClient requiredClient = new RequiredClient() { Name = "Sharepoint", MinVersion = new ProductVersion(16, 0), MaxVersion = new ProductVersion(16, 2) };
			gate.ClientVersions.Add(requiredClient.Name, requiredClient);
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				CallingClient = new GatedClient() { Name = "Sharepoint", Version = new ProductVersion(16, 2) }
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.False(result, "Did not expected action to be run.");
			Assert.False(gateContext.ActivatedGates.Contains(gate), "Did not expected gate to be activated.");
		}

		#endregion


		#region User Groups
		[Fact]
		public void PerformAction_WhenTestGroupIsSpecifiedAndRequestHasMatchingUser_RunsAction()
		{
			Gate gate = new Gate("client")
			{
				Users = new HashSet<string>(new string[] { "test1@mydomain.xx" }, StringComparer.OrdinalIgnoreCase),
				UserTypes = UserGroupTypes.CustomGroup
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				Users = new List<GatedUser>()
				{
					new GatedUser() { IsDogfoodUser = false, UserIdentifier = "test1@mydomain.xx" }
				}
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.True(result, "Expected action to be run.");
			Assert.True(gateContext.ActivatedGates.Contains(gate), "Expected gate to be activated.");
		}


		[Fact]
		public void PerformAction_WhenTestGroupIsSpecifiedAndRequestDoesNotHaveMatchingUser_DoesNotRunAction()
		{
			Gate gate = new Gate("client")
			{
				Users = new HashSet<string>(new string[] { "test1@mydomain.xx" }, StringComparer.OrdinalIgnoreCase),
				UserTypes = UserGroupTypes.CustomGroup
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				Users = new List<GatedUser>()
				{
					new GatedUser() { IsDogfoodUser = false, UserIdentifier = "test3@mydomain.xx" }
				}
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.False(result, "Did not expected action to be run.");
			Assert.False(gateContext.ActivatedGates.Contains(gate), "Did not expected gate to be activated.");
		}


		[Fact]
		public void PerformAction_WhenUsergroupNoneIsSpecified_DoesNotRunAction()
		{
			Gate gate = new Gate("client")
			{
				UserTypes = UserGroupTypes.None
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				Users = new List<GatedUser>()
				{
					new GatedUser() { IsDogfoodUser = false, UserIdentifier = "test3@mydomain.xx" }
				}
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.False(result, "Did not expected action to be run.");
			Assert.False(gateContext.ActivatedGates.Contains(gate), "Did not expected gate to be activated.");
		}


		[Fact]
		public void PerformAction_WhenUsergroupNoneIsSpecifiedAndGateIsRequested_RunsAction()
		{
			Gate gate = new Gate("client")
			{
				UserTypes = UserGroupTypes.None
			};
			UnitTestGates.AddGateOverride(gate.Name, gate);

			UnitTestGatedRequest gatedRequest = new UnitTestGatedRequest
			{
				Users = new List<GatedUser>()
				{
					new GatedUser() { IsDogfoodUser = false, UserIdentifier = "test3@mydomain.xx" }
				},
				RequestedGateIds = new HashSet<string>(new string[] { gate.Name })
			};

			bool result = false;

			IGateContext gateContext = new GateContext(gatedRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			gateContext.PerformAction(
				new GatedAction(UnitTestGates.GetGate(gate.Name),
					() => { result = true; }));

			Assert.True(result, "Expected action to be run.");
			Assert.True(gateContext.ActivatedGates.Contains(gate), "Expected gate to be activated.");
		}
		#endregion
	}
}