// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
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
		internal const string ServicePackageVariableName = "Fabric_ServicePackageName";
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
			ServicePackageName = GetVariable(ServicePackageVariableName) ?? DefaultEmptyValue;
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

		/// <inheritdoc/>
		public string ServicePackageName { get; protected set; }

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
		/// <returns> Build version if found, otherwise null </returns>
		protected string? GetBuildVersionFromServiceManifest()
		{
			string? serviceManifestPath = GetServiceManifestPath();
			return serviceManifestPath == null ? null :
				XElement.Load(serviceManifestPath).Attribute("Version")?.Value;
		}


		private string? GetServiceManifestPath()
		{
			string? applicationDir = GetVariable(FarbicFolderApplication);
			string? serviceManifestName = ServicePackageName;

			if (applicationDir == null || serviceManifestName == null)
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
