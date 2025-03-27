// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Abstractions.ExecutionContext
{
	/// <summary>
	/// Base class for machine information
	/// </summary>
	public class BaseExecutionContext : IExecutionContext
	{
		// Set as an environment variable (e.g. through ARM template deployment)
		internal const string RegionNameVariableName = "REGION_NAME";
		internal const string RegionShortNameVariableName = "REGION_SHORT_NAME";
		internal const string ClusterNameVariableName = "CLUSTER_NAME";
		internal const string SliceNameVariableName = "SLICE_NAME";
		internal const string AspNetCoreEnviromentVariableName = "ASPNETCORE_ENVIRONMENT";
		internal const string DotNetEnviromentVariableName = "DOTNET_ENVIRONMENT"; // getting environment directly only if we don't have IHostEnvironment ex. InitializationLogger

		// defined by Service Fabric https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-environment-variables-reference
		internal const string ServiceNameVariableName = "Fabric_ServiceName";
		internal const string ApplicationNameVariableName = "Fabric_ApplicationName";
		internal const string NodeNameVariableName = "Fabric_NodeName";
		internal const string NodeIPOrFQDNVariableName = "Fabric_NodeIPOrFQDN";

		/// <summary>
		/// Create instance of execution context
		/// Inherit from this class to populate defined properties with proper values
		/// </summary>
		public BaseExecutionContext(IHostEnvironment? hostEnvironment = null)
		{
			MachineName = GetMachineName();
			BuildVersion = GetBuildVersion();

			ClusterIpAddress = GetIpAddress(MachineName);

			RegionName = GetVariable(RegionNameVariableName) ?? DefaultEmptyValue;
			RegionShortName = GetVariable(RegionShortNameVariableName) ?? DefaultEmptyValue;
			DeploymentSlice = GetVariable(SliceNameVariableName) ?? DefaultEmptyValue;

			if (hostEnvironment != null)
			{
				EnvironmentName = hostEnvironment.EnvironmentName ?? DefaultEmptyValue;
				IsPrivateDeployment = hostEnvironment.IsDevelopment();
			}
			else
			{
				EnvironmentName = GetVariable(AspNetCoreEnviromentVariableName)
					?? GetVariable(DotNetEnviromentVariableName)
					?? Environments.Development;
				IsPrivateDeployment = string.Equals(EnvironmentName, Environments.Development, StringComparison.OrdinalIgnoreCase);
			}

			IsCanary = false;

			// Service Fabric specific values
			ServiceName = GetVariable(ServiceNameVariableName) ?? DefaultEmptyValue;
			ApplicationName = GetVariable(ApplicationNameVariableName) ?? DefaultEmptyValue;

			NodeName = GetVariable(NodeNameVariableName) ?? DefaultEmptyValue;
			MachineId = FormattableString.Invariant($"{MachineName}_{NodeName}");

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
		public string NodeName { get; protected set; }

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
		public string RegionShortName { get; protected set; }

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
			string buildVersion = DefaultEmptyValue;

			Assembly? assembly = Assembly.GetEntryAssembly();
			if (assembly != null)
			{
				FileVersionInfo assemblyVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
				// We used assemblyVersion.ProductVersion previously, but in net8 it was changed to include commit hash.
				// More details here: https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/8.0/source-link
				buildVersion = $"{assemblyVersion.ProductMajorPart}.{assemblyVersion.ProductMinorPart}.{assemblyVersion.ProductBuildPart}.{assemblyVersion.ProductPrivatePart}";
			}

			return buildVersion;
		}

		/// <summary>
		/// Default empty value
		/// </summary>
		protected static readonly string DefaultEmptyValue = "None";
	}
}
