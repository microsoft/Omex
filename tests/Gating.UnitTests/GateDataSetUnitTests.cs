// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Omex.Gating.Data;
using Microsoft.Omex.Gating.Experimentation;
using Microsoft.Omex.Gating.UnitTests.Shared;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.Model.Types;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared.Configuration;
using Xunit;

namespace Microsoft.Omex.Gating.UnitTests
{
	/// <summary>
	/// Unit tests for the GateDataSet
	/// </summary>
	public sealed class GateDataSetUnitTests : UnitTestBase
	{
		[Fact]
		public void Load_WithIncorrectXml_ShouldReturnIncorrectDataSet()
		{
			FailOnErrors = false;

			GateDataSet dataSet = LoadGateDataSet(IncorrectXml);

			Assert.NotNull(dataSet);
			Assert.Equal(dataSet.Gates.Count(), 0);
		}


		[Fact]
		public void Load_WithDifferentSchema_ShouldReturnIncorrectDataSet()
		{
			FailOnErrors = false;

			GateDataSet dataSet = LoadGateDataSet(DifferentSchema);

			Assert.NotNull(dataSet);
			Assert.Equal(dataSet.GateNames.Count(), 0);
		}


		[Fact]
		public void RetrieveAllGates_WithDataSetUsingOnlyOneGate_ShouldReturnOnlyOneGate()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidSimple);

			IEnumerable<IGate> gates = dataSet.Gates;
			IGate gate = gates.FirstOrDefault<IGate>();

			Assert.NotNull(gate);
			Assert.Equal(gate.Name, "MyProduct.Test");

			Assert.NotNull(gate.Markets);
			Assert.Equal(gate.Markets.Count, 1);
			Assert.Equal(gate.Markets.First(), "en-us");

