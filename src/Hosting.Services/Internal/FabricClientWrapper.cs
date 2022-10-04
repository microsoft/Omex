// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Fabric.Health;
using System.Fabric.Query;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services.Internal
{
	/// <summary>
	/// Fabric client wrapper
	/// </summary>
	public class FabricClientWrapper : IFabricClientWrapper
	{
		/// <summary>
		/// Fabric client
		/// </summary>
		private readonly FabricClient m_fabricClient;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fabricClient">Fabric client</param>
		public FabricClientWrapper(FabricClient fabricClient) =>
			m_fabricClient = fabricClient ?? throw new ArgumentNullException(nameof(fabricClient));

		/// <summary>
		/// Gets service replicas
		/// </summary>
		/// <param name="partitionId">Partition id</param>
		/// <param name="continuationToken">Continuation token</param>
		/// <returns>Service replica list</returns>
		public Task<ServiceReplicaList> GetReplicasAsync(Guid partitionId, string continuationToken) =>
			m_fabricClient.QueryManager.GetReplicaListAsync(partitionId, continuationToken);

		/// <summary>
		/// Get replica health
		/// </summary>
		/// <param name="partitionId">Partition id</param>
		/// <param name="replicaId">Replica id</param>
		/// <returns>Replica health</returns>
		public Task<ReplicaHealth> GetReplicaHealthAsync(Guid partitionId, long replicaId) =>
			m_fabricClient.HealthManager.GetReplicaHealthAsync(partitionId, replicaId);

		/// <summary>
		/// Get the details for all applications created in the system
		/// </summary>
		/// <returns>Application list</returns>
		public Task<ApplicationList> GetApplicationListAsync() =>
			m_fabricClient.QueryManager.GetApplicationListAsync();

		/// <summary>
		/// Get the details for all applications or for a specific application created in the system
		/// </summary>
		/// <param name="applicationNameFilter">Application name filter</param>
		/// <param name="continuationToken">Continuation token</param>
		/// <returns>Application list</returns>
		public Task<ApplicationList> GetApplicationListAsync(Uri applicationNameFilter, string continuationToken) =>
			m_fabricClient.QueryManager.GetApplicationListAsync(applicationNameFilter, continuationToken);

		/// <summary>
		/// Get the details for all the application types provisioned or being provisioned in the system
		/// </summary>
		/// <returns>Application type list</returns>
		public Task<ApplicationTypeList> GetApplicationTypeListAsync() =>
			m_fabricClient.QueryManager.GetApplicationTypeListAsync();

		/// <summary>
		/// Get the information about all services belonging to the application specified by the application name URI
		/// </summary>
		/// <param name="applicationName">Application name</param>
		/// <returns>Service list</returns>
		public Task<ServiceList> GetServiceListAsync(Uri applicationName) =>
			m_fabricClient.QueryManager.GetServiceListAsync(applicationName);


		/// <summary>
		/// Gets the details for all nodes in the cluster
		/// </summary>
		/// <returns>Node list</returns>
		public Task<NodeList> GetNodeListAsync() =>
			m_fabricClient.QueryManager.GetNodeListAsync();

		/// <summary>
		/// Get the details for all nodes in the cluster or for the specified node
		/// </summary>
		/// <param name="nodeNameFilter">Node name filter</param>
		/// <param name="continuationToken">Continuation token</param>
		/// <returns>Node list</returns>
		public Task<NodeList> GetNodeListAsync(string nodeNameFilter, string continuationToken) =>
			m_fabricClient.QueryManager.GetNodeListAsync(nodeNameFilter, continuationToken);


		/// <summary>
		/// The health of a Service Fabric cluster
		/// </summary>
		/// <returns>Cluster health</returns>
		public Task<ClusterHealth> GetClusterHealthAsync() =>
			m_fabricClient.HealthManager.GetClusterHealthAsync();

		/// <summary>
		/// Gets the health of the specified Service Fabric application
		/// </summary>
		/// <param name="applicationName">Application name</param>
		/// <returns>Application health</returns>
		public Task<ApplicationHealth> GetApplicationHealthAsync(Uri applicationName) =>
			m_fabricClient.HealthManager.GetApplicationHealthAsync(applicationName);

		/// <summary>
		/// Get metrics and load information on the node
		/// </summary>
		/// <param name="nodeName">Node name</param>
		/// <returns>Node load information</returns>
		public Task<NodeLoadInformation> GetNodeLoadInformationAsync(string nodeName) =>
			m_fabricClient.QueryManager.GetNodeLoadInformationAsync(nodeName);

		/// <summary>
		/// Gets the health of a Service Fabric node
		/// </summary>
		/// <param name="nodeName">Node name</param>
		/// <returns>Node health</returns>
		public Task<NodeHealth> GetNodeHealthAsync(string nodeName) =>
			m_fabricClient.HealthManager.GetNodeHealthAsync(nodeName);

		/// <summary>
		/// Reports health on a Service Fabric entity
		/// </summary>
		/// <param name="healthReport">Health report</param>
		public void ReportHealth(HealthReport healthReport) =>
			m_fabricClient.HealthManager.ReportHealth(healthReport);

		/// <summary>
		/// Get application upgrade process
		/// </summary>
		/// <param name="applicationName">Application name in Uri form</param>
		/// <returns>An UpgradeProcess object</returns>
		public Task<ApplicationUpgradeProgress> GetApplicationUpgradeProgressAsync(Uri applicationName) =>
			m_fabricClient.ApplicationManager.GetApplicationUpgradeProgressAsync(applicationName);

		/// <summary>
		/// Gets the health of a Service Fabric partition
		/// </summary>
		/// <param name="partitionId">Partition id</param>
		/// <returns>Partition health</returns>
		public Task<PartitionHealth> GetPartitionHealthAsync(Guid partitionId) =>
			m_fabricClient.HealthManager.GetPartitionHealthAsync(partitionId);

		/// <summary>
		/// Gets the details for all partitions of a service
		/// </summary>
		/// <param name="serviceName">Service name</param>
		/// <returns>Service partition list</returns>
		public Task<ServicePartitionList> GetPartitionListAsync(Uri serviceName) =>
			m_fabricClient.QueryManager.GetPartitionListAsync(serviceName);

		/// <summary>
		/// Gets the health of a Service Fabric service
		/// </summary>
		/// <param name="serviceName">Service name</param>
		/// <returns>Service health</returns>
		public Task<ServiceHealth> GetServiceHealthAsync(Uri serviceName) =>
			m_fabricClient.HealthManager.GetServiceHealthAsync(serviceName);

		/// <summary>
		/// Gets the view of replicas from a node
		/// </summary>
		/// <param name="nodeName">Node name</param>
		/// <param name="applicationName">Application name</param>
		/// <returns>Deployed service replica list</returns>
		public Task<DeployedServiceReplicaList> GetDeployedReplicaListAsync(string nodeName, Uri applicationName) =>
			m_fabricClient.QueryManager.GetDeployedReplicaListAsync(nodeName, applicationName);
	}
}
