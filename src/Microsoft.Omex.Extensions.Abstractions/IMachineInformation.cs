// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net;

namespace Microsoft.Omex.Extensions.Logging.Diagnostics
{
	/// <summary>
	/// Machine information interface
	/// </summary>
	public interface IMachineInformation
	{
		/// <summary>
		/// Agent name
		/// </summary>
		string AgentName { get; }

		
		/// <summary>
		/// Build version string
		/// </summary>
		string BuildVersion { get; }


		/// <summary>
		/// The deployment slice
		/// </summary>
		string DeploymentSlice { get; }


		/// <summary>
		/// The environment name
		/// </summary>
		string EnvironmentName { get; }


		/// <summary>
		/// Is Canary environment?
		/// </summary>
		bool IsCanary { get; }


		/// <summary>
		/// Is Dev Fabric environment?
		/// </summary>
		bool IsDevFabric { get; }


		/// <summary>
		/// Is the deployment a private deployment?
		/// </summary>
		bool IsPrivateDeployment { get; }


		/// <summary>
		/// The name of the deployment cluster to which this machine belongs.
		/// </summary>
		string MachineCluster { get; }


		/// <summary>
		/// The ip adress of the deployment cluster to which this machine belongs.
		/// </summary>
		IPAddress MachineClusterIpAddress { get; }


		/// <summary>
		/// The number of machines in the service pool.
		/// </summary>
		int MachineCount { get; }


		/// <summary>
		/// The reporting identifier for the current machine.
		/// </summary>
		string MachineId { get; }


		/// <summary>
		/// The role of the host machine
		/// </summary>
		string MachineRole { get; }


		/// <summary>
		/// The region
		/// </summary>
		string RegionName { get; }


		/// <summary>
		/// The service name
		/// </summary>
		string ServiceName { get; }
	}
}