			Assert.NotNull(gate.ClientVersions);
			Assert.Equal(gate.ClientVersions.Count, 1);
			Assert.Equal(gate.ClientVersions.Keys.First(), "ClientOne");
		}


		[Fact]
		public void RetrieveAllGates_WithDataSetUsingOnlyOneGateWithEnvironments_ShouldReturnOnlyOneGate()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidSimpleWithEnvironments);

			IEnumerable<IGate> gates = dataSet.Gates;
			IGate gate = gates.FirstOrDefault<IGate>();

			Assert.NotNull(gate);
			Assert.Equal(gate.Name, "MyProduct.Test");

			Assert.NotNull(gate.Markets);
			Assert.Equal(gate.Markets.Count, 1);
			Assert.Equal(gate.Markets.First(), "en-us");

			Assert.NotNull(gate.Environments);
			Assert.Equal(gate.Environments.Count, 2);
			Assert.Equal(gate.Environments.First(), "PreProduction");
			Assert.Equal(gate.Environments.ElementAt(1), "Integration");

			Assert.NotNull(gate.ClientVersions);
			Assert.Equal(gate.ClientVersions.Count, 1);
			Assert.Equal(gate.ClientVersions.Keys.First(), "ClientOne");
		}


		[Fact]
		public void ClientVersions_WhereParentsHaveNoClientVersionsSpecified_ShouldReturnLeafClientVersions()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidSimpleNested);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate.ClientVersions);
			Assert.Equal(gate.ClientVersions.Count, 1);
			Assert.Equal(gate.ClientVersions.Keys.First(), "ClientOne");

			Assert.NotNull(gate.ParentGate);

			RequiredClient client = gate.ClientVersions["ClientOne"];
			Assert.NotNull(client);
			Assert.Equal(client.Name, "ClientOne");
			Assert.Equal(client.MinVersion, ProductVersion.Parse("16.1"));
			Assert.Equal(client.MaxVersion, ProductVersion.Parse("16.3"));
		}


		[Fact]
		public void ClientVersions_WhereParentsHaveNoClientVersionsAndEnvironmentsSpecified_ShouldReturnLeafClientVersionsAndEnvironments()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidSimpleNestedWithEnvironments);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate.ClientVersions);
			Assert.Equal(gate.ClientVersions.Count, 1);
			Assert.Equal(gate.ClientVersions.Keys.First(), "ClientOne");

			Assert.NotNull(gate.ParentGate);

			Assert.NotNull(gate.Environments);
			Assert.Equal(gate.Environments.Count, 2);
			Assert.Equal(gate.Environments.First(), "PreProduction");
			Assert.Equal(gate.Environments.ElementAt(1), "Integration");

			RequiredClient client = gate.ClientVersions["ClientOne"];
			Assert.NotNull(client);
			Assert.Equal(client.Name, "ClientOne");
			Assert.Equal(client.MinVersion, ProductVersion.Parse("16.1"));
			Assert.Equal(client.MaxVersion, ProductVersion.Parse("16.3"));
		}


		[Fact]
		public void Load_DataSetFileWithMissingParentGate_ShouldReturnDataSetWithNoGates()
		{
			FailOnErrors = false;
			GateDataSet dataSet = LoadGateDataSet(MissingParentGate);

			Assert.Equal(dataSet.Gates.Count(), 0);
		}


		[Fact]
		public void Load_NestedGates_ShouldReturnLeafGateWithDenormalizedRestrictions()
		{
			FailOnErrors = false;
			GateDataSet dataSet = LoadGateDataSet(InheritedRestrictions);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate.Markets);
			Assert.Equal(gate.Markets.Count, 1);
			Assert.Equal(gate.Markets.First(), "en-ie");

			Assert.NotNull(gate.ClientVersions);

			RequiredClient client = gate.ClientVersions.Values.Single();
			Assert.NotNull(client);
			Assert.Equal(client.Name, "ClientOne");
			Assert.Equal(client.MinVersion, ProductVersion.Parse("16.2"));
			Assert.Equal(client.MaxVersion, ProductVersion.Parse("16.3"));
		}


		[Fact]
		public void Load_NestedGatesWithEnvironments_ShouldReturnLeafGateWithDenormalizedRestrictions()
		{
			FailOnErrors = false;
			GateDataSet dataSet = LoadGateDataSet(InheritedRestrictionsWithEnvironments);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate.Markets);
			Assert.Equal(gate.Markets.Count, 1);
			Assert.Equal(gate.Markets.First(), "en-ie");

			Assert.NotNull(gate.Environments);
			Assert.Equal(gate.Environments.Count, 1);
			Assert.Equal(gate.Environments.First(), "PreProduction");

			Assert.NotNull(gate.ClientVersions);

			RequiredClient client = gate.ClientVersions.Values.Single();
			Assert.NotNull(client);
			Assert.Equal(client.Name, "ClientOne");
			Assert.Equal(client.MinVersion, ProductVersion.Parse("16.2"));
			Assert.Equal(client.MaxVersion, ProductVersion.Parse("16.3"));
		}


		[Fact]
		public void UserGroup_WhereParentsHaveNoneUserGroupSpecified_ShouldOverrideChildUserGroup()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidUserGroupNested);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.Equal(gate.UserTypes, UserGroupTypes.None);
			Assert.NotNull(gate.ParentGate);
			Assert.Equal(gate.ParentGate.UserTypes, UserGroupTypes.None);

			UnitTestGatedRequest request = new UnitTestGatedRequest
			{
				Users = new List<GatedUser>()
			{
				new GatedUser() { IsDogfoodUser = false, UserIdentifier = "test3@mydomain.xx" }
			}
			};

			IGateContext gateContext = new GateContext(request, new BasicMachineInformation(), new DefaultExperimentContext());
			Assert.False(gateContext.IsGateApplicable(gate));
		}


		[Fact]
		public void UserGroup_WhereParentsHaveNoneUserGroupAndNoneEnvironmentSpecified_ShouldOverrideChildUserGroupAndEnvironment()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidUserGroupAndEnvironmentNested);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.Equal(gate.UserTypes, UserGroupTypes.None);
			Assert.NotNull(gate.ParentGate);
			Assert.Equal(gate.ParentGate.UserTypes, UserGroupTypes.None);

			Assert.NotNull(gate.Environments);
			Assert.Equal(gate.Environments.Count, 0);

			UnitTestGatedRequest request = new UnitTestGatedRequest
			{
				Users = new List<GatedUser>()
			{
				new GatedUser() { IsDogfoodUser = false, UserIdentifier = "test3@mydomain.xx" }
			}
			};

			IGateContext gateContext = new GateContext(request, new BasicMachineInformation(), new DefaultExperimentContext());
			Assert.False(gateContext.IsGateApplicable(gate));
		}


		[Fact]
		public void AllowedBrowsers_WithValidSimpleConstraintsOnBrowser_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidSimpleConstraintsOnBrowser);

			IGate parentGate = dataSet.GetGate("MyProduct.TestGate");
			IGate childGate = dataSet.GetGate("MyProduct.TestGate.Browsers");

			Assert.NotNull(parentGate);
			Assert.NotNull(childGate);
			Assert.NotNull(childGate.AllowedBrowsers);

			Assert.Equal(childGate.AllowedBrowsers.Count, 3);
			Assert.Equal(childGate.AllowedBrowsers["BrowserThree"].Count, 5);
			Assert.Equal(childGate.AllowedBrowsers["BrowserOne"].Count, 2);
			Assert.Equal(childGate.AllowedBrowsers["BrowserFour"].Count, 0);
		}


		[Fact]
		public void AllowedBrowsers_WithValidNestedConstraintsOnBrowser_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidNestedConstraintsOnBrowser);

			IGate parentGate = dataSet.GetGate("MyProduct.TestGate");
			IGate childGate = dataSet.GetGate("MyProduct.TestGate.Browsers");

			Assert.NotNull(parentGate);
			Assert.NotNull(childGate);

			Assert.NotNull(childGate.AllowedBrowsers);
			Assert.NotNull(parentGate.AllowedBrowsers);

			Assert.Equal(parentGate.AllowedBrowsers.Count, 5);
			Assert.Equal(parentGate.AllowedBrowsers["BrowserOne"].Count, 3);
			Assert.Equal(parentGate.AllowedBrowsers["BrowserTwo"].Count, 2);
			Assert.Equal(parentGate.AllowedBrowsers["BrowserThree"].Count, 3);
			Assert.Equal(parentGate.AllowedBrowsers["BrowserFour"].Count, 0);
			Assert.Equal(parentGate.AllowedBrowsers["BrowserFive"].Count, 2);

			// Child gate's browser type after consolidation
			Assert.Equal(childGate.AllowedBrowsers.Count, 4);
			Assert.Equal(childGate.AllowedBrowsers["BrowserOne"].Count, 2);
			Assert.Equal(childGate.AllowedBrowsers["BrowserTwo"].Count, 2);
			Assert.Equal(childGate.AllowedBrowsers["BrowserThree"].Count, 3);
			Assert.Equal(childGate.AllowedBrowsers["BrowserFour"].Count, 2);
		}


		[Fact]
		public void BlockedBrowsers_WithValidSimpleConstraintsOnBlockedBrowsers_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidSimpleConstraintsOnBlockedBrowsers);

			IGate parentGate = dataSet.GetGate("MyProduct.TestGate");
			IGate childGate = dataSet.GetGate("MyProduct.TestGate.BlockedBrowsers");

			Assert.NotNull(parentGate);
			Assert.NotNull(childGate);

			Assert.Null(parentGate.BlockedBrowsers);

			Assert.NotNull(childGate);

			Assert.Equal(childGate.BlockedBrowsers.Count, 3);
			Assert.Equal(childGate.BlockedBrowsers["BrowserThree"].Count, 5);
			Assert.Equal(childGate.BlockedBrowsers["BrowserOne"].Count, 2);
			Assert.Equal(childGate.BlockedBrowsers["BrowserFour"].Count, 0);
		}


		[Fact]
		public void BlockedBrowsers_WithValidNestedConstraintsOnBlockedBrowser_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidNestedConstraintsOnBlockedBrowsers);

			IGate parentGate = dataSet.GetGate("MyProduct.TestGate");
			IGate childGate = dataSet.GetGate("MyProduct.TestGate.BlockedBrowsers");

			Assert.NotNull(parentGate);
			Assert.NotNull(childGate);

			Assert.NotNull(childGate.BlockedBrowsers);
			Assert.NotNull(parentGate.BlockedBrowsers);

			Assert.Equal(parentGate.BlockedBrowsers.Count, 5);
			Assert.Equal(parentGate.BlockedBrowsers["BrowserOne"].Count, 3);
			Assert.Equal(parentGate.BlockedBrowsers["BrowserTwo"].Count, 2);
			Assert.Equal(parentGate.BlockedBrowsers["BrowserThree"].Count, 3);
			Assert.Equal(parentGate.BlockedBrowsers["BrowserFour"].Count, 0);
			Assert.Equal(parentGate.BlockedBrowsers["BrowserFive"].Count, 2);

			// Child gate's browser type after consolidation
			Assert.Equal(childGate.BlockedBrowsers.Count, 4);
			Assert.Equal(childGate.BlockedBrowsers["BrowserOne"].Count, 2);
			Assert.Equal(childGate.BlockedBrowsers["BrowserTwo"].Count, 2);
			Assert.Equal(childGate.BlockedBrowsers["BrowserThree"].Count, 3);
			Assert.Equal(childGate.BlockedBrowsers["BrowserFour"].Count, 2);
		}


		[Fact]
		public void Browsers_WithNotValidGateXml_ShouldNotLoadCorrectly()
		{
			FailOnErrors = false;
			GateDataSet dataSet = LoadGateDataSet(NotValidGateXmlWithBothAllowedAndBlockedBrowsers);

			IGate gate = dataSet.GetGate("MyProduct.TestGate.NotValidGateXml");

			Assert.Null(gate);
		}



		[Fact]
		public void Load_GateSimpleDates_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidSimpleWithDates);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.True(gate.StartDate.HasValue);
			Assert.Equal(gate.StartDate.Value, DateTime.Parse("2014-01-18T12:22:10Z", CultureInfo.InvariantCulture).ToUniversalTime());
			Assert.True(gate.EndDate.HasValue);
			Assert.Equal(gate.EndDate.Value, DateTime.Parse("2015-01-18T14:12:50Z", CultureInfo.InvariantCulture).ToUniversalTime());
		}


		[Fact]
		public void Load_GateNestedDates_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidNestedWithDates);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.True(gate.StartDate.HasValue);
			Assert.Equal(gate.StartDate.Value, DateTime.Parse("2014-01-18T19:12:12Z", CultureInfo.InvariantCulture).ToUniversalTime());
			Assert.True(gate.EndDate.HasValue);
			Assert.Equal(gate.EndDate.Value, DateTime.Parse("2015-01-18T08:12:24Z", CultureInfo.InvariantCulture).ToUniversalTime());
		}


		[Fact]
		public void Load_GatedReleasePlanWithOneReleaseGate_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ReleasePlanWithOneReleaseGate);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.ReleasePlan);

			Assert.Equal(gate.ReleasePlan.Length, 1);
			Assert.Equal(gate.ReleasePlan[0].Name, "MyProduct.WebService.Feature.ReleasePlan.Integration");
		}


		[Fact]
		public void Load_GatedReleasePlanWithOneReleaseGate_ReleaseGateShouldHaveReleasePlanName()
		{
			GateDataSet dataSet = LoadGateDataSet(ReleasePlanWithOneReleaseGate);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.ReleasePlan);

			Assert.Equal(gate.ReleasePlan.Length, 1);
			Assert.Equal(gate.ReleasePlan[0].Name, "MyProduct.WebService.Feature.ReleasePlan.Integration");
			Assert.Equal(gate.ReleasePlan[0].RepleasePlanGateName, "MyProduct.Test");
			Assert.Null(gate.RepleasePlanGateName);
		}


		[Fact]
		public void Load_GatedReleasePlanWithMultipleReleaseGates_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ReleasePlanWithMultipleReleaseGates);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.ReleasePlan);

			Assert.Equal(gate.ReleasePlan.Length, 3);
			Assert.Equal(gate.ReleasePlan[0].Name, "MyProduct.WebService.Feature.ReleasePlan.Integration");
			Assert.Equal(gate.ReleasePlan[1].Name, "MyProduct.WebService.Feature.ReleasePlan.PreProduction");
			Assert.Equal(gate.ReleasePlan[2].Name, "MyProduct.WebService.Feature.ReleasePlan.Production");
		}


		[Fact]
		public void Load_GatedReleasePlanWithSameReleaseGates_ShouldLoadCorrectly()
		{
			FailOnErrors = false;
			GateDataSet dataSet = LoadGateDataSet(ReleasePlanWithSameReleaseGates);

			Assert.NotNull(dataSet);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.ReleasePlan);

			Assert.Equal(gate.ReleasePlan.Length, 3);
			Assert.Equal(gate.ReleasePlan[0].Name, "MyProduct.WebService.Feature.ReleasePlan.Integration");
			Assert.Equal(gate.ReleasePlan[1].Name, "MyProduct.WebService.Feature.ReleasePlan.PreProduction");
			Assert.Equal(gate.ReleasePlan[2].Name, "MyProduct.WebService.Feature.ReleasePlan.Production");
		}


		[Fact]
		public void Load_GatedReleasePlanWithConsolidationFromContainingGate_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ReleasePlanWithConsolidationFromContainingGate);

			Assert.NotNull(dataSet);
			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.ReleasePlan);

			Assert.Equal(gate.ReleasePlan.Length, 2);

			Assert.Equal(gate.ReleasePlan[0].Name, "MyProduct.Test.ReleasePlan.Integration");
			Assert.Equal(gate.ReleasePlan[1].Name, "MyProduct.Test.ReleasePlan.Other");

			Assert.Equal(gate.ReleasePlan[0].Markets.Count, 2);
			Assert.Equal(gate.ReleasePlan[1].Markets.Count, 1);

			Assert.Equal(gate.ReleasePlan[0].HostEnvironments.Count, 1);
			Assert.Equal(gate.ReleasePlan[1].HostEnvironments.Count, 3);

			Assert.Equal(gate.ReleasePlan[0].Users.Count, 4);
			Assert.Equal(gate.ReleasePlan[1].Users.Count, 2);
		}


		[Fact]
		public void Load_ExperimentalGate_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidExperimentalGate);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.ExperimentInfo);
			Assert.Equal(gate.ExperimentInfo.ExperimentName, "TestExperiment");
			Assert.Equal(gate.ExperimentInfo.ExperimentWeight, (uint)10);

			IExperiments experiments = dataSet.Experiments;
			IEnumerable<IGate> gates = experiments.GetExperiment("TestExperiment").Gates;

			Assert.Equal(gates.Count(), 1);
			Assert.Equal(gates.First().Name, "MyProduct.Test");
			Assert.Equal(gates.First().ParentGate.Name, "MyProduct");
		}


		[Fact]
		public void Load_GateWithParentExperimentalGate_ShouldNotLoadCorrectly()
		{
			FailOnErrors = false;
			GateDataSet dataSet = LoadGateDataSet(ParentExperimentalGate);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.ExperimentInfo);
			Assert.Equal(gate.ExperimentInfo.ExperimentName, "TestExperiment");
			Assert.Equal(gate.ExperimentInfo.ExperimentWeight, (uint)10);

			IExperiments experiments = dataSet.Experiments;
			IEnumerable<IGate> gates = experiments.GetExperiment("TestExperiment").Gates;

			Assert.Equal(gates.Count(), 1);
			Assert.Equal(gates.First().Name, "MyProduct.Test");

			gate = dataSet.GetGate("MyProduct.ParentExperiment");
			Assert.Null(gate);
		}


		[Fact]
		public void Load_ExperimentsWithReleasePlan_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ExperimentWithReleasePlan);

			IGate gate1 = dataSet.GetGate("MyProduct.TestExperiment1");

			Assert.NotNull(gate1);
			Assert.NotNull(gate1.ExperimentInfo);

			Assert.Equal(gate1.ExperimentInfo.ExperimentName, "TestExperiment");
			Assert.Equal(gate1.ExperimentInfo.ExperimentWeight, (uint)10);

			IGate gate2 = dataSet.GetGate("MyProduct.TestExperiment2");

			Assert.NotNull(gate2);
			Assert.NotNull(gate2.ExperimentInfo);
			Assert.Equal(gate2.ExperimentInfo.ExperimentName, "TestExperiment");
			Assert.Equal(gate2.ExperimentInfo.ExperimentWeight, (uint)5);

			IExperiments experiments = dataSet.Experiments;
			IEnumerable<IGate> gates = experiments.GetExperiment("TestExperiment").Gates;

			Assert.Equal(gates.Count(), 2);
			Assert.Equal(gates.ElementAt(0).ReleasePlan.Length, 2);
			Assert.Equal(gates.ElementAt(1).ReleasePlan.Length, 2);

			Assert.Equal(gates.ElementAt(0).ReleasePlan[0].Name, "MyProduct.TestExperiment1.ReleaseGate1");
			Assert.Equal(gates.ElementAt(0).ReleasePlan[1].Name, "MyProduct.TestExperiment1.ReleaseGate2");
			Assert.Equal(gates.ElementAt(1).ReleasePlan[0].Name, "MyProduct.TestExperiment2.ReleaseGate1");
			Assert.Equal(gates.ElementAt(1).ReleasePlan[1].Name, "MyProduct.TestExperiment2.ReleaseGate2");
		}


		[Fact]
		public void Load_GateSimpleVersionRange_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidSimpleWithVersionRange);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.ClientVersions);

			RequiredClient client = gate.ClientVersions.Values.Single();
			Assert.NotNull(client);
			Assert.Equal(client.Name, "ClientOne");
			Assert.Equal(client.VersionRanges.Count, 1);
			Assert.Equal(client.VersionRanges.First().Min, ProductVersion.Parse("16.1"));
			Assert.Equal(client.VersionRanges.First().Max, ProductVersion.Parse("16.3"));
		}


		[Fact]
		public void Load_GateSimpleAudienceGroup_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidSimpleWithAudienceGroup);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.ClientVersions);

			RequiredClient client = gate.ClientVersions.Values.Single();
			Assert.NotNull(client);
			Assert.Equal(client.Name, "ClientOne");
			Assert.Equal(client.AudienceGroup, "ClientLoop");
		}


		[Fact]
		public void Load_GateSimpleAppOverride_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ValidSimpleWithApplicationOverride);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.ClientVersions);

			RequiredClient client = gate.ClientVersions.Values.Single();
			Assert.NotNull(client);
			Assert.Equal(client.Name, "ClientOne");
			Assert.NotNull(client.Overrides);

			RequiredApplication app = client.Overrides.Values.Single();
			Assert.NotNull(app);
			Assert.Equal(app.Name, "8");
			Assert.Equal(app.MinVersion, ProductVersion.Parse("16.8"));
			Assert.Equal(app.MaxVersion, ProductVersion.Parse("16.9"));
			Assert.Equal(app.AudienceGroup, "AppLoop");
		}


		[Fact]
		public void Load_GatedSimpleServices_ShouldLoadCorrectly()
		{
			FailOnErrors = false;
			GateDataSet dataSet = LoadGateDataSet(ValidSimpleServices);

			Assert.NotNull(dataSet);

			IGate gate = dataSet.GetGate("MyProduct.WebSite.Test.Services");
			Assert.NotNull(gate);
			Assert.NotNull(gate.Services);
			Assert.Equal(gate.Services.Count, 4);

			Assert.True(gate.Services.TryGetValue("ServiceOne", out GatedServiceTypes serviceFlags));
			Assert.Equal(serviceFlags.ToString(), GatedServiceTypes.All.ToString());

			Assert.True(gate.Services.TryGetValue("ServiceThree", out serviceFlags));
			Assert.Equal(serviceFlags.ToString(), GatedServiceTypes.CanaryService.ToString());

			Assert.True(gate.Services.TryGetValue("ServiceTwo", out serviceFlags));
			Assert.Equal(serviceFlags.ToString(), GatedServiceTypes.FullService.ToString());

			Assert.True(gate.Services.TryGetValue("ServiceFour", out serviceFlags));
			Assert.Equal(serviceFlags.ToString(), GatedServiceTypes.All.ToString());
		}


		[Fact]
		public void Load_GatedNestedServices_ShouldLoadCorrectly()
		{
			FailOnErrors = false;
			GateDataSet dataSet = LoadGateDataSet(ValidNestedServices);

			Assert.NotNull(dataSet);

			IGate gate = dataSet.GetGate("MyProduct.WebSite.Test.Services");

			Assert.NotNull(gate);
			Assert.NotNull(gate.Services);

			Assert.Equal(gate.Services.Count, 5);

			Assert.True(gate.Services.TryGetValue("ServiceOne", out GatedServiceTypes serviceFlags));
			Assert.Equal(serviceFlags.ToString(), GatedServiceTypes.FullService.ToString());

			Assert.True(gate.Services.TryGetValue("ServiceTwo", out serviceFlags));
			Assert.Equal(serviceFlags.ToString(), GatedServiceTypes.CanaryService.ToString());

			Assert.True(gate.Services.TryGetValue("ServiceFour", out serviceFlags));
			Assert.Equal(serviceFlags.ToString(), GatedServiceTypes.All.ToString());

			Assert.True(gate.Services.TryGetValue("ServiceFive", out serviceFlags));
			Assert.Equal(serviceFlags.ToString(), GatedServiceTypes.None.ToString());

			Assert.True(gate.Services.TryGetValue("ServiceSeven", out serviceFlags));
			Assert.Equal(serviceFlags.ToString(), GatedServiceTypes.None.ToString());
		}


		[Fact]
		public void Load_GateOneCloudContext_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(OneCloudContext);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.CloudContexts);
			Assert.Equal(gate.CloudContexts.Count(), 1);
			Assert.Equal(gate.CloudContexts.Single(), "Public");
		}


		[Fact]
		public void Load_GateThreeCloudContexts_ShouldLoadCorrectly()
		{
			GateDataSet dataSet = LoadGateDataSet(ThreeCloudContext);

			IGate gate = dataSet.GetGate("MyProduct.Test");

			Assert.NotNull(gate);
			Assert.NotNull(gate.CloudContexts);
			Assert.Equal(gate.CloudContexts.Count(), 3);
			Assert.True(gate.CloudContexts.Contains("Public"));
			Assert.True(gate.CloudContexts.Contains("Sovereign"));
			Assert.True(gate.CloudContexts.Contains("Local"));
		}


		/// <summary>
		/// Loads GateDataSet with required contents
		/// </summary>
		/// <param name="content">gate resource content</param>
		/// <returns>Loaded Gates DataSet</returns>
		private static GateDataSet LoadGateDataSet(string content)
		{
			IDictionary<string, IResourceDetails> resources =
				new Dictionary<string, IResourceDetails>(2, StringComparer.OrdinalIgnoreCase)
			{
				{ ResourceNames.Gates, EmbeddedResources.CreateResourceDetails(content) },
				{ ResourceNames.TestGroups, EmbeddedResources.CreateResourceDetails(ValidDataRaw) }
			};
			GateDataSet dataSet = new GateDataSet(ResourceNames.Gates, ResourceNames.TestGroups);
			dataSet.Load(resources);
			return dataSet;
		}


		/// <summary>
		/// Valid test groups xml data
		/// </summary>
		private const string ValidDataRaw =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestGroups xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<TestGroup name=""Test"">
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
		<Member email=""test2@mydomain.xx"" alias=""myalias""/>
		<Member deploymentId=""{E5C91678-399E-46ef-B5BF-D619AEACD050}"" alias=""myalias""/>
		<Member deploymentId=""{3D5ABF05-6148-4594-9BBE-A3CB2A34EBF8}"" alias=""myalias""/>
	</TestGroup>
	<TestGroup name=""Automation"">
		<Member email=""test3@mydomain.xx"" alias=""myalias""/>
		<Member email=""test1@mydomain.xx"" alias=""myalias""/>
	</TestGroup>
	<TestGroup name=""Probes"">
		<Member deploymentId=""{3D5ABF05-6148-4594-9BBE-A3CB2A34EBF8}"" alias=""myalias""/>
		<Member deploymentId=""{8499ED16-258C-49d4-A1E0-C9E1B1460FDC}"" alias=""myalias""/>
	</TestGroup>
