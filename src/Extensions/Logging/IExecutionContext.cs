// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Interface with information about the environment that executes code
	/// </summary>
	public interface IExecutionContext
	{
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
		/// Is Canary environment
		/// </summary>
		bool IsCanary { get; }

		/// <summary>
		/// Is the deployment a private deployment
		/// </summary>
		bool IsPrivateDeployment { get; }

		/// <summary>
		/// The name of the deployment cluster to which this machine belongs
		/// </summary>
		string Cluster { get; }

		/// <summary>
		/// The ip address of the deployment cluster to which this machine belongs
		/// </summary>
		IPAddress ClusterIpAddress { get; }

		/// <summary>
		/// The reporting identifier for the current machine
		/// </summary>
		string MachineId { get; }

		/// <summary>
		/// Name of the application
		/// </summary>
		/// <remarks>
		/// Could be the same as service name, but would be different if you running ex. Service Fabric application with multiple services
		/// </remarks>
		string ApplicationName { get; }

		/// <summary>
		/// Azure region name
		/// </summary>
		string RegionName { get; }

		/// <summary>
		/// The service name
		/// </summary>
		string ServiceName { get; }
	}
}
