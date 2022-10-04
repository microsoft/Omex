// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Fabric.Health;
using System.Fabric.Query;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Fabric client wrapper interface
	/// </summary>
	public interface IFabricClientWrapper
	{
		/// <summary>
		/// Gets service replicas
		/// </summary>
		/// <param name="partitionId">Partition id</param>
		/// <param name="continuationToken">Continuation token</param>
		/// <returns>Service replica list</returns>
		Task<ServiceReplicaList> GetReplicasAsync(Guid partitionId, string continuationToken);

		/// <summary>
		/// Get replica health
		/// </summary>
		/// <param name="partitionId">Partition id</param>
		/// <param name="replicaId">Replica id</param>
		/// <returns>Replica health</returns>
		Task<ReplicaHealth> GetReplicaHealthAsync(Guid partitionId, long replicaId);

		/// <summary>
		/// Get the details for all applications created in the system
		/// </summary>
		/// <returns>Application list</returns>
		Task<ApplicationList> GetApplicationListAsync();

		/// <summary>
		/// Get the details for all applications or for a specific application created in the system
		/// </summary>
		/// <param name="applicationNameFilter">Application name filter</param>
		/// <param name="continuationToken">Continuation token</param>
		/// <returns>Application list</returns>
		Task<ApplicationList> GetApplicationListAsync(Uri applicationNameFilter, string continuationToken);

		/// <summary>
		/// Get the details for all the application types provisioned or being provisioned in the system
		/// </summary>
		/// <returns>Application type list</returns>
		Task<ApplicationTypeList> GetApplicationTypeListAsync();

		/// <summary>
		/// Get the information about all services belonging to the application specified by the application name URI
		/// </summary>
		/// <param name="applicationName">Application name</param>
		/// <returns>Service list</returns>
		Task<ServiceList> GetServiceListAsync(Uri applicationName);

		/// <summary>
		/// Gets the details for all nodes in the cluster
		/// </summary>
		/// <returns>Node list</returns>
		Task<NodeList> GetNodeListAsync();

		/// <summary>
		/// Get the details for all nodes in the cluster or for the specified node
		/// </summary>
		/// <param name="nodeNameFilter">Node name filter</param>
		/// <param name="continuationToken">Continuation token</param>
		/// <returns>Node list</returns>
		Task<NodeList> GetNodeListAsync(string nodeNameFilter, string continuationToken);

		/// <summary>
		/// The health of a Service Fabric cluster
		/// </summary>
		/// <returns>Cluster health</returns>
		Task<ClusterHealth> GetClusterHealthAsync();

		/// <summary>
		/// Gets the health of the specified Service Fabric application
		/// </summary>
		/// <param name="applicationName">Application name</param>
		/// <returns>Application health</returns>
		Task<ApplicationHealth> GetApplicationHealthAsync(Uri applicationName);

		/// <summary>
		/// Get metrics and load information on the node
		/// </summary>
		/// <param name="nodeName">Node name</param>
		/// <returns>Node load information</returns>
		Task<NodeLoadInformation> GetNodeLoadInformationAsync(string nodeName);

		/// <summary>
		/// Gets the health of a Service Fabric node
		/// </summary>
		/// <param name="nodeName">Node name</param>
		/// <returns>Node health</returns>
		Task<NodeHealth> GetNodeHealthAsync(string nodeName);

		/// <summary>
		/// Reports health on a Service Fabric entity
		/// </summary>
		/// <param name="healthReport">Health report</param>
		void ReportHealth(HealthReport healthReport);

		/// <summary>
		/// Get application upgrade process
		/// </summary>
		/// <param name="applicationName">Application name in Uri form</param>
		/// <returns>An UpgradeProcess object</returns>
		Task<ApplicationUpgradeProgress> GetApplicationUpgradeProgressAsync(Uri applicationName);

		/// <summary>
		/// Gets the health of a Service Fabric partition
		/// </summary>
		/// <param name="partitionId">Partition id</param>
		/// <returns>Partition health</returns>
		Task<PartitionHealth> GetPartitionHealthAsync(Guid partitionId);

		/// <summary>
		/// Gets the details for all partitions of a service
		/// </summary>
		/// <param name="serviceName">Service name</param>
		/// <returns>Service partition list</returns>
		Task<ServicePartitionList> GetPartitionListAsync(Uri serviceName);

		/// <summary>
		/// Gets the health of a Service Fabric service
		/// </summary>
		/// <param name="serviceName">Service name</param>
		/// <returns>Service health</returns>
		Task<ServiceHealth> GetServiceHealthAsync(Uri serviceName);

		/// <summary>
		/// Gets the view of replicas from a node
		/// </summary>
		/// <param name="nodeName">Node name</param>
		/// <param name="applicationName">Application name</param>
		/// <returns>Deployed service replica list</returns>
		Task<DeployedServiceReplicaList> GetDeployedReplicaListAsync(string nodeName, Uri applicationName);
	}
}