</TestGroups>";


		/// <summary>
		/// Simple valid gating file
		/// </summary>
		private const string ValidSimple =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
		<UserGroups>
			<UserGroup Type=""Dogfood""/>
		</UserGroups>
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.3""/>
		</ClientVersions>
	</Gate>
</Gates>";


		/// <summary>
		/// Simple valid gating file with environments
		/// </summary>
		private const string ValidSimpleWithEnvironments =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
			<Environment Name=""Integration""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""Dogfood""/>
		</UserGroups>
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.3""/>
		</ClientVersions>
	</Gate>
</Gates>";


		/// <summary>
		/// Incorrect Xml
		/// </summary>
		private const string IncorrectXml =
			@"This is not xml";


		/// <summary>
		/// Different schema
		/// </summary>
		private const string DifferentSchema =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
	</Gate>
</Root>";


		/// <summary>
		/// Simple valid nested gating file
		/// </summary>
		private const string ValidSimpleNested =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
		<UserGroups>
			<UserGroup Type=""Dogfood""/>
		</UserGroups>
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.3""/>
		</ClientVersions>
		<ParentGate Name=""MyProduct""/>
	</Gate>
	<Gate Name=""MyProduct"">
	</Gate>
</Gates>";


		/// <summary>
		/// Simple valid nested gating file with environments
		/// </summary>
		private const string ValidSimpleNestedWithEnvironments =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
			<Environment Name=""Integration""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""Dogfood""/>
		</UserGroups>
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.3""/>
		</ClientVersions>
		<ParentGate Name=""MyProduct""/>
	</Gate>
	<Gate Name=""MyProduct"">
	</Gate>
