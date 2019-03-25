// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net;
using Microsoft.Omex.System.Diagnostics;
using Xunit;

namespace Microsoft.Omex.System.UnitTests.Diagnostics
{
	/// <summary>
	/// Unit tests for BasicMachineInformation class
	/// </summary>
	public sealed class BasicMachineInformationUnitTests
	{
		[Fact]
		public void AgentName_ShouldBeEmpty()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.Equal<string>(EmptyValue, machineInformation.AgentName);
		}


		[Fact]
		public void BuildVersion_ShouldNotBeEmpty()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.False(string.IsNullOrWhiteSpace(machineInformation.BuildVersion));
		}


		[Fact]
		public void DeploymentSlice_ShouldBeEmpty()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.Equal<string>(EmptyValue, machineInformation.DeploymentSlice);
		}


		[Fact]
		public void EnvironmentName_ShouldBeEmpty()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.Equal<string>(EmptyValue, machineInformation.EnvironmentName);
		}


		[Fact]
		public void IsCanary_ShouldBeFalse()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.Equal<bool>(false, machineInformation.IsCanary);
		}


		[Fact]
		public void IsDevFabric_ShouldBeFalse()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.Equal<bool>(false, machineInformation.IsDevFabric);
		}


		[Fact]
		public void IsPrivateDeployment_ShouldBeTrue()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.Equal<bool>(true, machineInformation.IsPrivateDeployment);
		}


		[Fact]
		public void MachineCount_ShouldBeOne()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.Equal<int>(1, machineInformation.MachineCount);
		}


		[Fact]
		public void MachineCluster_ShouldNotBeEmpty()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.False(string.IsNullOrWhiteSpace(machineInformation.MachineCluster));
		}


		[Fact]
		public void MachineClusterIpAddress_ShouldNotBeNone()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.NotEqual<IPAddress>(IPAddress.None, machineInformation.MachineClusterIpAddress);
		}


		[Fact]
		public void MachineId_ShouldNotBeEmpty()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.False(string.IsNullOrWhiteSpace(machineInformation.MachineId));
		}


		[Fact]
		public void MachineRole_ShouldBeEmpty()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.Equal<string>(EmptyValue, machineInformation.MachineRole);
		}


		[Fact]
		public void RegionName_ShouldBeEmpty()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.Equal<string>(EmptyValue, machineInformation.RegionName);
		}


		[Fact]
		public void ServiceName_ShouldBeEmpty()
		{
			IMachineInformation machineInformation = new BasicMachineInformation();

			Assert.Equal<string>(EmptyValue, machineInformation.ServiceName);
		}


		private const string EmptyValue = "None";
	}
}