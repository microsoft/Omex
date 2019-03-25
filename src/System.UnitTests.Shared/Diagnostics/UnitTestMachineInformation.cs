// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net;
using Microsoft.Omex.System.Diagnostics;

namespace Microsoft.Omex.System.UnitTests.Shared.Diagnostics
{
	/// <summary>
	/// Unit test machine information
	/// </summary>
	public class UnitTestMachineInformation : IMachineInformation
	{
		/// <summary>
		/// Public Constructor
		/// using default values in order to register it on Instance Container
		/// </summary>
		public UnitTestMachineInformation()
			: this("None")
		{
		}


		/// <summary>
		/// Public Constructor.
		/// </summary>
		/// <param name="serviceName">The service.</param>
		/// <param name="isCanary">Is service canary.</param>
		/// <param name="agentName">Agent name.</param>
		/// <param name="machineId">Machine Id.</param>
		/// <param name="regionName">Region</param>
		/// <param name="isDevFabric">IsDevFabric</param>
		/// <param name="environmentName">Environment</param>
		/// <param name="isPrivateDeployment">is private deployment</param>
		/// <param name="machineCount">machine count</param>
		public UnitTestMachineInformation(string serviceName = "None", bool isCanary = false, string agentName = "None",
			string machineId = "MachineId", string regionName = "None", bool isDevFabric = true,
			string environmentName = "None", bool isPrivateDeployment = true, int machineCount = 1)
		{
			EnvironmentName = environmentName;
			MachineId = machineId;
			IsDevFabric = isDevFabric;
			IsCanary = isCanary;
			ServiceName = serviceName;
			AgentName = agentName;
			RegionName = regionName;
			IsPrivateDeployment = isPrivateDeployment;
			MachineCount = machineCount;
		}


		/// <summary>
		/// Get the current agent instance
		/// </summary>
		public string AgentName { get; }


		/// <summary>
		/// Build version string
		/// </summary>
		public string BuildVersion => "1.0.0.0";


		/// <summary>
		/// The deployment slice
		/// </summary>
		public string DeploymentSlice { get; }


		/// <summary>
		/// The environment name
		/// </summary>
		public string EnvironmentName { get; }


		/// <summary>
		/// Is Canary environment?
		/// </summary>
		public bool IsCanary { get; }


		/// <summary>
		/// Is Dev Fabric environment?
		/// </summary>
		public bool IsDevFabric { get; }


		/// <summary>
		/// Is private deployment?
		/// </summary>
		public bool IsPrivateDeployment { get; }


		/// <summary>
		/// The number of machines in the service pool.
		/// </summary>
		public int MachineCount { get; }


		/// <summary>
		/// The name of the deployment cluster to which this machine belongs.
		/// </summary>
		public string MachineCluster => "MachineCluster";


		/// <summary>
		/// The ip adress of the deployment cluster to which this machine belongs.
		/// </summary>
		public IPAddress MachineClusterIpAddress => IPAddress.None;


		/// <summary>
		/// The reporting identifier for the current machine.
		/// </summary>
		public string MachineId { get; }


		/// <summary>
		/// The role of the host machine
		/// </summary>
		public string MachineRole => "MachineRole";


		/// <summary>
		/// The region
		/// </summary>
		public string RegionName { get; }


		/// <summary>
		/// The service name
		/// </summary>
		public string ServiceName { get; }
	}
}