</Gates>";


		/// <summary>
		/// User group nesting
		/// </summary>
		private const string ValidUserGroupNested =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
		<UserGroups>
			<UserGroup Type=""Dogfood""/>
		</UserGroups>
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.3""/>
		</ClientVersions>
		<ParentGate Name=""MyProduct""/>
	</Gate>
	<Gate Name=""MyProduct"">
		<UserGroups>
			<UserGroup Type=""None""/>
		</UserGroups>
	</Gate>
</Gates>";


		/// <summary>
		/// User group and environment nesting
		/// </summary>
		private const string ValidUserGroupAndEnvironmentNested =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
			<Environment Name=""Integration""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""Dogfood""/>
		</UserGroups>
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.3""/>
		</ClientVersions>
		<ParentGate Name=""MyProduct""/>
	</Gate>
	<Gate Name=""MyProduct"">
		<Environments>
			<Environment Name=""None""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""None""/>
		</UserGroups>
	</Gate>
</Gates>";


		/// <summary>
		/// Gating file with a missing parent gate
		/// </summary>
		private const string MissingParentGate =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<ParentGate Name=""MyProduct""/>
	</Gate>
</Gates>";


		/// <summary>
		/// Gating file with nested requirements
		/// </summary>
		private const string InheritedRestrictions =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Test""/>
		</UserGroups>
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.3""/>
			<ClientVersion Name=""ClientTwo"" MinVersion=""16.1"" MaxVersion=""16.3""/>
		</ClientVersions>
		<ParentGate Name=""MyProduct""/>
	</Gate>
	<Gate Name=""MyProduct"">
		<Markets>
			<Market Name=""en-gb""/>
			<Market Name=""en-ie""/>
		</Markets>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Automation""/>
		</UserGroups>
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.2"" MaxVersion=""16.4""/>
		</ClientVersions>
	</Gate>
