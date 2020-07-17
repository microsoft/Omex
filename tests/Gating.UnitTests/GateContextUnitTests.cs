// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Omex.Gating.Data;
using Microsoft.Omex.Gating.Experimentation;
using Microsoft.Omex.Gating.UnitTests.Shared;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Model.Types;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared.Configuration;
using Microsoft.Omex.System.UnitTests.Shared.Diagnostics;
using Xunit;

namespace Microsoft.Omex.Gating.UnitTests
{
	/// <summary>
	/// Unit Tests for gate context
	/// </summary>
	public sealed class GateContextUnitTests : UnitTestBase
	{
#pragma warning disable SM02239 // hard coded e-mail for unit tests
		private static readonly string s_userIdentifierInGroup = "test1@microsoft.com";
		private static readonly string s_userIdentifierNotInGroup = "test1@apple.com";
#pragma warning restore SM02239

		[Fact]
		public void RequestNoEnvironment_Specified()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", null, 8);
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.NoEnvironment")), "Gate with no environment restrictions should be applicable");
			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.ReqEnvironment")), "Gate with environment restrictions should not be applicable");
			Assert.False(context.IsGateApplicable(m_dataset.GetGate("DogfoodApps")), "Gate with environment restrictions should not be applicable");
		}

		[Fact]
		public void RequestWithEnvironment_Specified()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", "PreProduction", 8);
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.NoEnvironment")), "Gate with no environment restrictions should be applicable");
			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.ReqEnvironment")), "Gate with environment restrictions should be applicable");
			Assert.True(context.IsGateApplicable(m_dataset.GetGate("DogfoodApps")), "Gate with environment restrictions and no client version restrictions should be applicable");
		}

		[Fact]
		public void IsGateApplicable_GateWithEnabledSetting_ReturnTrue()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", "PreProduction", 0);
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext(), null,
				new UnitTestGateSettings(new List<string> { "MyProduct.Test.DisabledGate" }));

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.DisabledGate")),
				"Gate explicitly enabled should be applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithDisabledSetting_ReturnFalse()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", "PreProduction", 0);
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext(), null,
				new UnitTestGateSettings(null, new List<string> { "MyProduct.Test.EnabledGate" }));

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.EnabledGate")),
				"Gate explicitly disabled should not be applicable.");
		}

		[Fact]
		public void IsGateApplicable_WhenGateIsEnabled_ReturnTrue()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", "PreProduction", 0);
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.EnabledGate")),
				"Gate is enabled through the enabled flag in the xml.");
		}

		[Fact]
		public void IsGateApplicable_WhenGateIsDisabled_ReturnFalse()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", "PreProduction", 0);
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.DisabledGate")),
				"Gate is disabled through the enabled flag in the xml.");
		}

		[Fact]
		public void IsGateApplicable_EnablingDisabledGateThroughSettingsForApplicableGate_ReturnsTrue()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", "PreProduction", 0);
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext(), null,
				new UnitTestGateSettings(null, null, new List<string> { "MyProduct.Test.ApplicableDisabledGate" }));

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.ApplicableDisabledGate")),
				"Disabled gate enabled through settings and after applicablity check is returned as applicable.");
		}

		[Fact]
		public void IsGateApplicable_EnablingDisabledGateThroughSettingsForInApplicableGate_ReturnsFalse()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", "PreProduction", 0);
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext(), null,
				new UnitTestGateSettings(null, null, new List<string> { "MyProduct.Test.InapplicableDisabledGate" }));

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.InapplicableDisabledGate")),
				"Disabled gate enabled through settings and after applicablity check is returned as inapplicable.");
		}

		[Fact]
		public void IsGateApplicable_EnablingEnabledGateThroughSettingsForApplicableGate_ReturnsTrue()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", "PreProduction", 0);
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext(), null,
				new UnitTestGateSettings(null, null, new List<string> { "MyProduct.Test.EnabledGate" }));

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.EnabledGate")),
				"Enabling already enabled gate through settings, after applicablity check is returned as applicable.");
		}

		[Fact]
		public void IsGateApplicable_EnablingEnabledGateThroughSettingsForInapplicableGate_ReturnsFalse()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-gb", "PreProduction", 0);
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext(), null,
				new UnitTestGateSettings(null, null, new List<string> { "MyProduct.Test.EnabledGate" }));

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.EnabledGate")),
				"Enabling already enabled gate through settings, after applicablity check is returned as inapplicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithDates_ReturnCorrectly()
		{
			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", "PreProduction", 8);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("CorrectDate")), "Gate with correct dates should be applicable");
			Assert.False(context.IsGateApplicable(m_dataset.GetGate("BeforeStartDate")), "Gate Before Start Date should  ot be applicable");
			Assert.False(context.IsGateApplicable(m_dataset.GetGate("AfterEndDate")), "Gate after end date should not be applicable");
		}

		[Fact]
		public void IsGateApplicable_GateWithNoBlockedQueryParameterConstraints_ReturnsApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			request.QueryParameters = new Dictionary<string, HashSet<string>>() { { "name1", new HashSet<string>() { "value1" } }, { "name2", new HashSet<string>() { "value2" } } };

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedQueryParameters.NoBlockedQueryParameters")),
				"Gate having no BlockedQueryParameters constraint should be applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithEmptyBlockedQueryParameterConstraints_ReturnsApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			request.QueryParameters = new Dictionary<string, HashSet<string>>() { { "name1", new HashSet<string>() { "value1" } }, { "name2", new HashSet<string>() { "value2" } } };

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedQueryParameters.EmptyBlockedQueryParameters")),
				"Gate having empty BlockedQueryParameters constraint should be applicable.");
		}

		[Fact]
		public void IsGateApplicable_RequestWithNoParametersAndGateWithBlockedQueryParameters_ReturnsApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			request.QueryParameters = new Dictionary<string, HashSet<string>>();

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedQueryParameters.WithBlockedQueryParameters")),
				"Gate having some BlockedQueryParameters constraint should be applicable when the request has no query parameters.");
		}

		[Fact]
		public void IsGateApplicable_GateWithSomeBlockedQueryParameterConstraints_ReturnsApplicableWhenBlockedParametersAreNotPresent()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			request.QueryParameters = new Dictionary<string, HashSet<string>>() { { "name1", new HashSet<string>() { "value1" } } };

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedQueryParameters.WithBlockedQueryParameters")),
				"Gate having BlockedQueryParameters constraint but not matched in the request should be applicable");
		}

		[Fact]
		public void IsGateApplicable_GateWithSomeBlockedQueryParameterConstraints_ReturnsNotApplicableWhenBlockedParameterPresent()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			request.QueryParameters = new Dictionary<string, HashSet<string>>() { { "name1", new HashSet<string>() { "value1" } }, { "name2", new HashSet<string>() { "value2" } } };

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedQueryParameters.WithBlockedQueryParameters")),
				"Gate having BlockedQueryParameters constraint and matched in the request should not should be applicable");
		}

		[Fact]
		public void IsGateApplicable_GateWithSomeBlockedQueryParameterConstraints_ReturnsNotApplicableWhenBlockedParameterPresentWithDifferentCasing()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			request.QueryParameters = new Dictionary<string, HashSet<string>>() { { "nAMe2", new HashSet<string>() { "vaLue2" } } };

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedQueryParameters.WithBlockedQueryParameters")),
				"Gate having BlockedQueryParameters constraint with different casing to the query parameter in the request should be not applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithSomeBlockedQueryParameterConstraints_ReturnsNotApplicableWhenUseingValueWildCard()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			request.QueryParameters = new Dictionary<string, HashSet<string>>() { { "name3", new HashSet<string>() { "value" } } };

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedQueryParameters.WithBlockedQueryParameters")),
				"Gate having BlockedQueryParameters constraint with a wild card should block all requests with that query parameter name");
		}

		[Fact]
		public void IsGateApplicable_GateWithEmptyValueBlockedQueryParameterConstraint_ReturnsApplicableAsItIsSkipped()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			request.QueryParameters = new Dictionary<string, HashSet<string>>() { { "name", new HashSet<string>() { "value" } } };

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedQueryParameters.WithEmptyValue")),
				"Gate having BlockedQueryParameters constraint with an empty value is ignored");
		}

		[Fact]
		public void IsGateApplicable_GateWithNoBrowserConstraints_ReturnsApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BrowserType.NoBrowser")),
				"Gate having no browser constraint should be applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithBrowserButNoVersion_ReturnsApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BrowserType.NoBrowserVersion")),
				"Gate having no browser version constraint should be applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithNonCompatibleBrowser_ReturnsNotApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BrowserType.NonCompatibleBrowser")),
				"Gate having non compatibe browser should not be applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithNonCompatibleBrowserVersion_ReturnsNotApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BrowserType.NonCompatibleBrowserVersion")),
				"Gate having non compatible browser version should not be applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithCompatibleBrowserAndVersion_ReturnsApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BrowserType.Browsers")),
				"Gate having compatible browser should be applicable.");
		}

		[Fact]
		public void IsGateApplicable_RequestNotMadeFromBrowserWithGateExpectingBrowsers_ReturnsNotApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest("BrowserFive", -1);
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BrowserType.Browsers")),
				"If request is not made from a browser and gate is active only for certain browsers, context should return not applicable");
		}

		[Fact]
		public void IsGateApplicable_GateWithNoBlockedBrowser_ReturnsApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedBrowsers.NoBlockedBrowser")),
				"Gate having no blocked browser constraint should be applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithBlockedBrowserButNoVersion_ReturnsNotApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedBrowsers.NoBlockedBrowserVersion")),
				"Gate having no browser version constraint should be not applicable as the browser is blocked for all versions.");
		}

		[Fact]
		public void IsGateApplicable_GateWithNonCompatibleBlockedBrowser_ReturnsApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedBrowsers.NonCompatibleBrowser")),
				"Gate having non compatibe blocked browser should be not applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithNonCompatibleBlockedBrowserVersion_ReturnsApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedBrowsers.NonCompatibleBrowserVersion")),
				"Gate having non compatible blocked browser version should be applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateWithCompatibleBlockedBrowserAndVersion_ReturnsNotApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest();
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedBrowsers.Browsers")),
				"Gate having compatible blocked browser should not be applicable.");
		}

		[Fact]
		public void IsGateApplicable_RequestNotMadeFromBrowserWithGateBlockingBrowsers_ReturnsApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest("BrowserFive", -1);
			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.BlockedBrowsers.Browsers")),
				"If request is not made from a browser and gate is blocking access for certain browsers, context should return applicable");
		}

		[Fact]
		public void IsGateApplicable_GateRequestWithRequestedGates_ReturnsApplicable()
		{
			HashSet<string> requestedGates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"MyProduct.Test.DogfoodUsers",
				"MyProduct.Test.LiveIdUser",
			};

			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", null, 0, null, requestedGates);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.NoUsers")), "Gate disabled for all users should not be applicable");
			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.OrgIdUser")), "Gate enabled for OrgId users should not be applicable");
			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.DogfoodUsers")), "Requested gate enabled for Dogfood users should be applicable.");
			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.LiveIdUser")), "Requested gate enabled for LiveId users should be applicable.");
		}

		[Fact]
		public void IsGateApplicable_GateRequestWithRequestedAndBlockedGates_ReturnCorrectResults()
		{
			HashSet<string> requestedGates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"MyProduct.Test.DogfoodUsers",
				"MyProduct.Test.LiveIdUser",
			};

			HashSet<string> blockedGates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"MyProduct.Test.OrgIdUser"
			};

			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", null, 0, null, requestedGates, blockedGates);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.OrgIdUser")), "Gate enabled for OrgId users should not be applicable");
			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.DogfoodUsers")), "Blocked gate enabled for Dogfood users should be applicable.");
			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.LiveIdUser")), "Requested gate enabled for LiveId users should be applicable.");
		}

		[Fact]
		public void IsGateApplicable_ForNoServiceDescribedInGateOnRetailerService_ReturnsGateToBeNotApplicable()
		{
			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", null, 0);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.ServicesWithOutAnyService")),
				"Gate should not be applicable when service tag doesn't have any service");
		}

		[Fact]
		public void IsGateApplicable_OnRetailerService_ReturnsGateToBeApplicable()
		{
			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", null, 0);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation("ServiceFour"), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.Services")),
				"Gate should be applicable for retailer service.");
		}

		[Fact]
		public void IsGateApplicable_OnRetailerCanaryService_ReturnsGateToBeApplicable()
		{
			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", null, 0);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation("ServiceOne", true), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.Services")),
				"Gate should be applicable for retailer canary service.");
		}

		[Fact]
		public void IsGateApplicable_OnCoSubRetailerService_ReturnsGateToBeNotApplicable()
		{
			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", null, 0);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation("ServiceTwo"), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.Services")),
				"Gate should not be applicable for cosubretailer service.");
		}

		[Fact]
		public void IsGateApplicable_OnCoSubRetailerCanaryService_ReturnsGateToBeApplicable()
		{
			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", null, 0);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation("ServiceTwo", true), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.Services")),
				"Gate should not be applicable for cosubretailer canary service.");
		}

		[Fact]
		public void IsGateApplicable_OnDataStoreService_ReturnsGateToBeApplicable()
		{
			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", null, 0);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation("ServiceFour"), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.Services")),
				"Gate should be applicable for datastore service.");
		}

		[Fact]
		public void IsGateApplicable_OnSupplyChainServiceWithNoneServiceFlag_ReturnsGateToBeNotApplicable()
		{
			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", null, 0);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation("ServiceFive"), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.Services")),
				"Gate should be not applicable for supplychain service as service flag is set to none.");
		}

		[Fact]
		public void IsGateApplicable_WithUserInSpecificOrgIdList_ReturnsGateApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest
			{
				Users = new[]
				{
					new GatedUser { IsDogfoodUser = false, UserIdentifier = "test2@microsoft.com"}
				}
			};

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.SpecificOrgId")),
				"Gate should be applicable for user in the specific OrgId list.");
		}

		[Fact]
		public void IsGateApplicable_WithUserNotInSpecificOrgIdList_ReturnsGateNotApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest
			{
				Users = new[]
				{
					new GatedUser { IsDogfoodUser = false, UserIdentifier = s_userIdentifierInGroup}
				}
			};

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.SpecificOrgId")),
				"Gate should not be applicable for user not in the specific OrgId list.");
		}

		[Fact]
		public void IsGateApplicable_WithUserInGroupOrgIdList_ReturnsGateApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest
			{
				Users = new[]
				{
					new GatedUser { IsDogfoodUser = false, UserIdentifier = s_userIdentifierInGroup}
				}
			};

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.GroupOrgId")),
				"Gate should be applicable for user in the group OrgId list.");
		}

		[Fact]
		public void IsGateApplicable_WithUserNotInGroupOrgIdList_ReturnsGateNotApplicable()
		{
			UnitTestGatedRequest request = new UnitTestGatedRequest
			{
				Users = new[]
				{
					new GatedUser { IsDogfoodUser = false, UserIdentifier = s_userIdentifierNotInGroup}
				}
			};

			GateContext context = new GateContext(request, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.GroupOrgId")),
				"Gate should not be applicable for user not in the group OrgId list.");
		}

		[Fact]
		public void IsGateApplicable_GateWithClientAudienceGroup_ReturnsApplicable()
		{
			HashSet<string> audienceGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"ClientLoop"
			};

			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.0.0.0", "en-us", null, 0, audienceGroups);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.NoEnvironment")), "Gate should not be applicable");
			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.LoopUser")), "Gate should be applicable for audience group.");
		}

		[Fact]
		public void IsGateApplicable_GateWithAppAudienceGroup_ReturnsApplicable()
		{
			HashSet<string> audienceGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"AppLoop"
			};

			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.0.0.0", "en-us", null, 8, audienceGroups);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.NoEnvironment")), "Gate should not be applicable");
			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.LoopUser")), "Gate should be applicable for audience group.");
		}

		[Fact]
		public void IsGateApplicable_GateWithAppAudienceGroup_ReturnsNotApplicable()
		{
			HashSet<string> audienceGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"AppLoop"
			};

			IGatedRequest gateRequest = SetupGatedRequest("ClientOne", "16.0.0.0", "en-us", null, 0, audienceGroups);
			GateContext context = new GateContext(gateRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.NoEnvironment")), "Gate should not be applicable");
			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.LoopUser")), "Gate should not be applicable for audience group.");
		}

		[Fact]
		public void RequestWithCloudContext_Specified()
		{
			IGatedRequest gatedRequest = SetupGatedRequest("ClientOne", "16.3.0.0", "en-us", "PreProduction", 8, cloudContext: new HashSet<string> { "Public" });
			GateContext context = new GateContext(gatedRequest, new UnitTestMachineInformation(), new DefaultExperimentContext());

			Assert.True(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.CloudContextPublic")), "Gate with cloud context restrictions should be applicable");
			Assert.False(context.IsGateApplicable(m_dataset.GetGate("MyProduct.Test.CloudContextSovereign")), "Gate with different cloud context restrictions should not be applicable");
			Assert.True(context.IsGateApplicable(m_dataset.GetGate("DogfoodApps")), "Gate without a cloud context should be applicable");
		}

		#region Load Gate Xml
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
		/// Expired Date
		/// </summary>
		private static string OldDate { get; } = DateTime.UtcNow.AddDays(-2).ToString("s", CultureInfo.InvariantCulture);

		/// <summary>
		/// Later Date
		/// </summary>
		private static string LaterDate { get; } = DateTime.UtcNow.AddDays(2).ToString("s", CultureInfo.InvariantCulture);

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
				<TestGroup name=""OMEXSpecificOrgId"">
					<Member email=""test1@mydomain.xx"" alias=""myalias""/>
					<Member email=""test2@microsoft.com"" alias=""myalias""/>
				</TestGroup>
				<TestGroup name=""OMEXGroupOrgId"">
					<Member email=""test2@mydomain.xx"" alias=""myalias""/>
					<Member email=""@microsoft.com"" alias=""myalias""/>
				</TestGroup>
			</TestGroups>";

		/// <summary>
		/// Gate xml file
		/// </summary>
		private static string GateXml { get; } =
		@"<?xml version=""1.0"" encoding=""utf-8""?>
		<Gates>
			<Gate Name=""MyProduct.Test.BlockedQueryParameters.NoBlockedQueryParameters"" />
			<Gate Name=""MyProduct.Test.BlockedQueryParameters.EmptyBlockedQueryParameters"" >
				<BlockedQueryParameters>
				</BlockedQueryParameters>
			</Gate>
			<Gate Name=""MyProduct.Test.BlockedQueryParameters.WithBlockedQueryParameters"">
				<BlockedQueryParameters>
					<BlockedQueryParameter Name=""name2"" Value=""value2"" />
					<BlockedQueryParameter Name=""name2"" Value=""value2a"" />
					<BlockedQueryParameter Name=""name3"" Value=""*"" />
					<BlockedQueryParameter Name=""name3"" Value=""extra_value"" />
				</BlockedQueryParameters>
			</Gate>
			<Gate Name=""MyProduct.Test.BlockedQueryParameters.WithEmptyValue"">
				<BlockedQueryParameters>
					<BlockedQueryParameter Name=""name"" Value="""" />
				</BlockedQueryParameters>
			</Gate>
			<Gate Name=""MyProduct.Test.ReqEnvironment"">
				<Markets>
					<Market Name=""en-us""/>
				</Markets>
				<Environments>
					<Environment Name=""PreProduction""/>
					<Environment Name=""Integration""/>
				</Environments>
				<ClientVersions>
					<ClientVersion Name=""ClientOne"" MinVersion=""16.1.0.0"" MaxVersion=""16.3.1.2""/>
				</ClientVersions>
			</Gate>
			<Gate Name=""MyProduct.Test.NoEnvironment"">
				<Markets>
					<Market Name=""en-us""/>
				</Markets>
				<ClientVersions>
					<ClientVersion Name=""ClientOne"" MinVersion=""16.1.0.0"" MaxVersion=""16.3.1.2"">
						<ApplicationOverride AppCode=""8"" MinVersion=""16.2.0.0"" MaxVersion=""16.3.3.3""/>
					</ClientVersion>
				</ClientVersions>
			</Gate>
			<Gate Name=""MyProduct.Test.LoopUser"">
				<ClientVersions>
					<ClientVersion Name=""ClientOne"" AudienceGroup=""ClientLoop"">
						<ApplicationOverride AppCode=""8"" AudienceGroup=""AppLoop""/>
					</ClientVersion>
				</ClientVersions>
			</Gate>
			<Gate Name=""DogfoodApps"">
				<Environments>
					<Environment Name=""PreProduction""/>
				</Environments>
				<ClientVersions>
					<ClientVersion Name=""ClientOne""/>
				</ClientVersions>
			</Gate>
			<Gate Name=""MyProduct.Test.NoUsers"">
				<UserGroups>
					<UserGroup Type=""None"" />
				</UserGroups>
			</Gate>
			<Gate Name=""MyProduct.Test.DogfoodUsers"">
				<UserGroups>
					<UserGroup Type=""Dogfood"" />
				</UserGroups>
			</Gate>
			<Gate Name=""MyProduct.Test.LiveIdUser"">
				<UserGroups>
					<UserGroup Name=""Automation"" Type=""CustomGroup"" />
				</UserGroups>
			</Gate>
			<Gate Name=""MyProduct.Test.OrgIdUser"">
				<UserGroups>
					<UserGroup Name=""OrgIdUserGroup"" Type=""CustomGroup"" />
				</UserGroups>
			</Gate>
			<Gate Name=""MyProduct.Test.SpecificOrgId"">
				<UserGroups>
					<UserGroup Name=""OMEXSpecificOrgId"" Type=""CustomGroup"" />
				</UserGroups>
			</Gate>
			<Gate Name=""MyProduct.Test.GroupOrgId"">
				<UserGroups>
					<UserGroup Name=""OMEXGroupOrgId"" Type=""CustomGroup"" />
				</UserGroups>
			</Gate>
			<Gate Name=""CorrectDate"">
				<StartDate Value = """ + OldDate + @""" />
				<EndDate Value = """ + LaterDate + @""" />
			</Gate>
			<Gate Name=""BeforeStartDate"">
				<StartDate Value = """ + LaterDate + @""" />
				<EndDate Value = """ + LaterDate + @""" />
			</Gate>
			<Gate Name=""AfterEndDate"">
				<StartDate Value = """ + OldDate + @""" />
				<EndDate Value = """ + OldDate + @""" />
			</Gate>
			<Gate Name=""MyProduct.Test.BlockedBrowsers.NoBlockedBrowser"">
			</Gate>
			<Gate Name=""MyProduct.Test.BlockedBrowsers.NoBlockedBrowserVersion"">
				<Browsers>
					<BlockedBrowsers>
						<Browser Name=""BrowserThree"" />
					</BlockedBrowsers>
				</Browsers>
			</Gate>
			<Gate Name=""MyProduct.Test.BlockedBrowsers.Browsers"">
				<Browsers>
					<BlockedBrowsers>
						<Browser Name=""BrowserThree"">
							<Version Value=""10"" />
							<Version Value=""11"" />
						</Browser>
						<Browser Name=""BrowserOne"">
							<Version Value=""40"" />
						</Browser>
					</BlockedBrowsers>
				</Browsers>
			</Gate>
			<Gate Name=""MyProduct.Test.BlockedBrowsers.NonCompatibleBrowser"">
				<Browsers>
					<BlockedBrowsers>
						<Browser Name=""BrowserOne"">
							<Version Value=""40"" />
						</Browser>
						<Browser Name=""BrowserFour"" />
					</BlockedBrowsers>
				</Browsers>
			</Gate>
			<Gate Name=""MyProduct.Test.BlockedBrowsers.NonCompatibleBrowserVersion"">
				<Browsers>
					<BlockedBrowsers>
						<Browser Name=""BrowserThree"">
							<Version Value=""11"" />
						</Browser>
					</BlockedBrowsers>
				</Browsers>
			</Gate>
			<Gate Name=""MyProduct.Test.BrowserType.NoBrowser"">
			</Gate>
			<Gate Name=""MyProduct.Test.BrowserType.NoBrowserVersion"">
				<Browsers>
					<AllowedBrowsers>
						<Browser Name=""BrowserThree"" />
					</AllowedBrowsers>
				</Browsers>
			</Gate>
			<Gate Name=""MyProduct.Test.BrowserType.Browsers"">
				<Browsers>
					<AllowedBrowsers>
						<Browser Name=""BrowserThree"">
							<Version Value=""10"" />
							<Version Value=""11"" />
						</Browser>
						<Browser Name=""BrowserOne"">
							<Version Value=""40"" />
						</Browser>
					</AllowedBrowsers>
				</Browsers>
			</Gate>
			<Gate Name=""MyProduct.Test.BrowserType.NonCompatibleBrowser"">
				<Browsers>
					<AllowedBrowsers>
						<Browser Name=""BrowserOne"">
							<Version Value=""40"" />
						</Browser>
						<Browser Name=""BrowserFour"" />
					</AllowedBrowsers>
				</Browsers>
			</Gate>
			<Gate Name=""MyProduct.Test.BrowserType.NonCompatibleBrowserVersion"">
				<Browsers>
					<AllowedBrowsers>
						<Browser Name=""BrowserThree"">
							<Version Value=""11"" />
						</Browser>
					</AllowedBrowsers>
				</Browsers>
			</Gate>
			<Gate Name=""MyProduct.Test.EnabledGate"" Enabled=""true"">
				<Markets>
					<Market Name=""en-us"" />
				</Markets>
			</Gate>
			<Gate Name=""MyProduct.Test.DisabledGate"" Enabled=""false"">
				<Markets>
					<Market Name=""en-us"" />
				</Markets>
			</Gate>
			<Gate Name=""MyProduct.Test.ApplicableDisabledGate"" Enabled=""false"">
				<Markets>
					<Market Name=""en-us"" />
				</Markets>
				<Environments>
					<Environment Name=""PreProduction"" />
				</Environments>
			</Gate>
			<Gate Name=""MyProduct.Test.InapplicableDisabledGate"" Enabled=""false"">
				<Markets>
					<Market Name=""en-gb"" />
				</Markets>
				<Environments>
					<Environment Name=""PreProduction"" />
				</Environments>
			</Gate>
			<Gate Name=""MyProduct.Test.Services"">
				<Markets>
					<Market Name=""en-us"" />
				</Markets>
				<Services>
					<Service Type=""ServiceOne"" ActiveFor=""All"" />
					<Service Type=""ServiceTwo"" ActiveFor=""CanaryService"" />
					<Service Type=""ServiceFour"" ActiveFor=""FullService"" />
					<Service Type=""ServiceFive"" ActiveFor=""None"" />
				</Services>
			</Gate>
			<Gate Name=""MyProduct.Test.ServicesWithOutAnyService"">
				<Markets>
					<Market Name=""en-us"" />
				</Markets>
				<Services>
				</Services>
			</Gate>
			<Gate Name=""MyProduct.Test.CloudContextPublic"">
				<CloudContexts>
					<CloudContext Name=""Public"" />
				</CloudContexts>
			</Gate>
			<Gate Name=""MyProduct.Test.CloudContextSovereign"">
				<CloudContexts>
					<CloudContext Name=""Sovereign"" />
				</CloudContexts>
			</Gate>
		</Gates>";

		/// <summary>
		/// Gate dataset
		/// </summary>
		private GateDataSet m_dataset = LoadGateDataSet(GateXml);
		#endregion

		/// <summary>
		/// Setups the gated request.
		/// </summary>
		/// <param name="product">The product.</param>
		/// <param name="productVersion">The product version.</param>
		/// <param name="market">The market.</param>
		/// <param name="environment">The environment.</param>
		/// <param name="app">AppCode.</param>
		/// <param name="audienceGroup">Audience Group.</param>
		/// <param name="requestedGates">The requested gates.</param>
		/// <param name="blockedGates">The blocked gates.</param>
		/// <returns>Gated Request</returns>
		private static IGatedRequest SetupGatedRequest(string product, string productVersion, string market, string environment,
			int app, HashSet<string> audienceGroups = null, HashSet<string> requestedGates = null, HashSet<string> blockedGates = null, HashSet<string> cloudContext = null)
		{
			IGatedRequest gatedRequest = new UnitTestGatedRequest()
			{
				CallingClient = new GatedClient
				{
					Name = product,
					ProductCode = new ProductCode(product),
					Version = ProductVersion.Parse(productVersion),
					AppCode = app.ToString(),
					AudienceGroups = audienceGroups
				},
				Environment = environment,
				Market = market,
				Users = null,
				RequestedGateIds = requestedGates,
				BlockedGateIds = blockedGates,
				CloudContexts = cloudContext
			};

			return gatedRequest;
		}
	}
}
