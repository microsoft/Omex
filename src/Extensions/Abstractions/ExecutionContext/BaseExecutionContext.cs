// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Abstractions.ExecutionContext
{
	/// <summary>
	/// Base class for machine information
	/// </summary>
	public class BaseExecutionContext : IExecutionContext
	{
		// Defined by Azure https://whatazurewebsiteenvironmentvariablesareavailable.azurewebsites.net/
		internal const string RegionNameVariableName = "REGION_NAME";

		// We define them
		internal const string ClusterNameVariableName = "CLUSTER_NAME";
		internal const string SliceNameVariableName = "SLICE_NAME";
		internal const string EnviromentVariableName = "DOTNET_ENVIRONMENT"; // getting environment directly only if we don't have IHostEnvironment ex. InitializationLogger

		// defined by Service Fabric https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-environment-variables-reference
		internal const string ServiceNameVariableName = "Fabric_ServiceName";
		internal const string ApplicationNameVariableName = "Fabric_ApplicationName";
		internal const string NodeNameVariableName = "Fabric_NodeName";
		internal const string NodeIPOrFQDNVariableName = "Fabric_NodeIPOrFQDN";
		internal const string FarbicFolderApplication = "Fabric_Folder_Application";

		/// <summary>
		/// Create instance of execution context
		/// Inherit from this class to populate defined properties with proper values
		/// </summary>
		public BaseExecutionContext(IHostEnvironment? hostEnvironment = null)
		{
			MachineName = GetMachineName();
			BuildVersion = GetBuildVersionFromServiceManifest() ?? DefaultEmptyValue;

			ClusterIpAddress = GetIpAddress(MachineName);

			RegionName = GetVariable(RegionNameVariableName) ?? DefaultEmptyValue;
			DeploymentSlice = GetVariable(SliceNameVariableName) ?? DefaultEmptyValue;

			if (hostEnvironment != null)
			{
				EnvironmentName = hostEnvironment.EnvironmentName ?? DefaultEmptyValue;
				IsPrivateDeployment = hostEnvironment.IsDevelopment();
			}
			else
			{
				EnvironmentName = GetVariable(EnviromentVariableName) ?? DefaultEmptyValue;
				IsPrivateDeployment = string.Equals(EnvironmentName, Environments.Development, StringComparison.OrdinalIgnoreCase);
			}

			IsCanary = false;

			// Service Fabric specific values
			ServiceName = GetVariable(ServiceNameVariableName) ?? DefaultEmptyValue;
			ApplicationName = GetVariable(ApplicationNameVariableName) ?? DefaultEmptyValue;

			string nodeName = GetVariable(NodeNameVariableName) ?? DefaultEmptyValue;
			MachineId = FormattableString.Invariant($"{MachineName}_{nodeName}");

			string? nodeIPAddressOrFQDN = GetVariable(NodeIPOrFQDNVariableName);

			if (IPAddress.TryParse(nodeIPAddressOrFQDN, out IPAddress? ipAddress))
			{
				ClusterIpAddress = ipAddress;
			}

			Cluster = GetVariable(ClusterNameVariableName) ?? nodeIPAddressOrFQDN ?? MachineId;
		}

		/// <inheritdoc/>
		public string MachineId { get; protected set; }

		/// <inheritdoc/>
		public string MachineName { get; protected set; }

		/// <inheritdoc/>
		public string ApplicationName { get; protected set; }

		/// <inheritdoc/>
		public IPAddress ClusterIpAddress { get; protected set; }

		/// <inheritdoc/>
		public string Cluster { get; protected set; }

		/// <inheritdoc/>
		public string EnvironmentName { get; protected set; }

		/// <inheritdoc/>
		public string DeploymentSlice { get; protected set; }

		/// <inheritdoc/>
		public string RegionName { get; protected set; }

		/// <inheritdoc/>
		public string ServiceName { get; protected set; }

		/// <inheritdoc/>
		public string BuildVersion { get; protected set; }

		/// <inheritdoc/>
		public bool IsCanary { get; protected set; }

		/// <inheritdoc/>
		public bool IsPrivateDeployment { get; protected set; }

		/// <summary>
		/// Get environment variable value
		/// </summary>
		protected static string? GetVariable(string name) => Environment.GetEnvironmentVariable(name);

		/// <summary>
		/// Get Machine Name from environment variable
		/// </summary>
		protected static string GetMachineName() => Environment.MachineName ?? DefaultEmptyValue;

		/// <summary>
		/// Get ip address
		/// </summary>
		protected static IPAddress GetIpAddress(string hostNameOrAddress) =>
			Array.Find(Dns.GetHostAddresses(hostNameOrAddress), address => address.AddressFamily == AddressFamily.InterNetwork)
			?? IPAddress.None;

		/// <summary>
		/// Get build version
		/// </summary>
		protected static string GetBuildVersion()
		{
			Assembly? assembly = Assembly.GetEntryAssembly();
			return assembly != null
				? FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion ?? DefaultEmptyValue
				: DefaultEmptyValue;
		}

		/// <summary>
		/// Get build version from the current running service's manifest file
		/// </summary>
		/// <param name="serviceManifestName"> Service manifest name defined in ApplicationManifest.xml </param>
		/// <returns> Build version if found, otherwise null </returns>
		protected static string? GetBuildVersionFromServiceManifest(string serviceManifestName)
		{
			string? serviceManifestPath = GetServiceManifestPath(serviceManifestName);
			return serviceManifestPath == null ? null :
				XElement.Load(serviceManifestPath).Attribute("Version")?.Value;
		}

		/// <summary>
		/// Get build version from the current running service's manifest file
		/// </summary>
		/// <returns> Build version if found, otherwise null </returns>
		protected static string? GetBuildVersionFromServiceManifest()
		{
			string? applicationDir = GetVariable(FarbicFolderApplication);
			string? serviceName = GetVariable(ServiceNameVariableName);
			string? applicationManifest = GetApplicationManifestPath();

			if (applicationDir == null || serviceName == null || applicationManifest == null)
			{
				return null;
			}

			// Firstly we load all entries from Application Manifest
			IEnumerable<XElement> descendants = XDocument
				.Load(applicationManifest)
				.Descendants();

			// Query all service specific manifest names
			IEnumerable<string> manifestNames = descendants
				.Where(entry => entry.Name.LocalName == "ServiceManifestRef")
				.Select(entry => entry.Attribute("ServiceManifestName")?.Value)
				.Where(entry => entry != null)
				.Select(entry => entry!); // we know at this point that there are not nulls.

			// Query all services and corresponding types from Application Manifest
			IEnumerable<(string serviceName, string serviceTypeName)> serviceMetaInfo = QueryServicesNamesWithTypes(descendants);

			// Query all service types and corresponding build versions from service manifest files
			IEnumerable<(string serviceTypeName, string buildVersion)> manifestMetaInfo = QueryServicesTypesWithVersion(manifestNames);

			// Match our service with corresponding build version
			try
			{
				string targetServiceType = serviceMetaInfo
					.Single(entry => entry.serviceName == serviceName.Split('/').Last())
					.serviceTypeName;
				return manifestMetaInfo
					.Single(entry => entry.serviceTypeName == targetServiceType)
					.buildVersion;
			}
			catch (Exception)
			{
				// Target service not found
				return null;
			}
		}

		internal static IEnumerable<(string serviceTypeName, string buildVersion)> QueryServicesTypesWithVersion(IEnumerable<string> manifestNames)
		{
			return manifestNames
				.Select(manifestFilename =>
				{
					string? manifestFilePath = GetServiceManifestPath(manifestFilename);
					if (manifestFilePath == null)
					{
						return (serviceTypeName: null, buildVersion: null);
					}

					IEnumerable<XElement> manifestDescendants = XDocument.Load(manifestFilePath).Descendants();
					IEnumerable<XElement> serviceManifestEntries = manifestDescendants
						.Where(entry => entry.Name.LocalName == "ServiceManifest");

					if (serviceManifestEntries.Count() != 1)
					{
						return (serviceTypeName: null, buildVersion: null);
					}

					string? buildVersion = serviceManifestEntries.Single().Attribute("Version")?.Value;

					IEnumerable<string?> serviceTypeNames = manifestDescendants
						.Where(entry => entry.Name.LocalName is "StatelessServiceType" or "StatefulServiceType")
						.Select(entry => entry.Attribute("ServiceTypeName")?.Value);

					if (serviceTypeNames.Count() != 1)
					{
						return (serviceTypeName: null, buildVersion: null);
					}

					return (serviceTypeName: serviceTypeNames.Single(), buildVersion);
				})
				.Where(entry => entry.serviceTypeName != null && entry.buildVersion != null)
				.Select(entry => (serviceTypeName: entry.serviceTypeName!, buildVersion: entry.buildVersion!));
		}

		internal static IEnumerable<(string serviceName, string serviceTypeName)> QueryServicesNamesWithTypes(IEnumerable<XElement> descendants)
		{
			return descendants
					.Where(entry => entry.Name.LocalName == "Service")
					.Select(entry =>
					{
						string? serviceName = entry.Attribute("Name")?.Value;
						IEnumerable<XElement> serviceDescendants = entry
							.Descendants().Where(entry => entry.Name.LocalName is "StatelessService" or "StatefulService");

						if (serviceDescendants.Count() != 1)
						{
							return (null, null);
						}
						XElement head = serviceDescendants.Single();
						return (serviceName, serviceTypeName: head.Attribute("ServiceTypeName")?.Value);
					})
					.Where(entry => entry.serviceName != null && entry.serviceTypeName != null)
					.Select(entry => (serviceName: entry.serviceName!, serviceTypeName: entry.serviceTypeName!)); // we know at this point that there are not nulls.
		}

		internal static string? GetApplicationManifestPath()
		{
			Regex regex = new(@"(ApplicationManifest\..*\.xml)$");
			string[] files = Directory
				.EnumerateFiles(@"C:\\SfDevCluster\", "*.*", SearchOption.AllDirectories)
				.Where(path => regex.IsMatch(path))
				.ToArray();

			return files.Length != 1 ? null : files.Single();
		}

		internal static string? GetServiceManifestPath(string serviceManifestName)
		{
			string? applicationDir = GetVariable(FarbicFolderApplication);

			if (applicationDir == null)
			{
				return null;
			}

			string serviceProperName = serviceManifestName.Replace(@"\", @"\\").Replace(".", @"\.");
			string regexExp = string.Format(@"(.*{0}.*\.Manifest\..*\.xml)$", serviceProperName);
			Regex regex = new(regexExp);

			string[] manifests = Directory.GetFiles(applicationDir).Where(
					path => regex.IsMatch(path)
				).ToArray();

			return manifests.Length != 1 ? null : manifests.Single();
		}

		/// <summary>
		/// Default empty value
		/// </summary>
		protected static readonly string DefaultEmptyValue = "None";
	}
}