</Gates>";


		/// <summary>
		/// Gating file with nested requirements with environments
		/// </summary>
		private const string InheritedRestrictionsWithEnvironments =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
			<Environment Name=""Integration""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Test""/>
		</UserGroups>
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.3""/>
			<ClientVersion Name=""ClientTwo"" MinVersion=""16.1"" MaxVersion=""16.3""/>
		</ClientVersions>
		<ParentGate Name=""MyProduct""/>
	</Gate>
	<Gate Name=""MyProduct"">
		<Markets>
			<Market Name=""en-gb""/>
			<Market Name=""en-ie""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Automation""/>
		</UserGroups>
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.2"" MaxVersion=""16.4""/>
		</ClientVersions>
	</Gate>
</Gates>";


		/// <summary>
		/// Start date and end dates
		/// </summary>
		private const string ValidSimpleWithDates =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<StartDate Value=""2014-01-18T12:22:10Z"" />
		<EndDate Value=""2015-01-18T14:12:50Z"" />
	</Gate>
</Gates>";


		/// <summary>
		/// Dates inherited from parent
		/// </summary>
		private const string ValidNestedWithDates =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<StartDate Value=""2013-01-18T12:12:12Z"" />
		<EndDate Value=""2016-01-18T00:00:00Z"" />
		<ParentGate Name=""MyProduct""/>
	</Gate>
	<Gate Name=""MyProduct"">
		<StartDate Value=""2014-01-18T19:12:12Z"" />
		<EndDate Value=""2015-01-18T08:12:24Z"" />
	</Gate>
</Gates>";


		/// <summary>
		/// Valid gates with experiment
		/// </summary>
		private const string ValidExperimentalGate =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct"">
		<Environments>
			<Environment Name=""None""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""None""/>
		</UserGroups>
	</Gate>
	<Experiment Name= ""TestExperiment"">
		<ExperimentGate Name=""MyProduct.Test"" Weight = ""10"">
			<Markets>
				<Market Name=""en-us""/>
			</Markets>
			<Environments>
				<Environment Name=""PreProduction""/>
				<Environment Name=""Integration""/>
			</Environments>
			<UserGroups>
				<UserGroup Type=""Dogfood""/>
			</UserGroups>
			<ClientVersions>
				<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.3""/>
			</ClientVersions>
			<ParentGate Name=""MyProduct""/>
		</ExperimentGate>
	</Experiment>
</Gates>";


		/// <summary>
		/// Gates with experiment parent gate
		/// </summary>
		private const string ParentExperimentalGate =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct"">
		<Environments>
			<Environment Name=""None""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""None""/>
		</UserGroups>
	</Gate>
	<Gate Name=""MyProduct.ParentExperiment"">
		<ParentGate Name=""MyProduct.Test""/>
	</Gate>
	<Experiment Name= ""TestExperiment"">
		<ExperimentGate Name=""MyProduct.Test"" Weight = ""10"">
			<Markets>
				<Market Name=""en-us""/>
			</Markets>
			<Environments>
				<Environment Name=""PreProduction""/>
				<Environment Name=""Integration""/>
			</Environments>
			<UserGroups>
				<UserGroup Type=""Dogfood""/>
			</UserGroups>
			<ClientVersions>
				<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.3""/>
			</ClientVersions>
			<ParentGate Name=""MyProduct""/>
		</ExperimentGate>
	</Experiment>
