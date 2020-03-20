// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Omex.Gating.Data;
using Microsoft.Omex.Gating.UnitTests.Shared;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared.Configuration;
using Xunit;

namespace Microsoft.Omex.Gating.UnitTests
{
	/// <summary>
	/// Unit tests for GatedCode.
	/// </summary>
	public sealed class GatedCodeUnitTests : UnitTestBase
	{
		[Fact]
		public void GatedCode_UsingDefaultConstructor_BaseLineCodeFlagIsTrue()
		{
			GatedCode gatedCode = new GatedCode();

			Assert.True(gatedCode.IsBaselineCode, "Default Constructor should set BaseLineCodeFlag as true");
		}

		[Fact]
		public void GatedCode_IntitialzedWithNullObject_NonApplicableGatesShouldBeReturnedApplicableUsingGatesProperty()
		{
			// Gates property intialized with GatesNone object.
			GatedCode gatedCode = new GatedCode((IGate)null);

			UnitTestGateContext gateContext = new UnitTestGateContext
			{
				AlwaysReturnApplicable = false,
				ApplicableGates = new List<string>()
				{
					"Test.Gate1",
					"Test.Gate2"
				}
			};

			Assert.True(gatedCode.Gates.IsApplicable(gateContext, out IGate[] gates), "Returns true as no gate was applicable.");
			Assert.Null(gates);
		}

		[Fact]
		public void GatedCode_UsingGateWithoutReleasePlan_ApplicableGateReturnedApplicableUsingGatesProperty()
		{
			// Gates property intialized with GatesAny object.
			GatedCode gatedCode = new GatedCode(new Gate("Test.Gate1"));

			UnitTestGateContext gateContext = new UnitTestGateContext
			{
				AlwaysReturnApplicable = false,
				ApplicableGates = new List<string>()
				{
					"Test.Gate1",
					"Test.Gate2"
				}
			};

			Assert.False(gatedCode.IsBaselineCode, "Shouldn't be a baseline code as its gated.");
			Assert.True(gatedCode.Gates.IsApplicable(gateContext, out IGate[] gates), "Returns true with first applicable gate");
			Assert.Equal("Test.Gate1", gates.First().FullyQualifiedName);
		}

		[Fact]
		public void GatedCode_UsingGateWithoutReleasePlan_NonApplicableGateReturnedNonApplicableUsingGatesProperty()
		{
			// Gates property intialized with GatesAny object.
			GatedCode gatedCode = new GatedCode(new Gate("Test.Gate1"));

			UnitTestGateContext gateContext = new UnitTestGateContext
			{
				AlwaysReturnApplicable = false,
				ApplicableGates = new List<string>()
				{
					"Test.Gate2",
					"Test.Gate3"
				}
			};

			Assert.False(gatedCode.IsBaselineCode, "Shouldn't be a baseline code as its gated.");
			Assert.False(gatedCode.Gates.IsApplicable(gateContext, out IGate[] gates), "Returns false as no gate was applicable.");
			Assert.Null(gates);
		}

		[Fact]
		public void GatedCode_UsingGateWithReleasePlanHavingNoReleaseGate_NonApplicableGateReturnedApplicableUsingGatesProperty()
		{
			GateDataSet gateDataSet = LoadGateDataSet(GateXmlWithOutReleaseGate);

			// Gates property intialized with GatesNone object.
			GatedCode gatedCode = new GatedCode(gateDataSet.GetGate("MyProduct.Test.ReleasePlanWithoutReleaseGate"));

			UnitTestGateContext gateContext = new UnitTestGateContext
			{
				AlwaysReturnApplicable = false,
				ApplicableGates = new List<string>()
				{
					"Test.Gate2",
					"Test.Gate3"
				}
			};

			Assert.False(gatedCode.IsBaselineCode, "Shouldn't be a baseline code as its gated.");
			Assert.True(gatedCode.Gates.IsApplicable(gateContext, out IGate[] gates), "Returns true as no gate was applicable.");
			Assert.Null(gates);
		}

		[Fact]
		public void GatedCode_UsingGateWithReleasePlanHavingApplicableReleaseGates_FirstApplicableReleaseGateReturnedUsingGatesProperty()
		{
			GateDataSet gateDataSet = LoadGateDataSet(GateXmlWithReleaseGates);

			// Gates property intialized with GatesAny object.
			GatedCode gatedCode = new GatedCode(gateDataSet.GetGate("MyProduct.Test.ReleasePlan"));

			UnitTestGateContext gateContext = new UnitTestGateContext
			{
				AlwaysReturnApplicable = false,
				ApplicableGates = new List<string>()
				{
					"MyProduct.Test.ReleasePlan",
					"MyProduct.WebSite.Feature.ReleasePlan.Integration"
				}
			};

			Assert.False(gatedCode.IsBaselineCode, "Shouldn't be a baseline code as its gated.");
			Assert.True(gatedCode.Gates.IsApplicable(gateContext, out IGate[] gates), "Returns true with first applicable release gate.");
			Assert.Equal("MyProduct.WebSite.Feature.ReleasePlan.Integration", gates.First().FullyQualifiedName);
		}

		[Fact]
		public void GatedCode_UsingGateWithReleasePlanHavingNoApplicableReleaseGates_NoApplicableReleaseGateReturnedUsingGatesProperty()
		{
			GateDataSet gateDataSet = LoadGateDataSet(GateXmlWithReleaseGates);

			// Gates property intialized with GatesNone object.
			GatedCode gatedCode = new GatedCode(gateDataSet.GetGate("MyProduct.Test.ReleasePlan"));

			UnitTestGateContext gateContext = new UnitTestGateContext
			{
				AlwaysReturnApplicable = false,
				ApplicableGates = new List<string>()
				{
					"MyProduct.Test.ReleasePlan",
				}
			};

			Assert.False(gatedCode.IsBaselineCode, "Shouldn't be a baseline code as its gated.");
			Assert.False(gatedCode.Gates.IsApplicable(gateContext, out IGate[] gates), "Returns false as no release gate applicable");
			Assert.Null(gates);
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
				<TestGroup name=""Probes"">
					<Member deploymentId=""{3D5ABF05-6148-4594-9BBE-A3CB2A34EBF8}"" alias=""myalias""/>
					<Member deploymentId=""{8499ED16-258C-49d4-A1E0-C9E1B1460FDC}"" alias=""myalias""/>
				</TestGroup>
			</TestGroups>";

		/// <summary>
		/// The gate XML to be used by the unit test.
		/// </summary>
		private const string GateXmlWithReleaseGates =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
			<Gates>
				<Gate Name=""MyProduct.Test.ReleasePlan"">
					<ReleasePlan>
						<ReleaseGate Name=""MyProduct.WebSite.Feature.ReleasePlan.Integration"">
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
						<ReleaseGate Name=""MyProduct.WebSite.Feature.ReleasePlan.PreProduction"">
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
		/// The gate XML to be used by the unit test.
		/// </summary>
		private const string GateXmlWithOutReleaseGate =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
			<Gates>
				<Gate Name=""MyProduct.Test.ReleasePlanWithoutReleaseGate"">
					<ReleasePlan>
					</ReleasePlan>
				</Gate>
			</Gates>";
	}
}
