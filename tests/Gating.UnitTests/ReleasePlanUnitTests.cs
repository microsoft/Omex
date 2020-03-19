// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
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
	/// Unit tests for ReleasePlan.
	/// </summary>
	public sealed class ReleasePlanUnitTests : UnitTestBase
	{
		[Fact]
		public void IsApplicable_ForNotApplicableReleaseGates_NoReleaseGateReturned()
		{
			IGatedRequest gateRequest = SetupGateRequest("ClientThree", "16.3.0.0", "en-us");
			GateContext context = new GateContext(gateRequest, new BasicMachineInformation(), new DefaultExperimentContext());
			IGate gate = Dataset.GetGate("MyProduct.Test.ReleasePlan");
			GatesAny gates = new GatesAny(gate.ReleasePlan);

			Assert.False(gates.IsApplicable(context, out IGate[] applicableReleaseGates), "Gate should be not applicable");
			Assert.Null(applicableReleaseGates);
		}

		[Fact]
		public void IsApplicable_ForApplicableReleaseGate_ApplicableReleaseGateReturned()
		{
			IGatedRequest gateRequest = SetupGateRequest("ClientThree", "16.3.0.0", "en-us");
			GateContext context = new GateContext(gateRequest, new BasicMachineInformation(), new DefaultExperimentContext(), null,
				new UnitTestGateSettings(new List<string> { "MyProduct.WebSite.Feature.ReleasePlan.Integration" }));

			IGate gate = Dataset.GetGate("MyProduct.Test.ReleasePlan");
			GatesAny gates = new GatesAny(gate.ReleasePlan);

			Assert.True(gates.IsApplicable(context, out IGate[] applicableReleaseGates), "Gate should be applicable");

			Assert.NotNull(applicableReleaseGates);
			Assert.Same(applicableReleaseGates[0], gate.ReleasePlan[0]);
		}

		[Fact]
		public void IsApplicable_ContainterGateIsApplicable_ApplicableReleaseGateReturned()
		{
			IGatedRequest gateRequest = SetupGateRequest("ClientThree", "16.3.0.0", "en-us", "PreProduction", new[] { new GatedUser { UserIdentifier = "test1@mydomain.xx" } });
			GateContext context = new GateContext(gateRequest, new BasicMachineInformation(), new DefaultExperimentContext());

			GateDataSet dataSet = LoadGateDataSet(ReleaseGateXml);
			IGate gate = dataSet.GetGate("MyProduct.Test.ReleasePlan");
			GatesAny gates = new GatesAny(gate.ReleasePlan);

			Assert.True(gates.IsApplicable(context, out IGate[] applicableReleaseGates), "Gate should be applicable");

			Assert.NotNull(applicableReleaseGates);
			Assert.Same(applicableReleaseGates[0], gate.ReleasePlan[0]);
		}

		[Fact]
		public void IsApplicable_ContainterGateIsNotApplicable_NoReleaseGateReturned()
		{
			IGatedRequest gateRequest = SetupGateRequest("ClientThree", "16.3.0.0", "en-gb");
			GateContext context = new GateContext(gateRequest, new BasicMachineInformation(), new DefaultExperimentContext());

			GateDataSet dataSet = LoadGateDataSet(ReleaseGateXml);
			IGate gate = dataSet.GetGate("MyProduct.Test.ReleasePlan");
			GatesAny gates = new GatesAny(gate.ReleasePlan);

			Assert.False(gates.IsApplicable(context, out IGate[] applicableReleaseGates), "Gate should not be applicable");
			Assert.Null(applicableReleaseGates);
		}

		/// <summary>
		/// Loads the gate data set.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <returns>Gate data set.</returns>
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
				<TestGroup name=""OrgIdUserGroup"">
					<Member email=""@microsoft.com"" alias=""myalias""/>
				</TestGroup>
				<TestGroup name=""OMEXProbes"">
					<Member deploymentId=""{3D5ABF05-6148-4594-9BBE-A3CB2A34EBF8}"" alias=""myalias""/>
					<Member deploymentId=""{8499ED16-258C-49d4-A1E0-C9E1B1460FDC}"" alias=""myalias""/>
				</TestGroup>
			</TestGroups>";

		/// <summary>
		/// The gate XML to be used by the unit test.
		/// </summary>
		private const string GateXml =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
			<Gates>
				<Gate Name=""MyProduct.Test.ReleasePlan"">
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
						<ReleaseGate Name=""MyProduct.WebSite.Feature.ReleasePlan.Integration"">
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
							<StartDate Value=""2013-01-18T12:12:12Z"" />
							<EndDate Value=""2015-01-18T00:00:00Z"" />
						</ReleaseGate>
						<ReleaseGate Name=""MyProduct.WebSite.Feature.ReleasePlan.PreProduction"">
							<Markets>
								<Market Name=""en-us""/>
								<Market Name=""en-ie""/>
								<Market Name=""en-gb""/>
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
						<ReleaseGate Name=""MyProduct.WebSite.Feature.ReleasePlan.Production"">
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
		/// The gate XML to be used for testing applicability of release gates subject to applicability of container gate.
		/// </summary>
		private const string ReleaseGateXml =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
			<Gates>
				<Gate Name=""MyProduct.Test.ReleasePlan"">
					<Markets>
						<Market Name=""en-us""/>
						<Market Name=""en-ie""/>
					</Markets>
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
						</ReleaseGate>
						<ReleaseGate Name=""MyProduct.WebSite.Feature.ReleasePlan.PreProduction"">
							<Markets>
								<Market Name=""en-us""/>
								<Market Name=""en-ie""/>
								<Market Name=""en-gb""/>
							</Markets>
							<UserGroups>
								<UserGroup Type=""CustomGroup"" Name=""Automation""/>
							</UserGroups>
						</ReleaseGate>
					</ReleasePlan>
				</Gate>
			</Gates>";

		/// <summary>
		/// The m_dataset
		/// </summary>
		private static GateDataSet Dataset { get; }  = LoadGateDataSet(GateXml);

		/// <summary>
		/// Setups the gate request.
		/// </summary>
		/// <param name="product">The product.</param>
		/// <param name="version">The product version.</param>
		/// <param name="market">The market.</param>
		/// <param name="environment">The environment.</param>
		/// <param name="gateUsers">The gate users.</param>
		/// <returns>Gate Request</returns>
		private static IGatedRequest SetupGateRequest(string product, string version, string market,
			string environment = "PreProduction", IEnumerable<GatedUser> gateUsers = null)
		{
			IGatedRequest gateRequest = new UnitTestGatedRequest()
			{
				CallingClient = new GatedClient
				{
					Name = product,
					ProductCode = new ProductCode(product),
					Version = ProductVersion.Parse(version)
				},
				Environment = environment,
				Market = market,
				RequestedGateIds = null,
				Users = gateUsers
			};

			return gateRequest;
		}
	}
}