</Gates>";


		/// <summary>
		/// Experiment with release plan.
		/// </summary>
		private const string ExperimentWithReleasePlan =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct"">
		<Environments>
			<Environment Name=""None""/>
		</Environments>
	</Gate>
	<Experiment Name=""TestExperiment"">
		<ExperimentGate Name=""MyProduct.TestExperiment1"" Weight=""10"">
			<Markets>
				<Market Name=""en-us""/>
				<Market Name=""en-gb""/>
				<Market Name=""en-ie""/>
			</Markets>
			<UserGroups>
				<UserGroup Type=""Dogfood""/>
			</UserGroups>
			<ParentGate Name=""MyProduct""/>
			<ReleasePlan>
				<ReleaseGate Name=""MyProduct.TestExperiment1.ReleaseGate1"">
					<Markets>
						<Market Name=""en-us""/>
					</Markets>
				</ReleaseGate>
				<ReleaseGate Name=""MyProduct.TestExperiment1.ReleaseGate2"">
					<UserGroups>
						<UserGroup Type=""Dogfood""/>
					</UserGroups>
				</ReleaseGate>
			</ReleasePlan>
		</ExperimentGate>
		<ExperimentGate Name=""MyProduct.TestExperiment2"" Weight=""5"">
			<Markets>
				<Market Name=""en-us""/>
				<Market Name=""en-gb""/>
				<Market Name=""en-ie""/>
			</Markets>
			<UserGroups>
				<UserGroup Type=""Dogfood""/>
			</UserGroups>
			<ParentGate Name=""MyProduct""/>
			<ReleasePlan>
				<ReleaseGate Name=""MyProduct.TestExperiment2.ReleaseGate1"">
					<Markets>
						<Market Name=""en-us""/>
					</Markets>
				</ReleaseGate>
				<ReleaseGate Name=""MyProduct.TestExperiment2.ReleaseGate2"">
					<Markets>
						<Market Name=""en-gb""/>
					</Markets>
				</ReleaseGate>
			</ReleasePlan>
		</ExperimentGate>
	</Experiment>
</Gates>";


		/// <summary>
		/// Browser type and versions for a gate.
		/// </summary>
		private const string ValidSimpleConstraintsOnBrowser =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.TestGate.Browsers"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
			<Environment Name=""Integration""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Test""/>
		</UserGroups>
		<Browsers>
			<AllowedBrowsers>
				<Browser Name=""BrowserOne"">
					<Version Value=""40"" />
					<Version Value=""41"" />
				</Browser>
				<Browser Name=""BrowserThree"">
					<Version Value=""9"" />
					<Version Value=""10"" />
					<Version Value=""11"" />
				</Browser>
				<Browser Name=""BrowserThree"">
					<Version Value=""11"" />
					<Version Value=""13"" />
					<Version Value=""14"" />
				</Browser>
				<Browser Name=""BrowserFour"" />
			</AllowedBrowsers>
		</Browsers>
		<ParentGate Name=""MyProduct.TestGate""/>
	</Gate>
	<Gate Name=""MyProduct.TestGate"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
		</Environments>
	</Gate>
</Gates>";


		/// <summary>
		/// Browser types and versions of a gate inherited from a parent gate.
		/// </summary>
		private const string ValidNestedConstraintsOnBrowser =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.TestGate.Browsers"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
			<Environment Name=""Integration""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Test""/>
		</UserGroups>
		<Browsers>
			<AllowedBrowsers>
				<Browser Name=""BrowserOne"">
					<Version Value=""40"" />
					<Version Value=""41"" />
				</Browser>
				<Browser Name=""BrowserTwo"" />
				<Browser Name=""BrowserThree"">
					<Version Value=""7"" />
					<Version Value=""8"" />
					<Version Value=""9"" />
					<Version Value=""10"" />
					<Version Value=""11"" />
				</Browser>
				<Browser Name=""BrowserFour"">
					<Version Value=""20"" />
					<Version Value=""22"" />
				</Browser>
			</AllowedBrowsers>
		</Browsers>
		<ParentGate Name=""MyProduct.TestGate""/>
	</Gate>
	<Gate Name=""MyProduct.TestGate"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
		</Environments>
		<Browsers>
			<AllowedBrowsers>
				<Browser Name=""BrowserOne"">
					<Version Value=""40"" />
					<Version Value=""41"" />
					<Version Value=""42"" />
				</Browser>
				<Browser Name=""BrowserTwo"">
					<Version Value=""36"" />
					<Version Value=""37"" />
				</Browser>
				<Browser Name=""BrowserThree"">
					<Version Value=""9"" />
					<Version Value=""10"" />
					<Version Value=""11"" />
				</Browser>
				<Browser Name=""BrowserFour"" />
				<Browser Name=""BrowserFive"">
					<Version Value=""20"" />
					<Version Value=""21"" />
				</Browser>
			</AllowedBrowsers>
		</Browsers>
	</Gate>
</Gates>";


		/// <summary>
		/// Browser type and versions for a gate for which the access is to be blocked.
		/// </summary>
		private const string ValidSimpleConstraintsOnBlockedBrowsers =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.TestGate.BlockedBrowsers"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Test""/>
		</UserGroups>
		<Browsers>
			<BlockedBrowsers>
				<Browser Name=""BrowserOne"">
					<Version Value=""40"" />
					<Version Value=""41"" />
				</Browser>
				<Browser Name=""BrowserThree"">
					<Version Value=""9"" />
					<Version Value=""10"" />
					<Version Value=""11"" />
				</Browser>
				<Browser Name=""BrowserThree"">
					<Version Value=""11"" />
					<Version Value=""13"" />
					<Version Value=""14"" />
				</Browser>
				<Browser Name=""BrowserFour"" />
			</BlockedBrowsers>
		</Browsers>
		<ParentGate Name=""MyProduct.TestGate""/>
	</Gate>
	<Gate Name=""MyProduct.TestGate"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
	</Gate>
</Gates>";


		/// <summary>
		/// Browser types and versions of a gate inherited from a parent gate for which gate is to be blocked.
		/// </summary>
		private const string ValidNestedConstraintsOnBlockedBrowsers =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.TestGate.BlockedBrowsers"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
			<Environment Name=""Integration""/>
		</Environments>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Test""/>
		</UserGroups>
		<Browsers>
			<BlockedBrowsers>
				<Browser Name=""BrowserOne"">
					<Version Value=""40"" />
					<Version Value=""41"" />
				</Browser>
				<Browser Name=""BrowserTwo"" />
				<Browser Name=""BrowserThree"">
					<Version Value=""7"" />
					<Version Value=""8"" />
					<Version Value=""9"" />
					<Version Value=""10"" />
					<Version Value=""11"" />
				</Browser>
				<Browser Name=""BrowserFour"">
					<Version Value=""20"" />
					<Version Value=""22"" />
				</Browser>
			</BlockedBrowsers>
		</Browsers>
		<ParentGate Name=""MyProduct.TestGate""/>
	</Gate>
	<Gate Name=""MyProduct.TestGate"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
		</Environments>
		<Browsers>
			<BlockedBrowsers>
				<Browser Name=""BrowserOne"">
					<Version Value=""40"" />
					<Version Value=""41"" />
					<Version Value=""42"" />
				</Browser>
				<Browser Name=""BrowserTwo"">
					<Version Value=""36"" />
					<Version Value=""37"" />
				</Browser>
				<Browser Name=""BrowserThree"">
					<Version Value=""9"" />
					<Version Value=""10"" />
					<Version Value=""11"" />
				</Browser>
				<Browser Name=""BrowserFour"" />
				<Browser Name=""BrowserFive"">
					<Version Value=""20"" />
					<Version Value=""21"" />
				</Browser>
			</BlockedBrowsers>
		</Browsers>
	</Gate>
</Gates>";


		/// <summary>
		/// NotValid Gate xml having both Allowed and Blocked browsers.
		/// </summary>
		private const string NotValidGateXmlWithBothAllowedAndBlockedBrowsers =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.TestGate.NotValidGateXml"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<Environments>
			<Environment Name=""PreProduction""/>
		</Environments>
		<Browsers>
			<BlockedBrowsers>
				<Browser Name=""BrowserOne"">
					<Version Value=""40"" />
					<Version Value=""41"" />
				</Browser>
				<Browser Name=""BrowserThree"">
					<Version Value=""11"" />
					<Version Value=""13"" />
					<Version Value=""14"" />
				</Browser>
				<Browser Name=""BrowserFour"" />
			</BlockedBrowsers>
			<AllowedBrowsers>
				<Browser Name=""BrowserOne"">
					<Version Value=""40"" />
					<Version Value=""41"" />
				</Browser>
				<Browser Name=""BrowserThree"">
					<Version Value=""11"" />
					<Version Value=""13"" />
					<Version Value=""14"" />
				</Browser>
				<Browser Name=""BrowserFour"" />
			</AllowedBrowsers>
		</Browsers>
		<ParentGate Name=""MyProduct.TestGate""/>
	</Gate>
	<Gate Name=""MyProduct.TestGate"">
		<Markets>
			<Market Name=""en-us""/>
		</Markets>
	</Gate>
</Gates>";


		/// <summary>
		/// Release plan with multiple release gates.
		/// </summary>
		private const string ReleasePlanWithMultipleReleaseGates =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Test""/>
		</UserGroups>
		<HostEnvironments>
			<HostEnvironment Name=""PreProduction""/>
		</HostEnvironments>
		<ReleasePlan>
			<ReleaseGate Name=""MyProduct.WebService.Feature.ReleasePlan.Integration"">
				<Markets>
					<Market Name=""en-us""/>
					<Market Name=""en-ie""/>
				</Markets>
				<UserGroups>
					<UserGroup Type=""CustomGroup"" Name=""Test""/>
				</UserGroups>
				<HostEnvironments>
					<HostEnvironment Name=""Integration""/>
				</HostEnvironments>
				<StartDate Value=""2013-01-18T12:12:12Z"" />
				<EndDate Value=""2015-01-18T00:00:00Z"" />
			</ReleaseGate>
			<ReleaseGate Name=""MyProduct.WebService.Feature.ReleasePlan.PreProduction"">
				<Markets>
					<Market Name=""en-us""/>
					<Market Name=""en-ie""/>
				</Markets>
				<UserGroups>
					<UserGroup Type=""CustomGroup"" Name=""Test""/>
				</UserGroups>
				<HostEnvironments>
					<HostEnvironment Name=""PreProduction""/>
				</HostEnvironments>
				<StartDate Value=""2013-01-18T12:12:12Z"" />
				<EndDate Value=""2014-01-01T00:00:00Z"" />
			</ReleaseGate>
			<ReleaseGate Name=""MyProduct.WebService.Feature.ReleasePlan.Production"">
				<Markets>
					<Market Name=""en-us""/>
					<Market Name=""en-ie""/>
				</Markets>
				<UserGroups>
					<UserGroup Type=""CustomGroup"" Name=""Test""/>
				</UserGroups>
				<HostEnvironments>
					<HostEnvironment Name=""Production""/>
				</HostEnvironments>
				<StartDate Value=""2013-01-18T12:12:12Z"" />
				<EndDate Value=""2015-01-04T00:00:00Z"" />
			</ReleaseGate>
		</ReleasePlan>
	</Gate>
</Gates>";


		/// <summary>
		/// Release plan with one release gate.
		/// </summary>
		private const string ReleasePlanWithOneReleaseGate =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Test""/>
		</UserGroups>
		<HostEnvironments>
			<HostEnvironment Name=""PreProduction""/>
		</HostEnvironments>
		<ReleasePlan>
			<ReleaseGate Name=""MyProduct.WebService.Feature.ReleasePlan.Integration"">
				<Markets>
					<Market Name=""en-us""/>
					<Market Name=""en-ie""/>
				</Markets>
				<UserGroups>
					<UserGroup Type=""CustomGroup"" Name=""Test""/>
				</UserGroups>
				<HostEnvironments>
					<HostEnvironment Name=""Integration""/>
				</HostEnvironments>
				<StartDate Value=""2013-01-18T12:12:12Z"" />
				<EndDate Value=""2016-01-18T00:00:00Z"" />
			</ReleaseGate>
		</ReleasePlan>
	</Gate>
</Gates>";


		/// <summary>
		/// Release plan having release gates with same name.
		/// </summary>
		private const string ReleasePlanWithSameReleaseGates =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Test""/>
		</UserGroups>
		<HostEnvironments>
			<HostEnvironment Name=""PreProduction""/>
		</HostEnvironments>
		<ParentGate Name=""MyProduct""/>
		<ReleasePlan>
			<ReleaseGate Name=""MyProduct.WebService.Feature.ReleasePlan.Integration"">
				<Markets>
					<Market Name=""en-us""/>
					<Market Name=""en-ie""/>
				</Markets>
				<UserGroups>
					<UserGroup Type=""CustomGroup"" Name=""Test""/>
				</UserGroups>
				<HostEnvironments>
					<HostEnvironment Name=""Integration""/>
				</HostEnvironments>
				<StartDate Value=""2013-01-18T12:12:12Z"" />
				<EndDate Value=""2016-01-18T00:00:00Z"" />
			</ReleaseGate>
			<ReleaseGate Name=""MyProduct.WebService.Feature.ReleasePlan.Integration"">
				<Markets>
					<Market Name=""en-us""/>
					<Market Name=""en-ie""/>
				</Markets>
				<UserGroups>
					<UserGroup Type=""CustomGroup"" Name=""Test""/>
				</UserGroups>
				<HostEnvironments>
					<HostEnvironment Name=""PreProduction""/>
				</HostEnvironments>
				<StartDate Value=""2013-01-18T12:12:12Z"" />
				<EndDate Value=""2016-01-18T00:00:00Z"" />
			</ReleaseGate>
			<ReleaseGate Name=""MyProduct.WebService.Feature.ReleasePlan.PreProduction"">
				<Markets>
					<Market Name=""en-us""/>
					<Market Name=""en-ie""/>
				</Markets>
				<UserGroups>
					<UserGroup Type=""CustomGroup"" Name=""Test""/>
				</UserGroups>
				<HostEnvironments>
					<HostEnvironment Name=""PreProduction""/>
				</HostEnvironments>
				<StartDate Value=""2013-01-18T12:12:12Z"" />
				<EndDate Value=""2014-01-01T00:00:00Z"" />
			</ReleaseGate>
			<ReleaseGate Name=""MyProduct.WebService.Feature.ReleasePlan.Production"">
				<Markets>
					<Market Name=""en-us""/>
					<Market Name=""en-ie""/>
				</Markets>
				<UserGroups>
					<UserGroup Type=""CustomGroup"" Name=""Test""/>
				</UserGroups>
				<HostEnvironments>
					<HostEnvironment Name=""Production""/>
				</HostEnvironments>
				<StartDate Value=""2013-01-18T12:12:12Z"" />
				<EndDate Value=""2015-01-04T00:00:00Z"" />
			</ReleaseGate>
		</ReleasePlan>
	</Gate>
	<Gate Name=""MyProduct"">
		<KnownIPRanges>
			<KnownIPRange Name=""TheInternet""/>
		</KnownIPRanges>
		<HostEnvironments>
			<HostEnvironment Name=""Integration""/>
			<HostEnvironment Name=""PreProduction""/>
			<HostEnvironment Name=""Production""/>
		</HostEnvironments>
	</Gate>
</Gates>";


		/// <summary>
		/// Release plan containing release gates which will be consolidated using the container gate.
		/// </summary>
		private const string ReleasePlanWithConsolidationFromContainingGate =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<Markets>
			<Market Name=""en-us""/>
			<Market Name=""en-ie""/>
		</Markets>
		<UserGroups>
			<UserGroup Type=""CustomGroup"" Name=""Test""/>
			<UserGroup Type=""CustomGroup"" Name=""Automation""/>
		</UserGroups>
		<HostEnvironments>
			<HostEnvironment Name=""Integration""/>
			<HostEnvironment Name=""PreProduction""/>
			<HostEnvironment Name=""Production""/>
		</HostEnvironments>
		<ReleasePlan>
			<ReleaseGate Name=""MyProduct.Test.ReleasePlan.Integration"">
				<Markets>
					<Market Name=""en-us""/>
					<Market Name=""en-ie""/>
					<Market Name=""en-gb""/>
				</Markets>
				<UserGroups>
					<UserGroup Type=""CustomGroup"" Name=""Test""/>
				</UserGroups>
				<HostEnvironments>
					<HostEnvironment Name=""Integration""/>
				</HostEnvironments>
			</ReleaseGate>
			<ReleaseGate Name=""MyProduct.Test.ReleasePlan.Other"">
				<Markets>
					<Market Name=""en-us""/>
				</Markets>
				<UserGroups>
					<UserGroup Type=""CustomGroup"" Name=""Automation""/>
				</UserGroups>
			</ReleaseGate>
		</ReleasePlan>
	</Gate>
</Gates>";


		/// <summary>
		/// Gating file with simple services.
		/// </summary>
		private const string ValidSimpleServices =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.WebSite.Test.Services"">
		<KnownIPRanges>
			<KnownIPRange Name=""TheInternet""/>
		</KnownIPRanges>
		<HostEnvironments>
			<HostEnvironment Name=""Integration""/>
			<HostEnvironment Name=""PreProduction""/>
			<HostEnvironment Name=""Production""/>
		</HostEnvironments>
		<Services>
			<Service Type=""ServiceOne"" ActiveFor=""All"" />
			<Service Type=""ServiceThree"" ActiveFor=""CanaryService"" />
			<Service Type=""ServiceTwo"" ActiveFor=""FullService"" />
			<Service Type=""ServiceFour"" />
		</Services>
	</Gate>
</Gates>";


		/// <summary>
		/// Gating file with nested services.
		/// </summary>
		private const string ValidNestedServices =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.WebSite.Test.Services.Base"">
		<Services>
			<Service Type=""ServiceOne"" ActiveFor=""FullService"" />
			<Service Type=""ServiceSix"" ActiveFor=""CanaryService"" />
			<Service Type=""ServiceTwo"" ActiveFor=""All"" />
			<Service Type=""ServiceFour"" ActiveFor=""All"" />
			<Service Type=""ServiceFive"" ActiveFor=""FullService"" />
			<Service Type=""ServiceSeven"" ActiveFor=""FullService"" />
		</Services>
	</Gate>
	<Gate Name=""MyProduct.WebSite.Test.Services"">
		<HostEnvironments>
			<HostEnvironment Name=""Integration""/>
			<HostEnvironment Name=""PreProduction""/>
			<HostEnvironment Name=""Production""/>
		</HostEnvironments>
		<Services>
			<Service Type=""ServiceOne"" ActiveFor=""All"" />
			<Service Type=""ServiceThree"" ActiveFor=""FullService"" />
			<Service Type=""ServiceTwo"" ActiveFor=""CanaryService"" />
			<Service Type=""ServiceFour"" />
			<Service Type=""ServiceFive"" ActiveFor=""None"" />
			<Service Type=""ServiceSeven"" ActiveFor=""CanaryService"" />
		</Services>
		<ParentGate Name=""MyProduct.WebSite.Test.Services.Base"" />
	</Gate>
</Gates>";


		/// <summary>
		/// Gating file with version range
		/// </summary>
		private const string ValidSimpleWithVersionRange =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" VersionRange=""16.1-16.3""/>
		</ClientVersions>
	</Gate>
</Gates>";


		/// <summary>
		/// Gating file with audience group
		/// </summary>
		private const string ValidSimpleWithAudienceGroup =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.2"" AudienceGroup=""ClientLoop""/>
		</ClientVersions>
	</Gate>
</Gates>";


		/// <summary>
		/// Gating file with application override
		/// </summary>
		private const string ValidSimpleWithApplicationOverride =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<ClientVersions>
			<ClientVersion Name=""ClientOne"" MinVersion=""16.1"" MaxVersion=""16.2"" AudienceGroup=""ClientLoop"">
				<ApplicationOverride AppCode=""8"" MinVersion=""16.8"" MaxVersion=""16.9"" AudienceGroup=""AppLoop""/>
			</ClientVersion>
		</ClientVersions>
	</Gate>
</Gates>";


		/// <summary>
		/// Gating file with a sigle cloud context
		/// </summary>
		private const string OneCloudContext =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<CloudContexts>
			<CloudContext Name=""Public"" />
		</CloudContexts>
	</Gate>
</Gates>";


		/// <summary>
		/// Gating file with multiple cloud context
		/// </summary>
		private const string ThreeCloudContext =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Gates>
	<Gate Name=""MyProduct.Test"">
		<CloudContexts>
			<CloudContext Name=""Public"" />
			<CloudContext Name=""Sovereign"" />
			<CloudContext Name=""Local"" />
		</CloudContexts>
	</Gate>
</Gates>";
	}
}
