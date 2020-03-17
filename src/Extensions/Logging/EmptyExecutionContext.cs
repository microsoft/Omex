// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Base class for machine information
	/// </summary>
	public class EmptyExecutionContext : IExecutionContext
	{
		/// <summary>
		/// Create instance of empty execution context
		/// Inherit from this class to populate defined properties with proper values
		/// </summary>
		/// <remarks>
		/// Please keep this initialization as lightweight as possible
		/// </remarks>
		protected EmptyExecutionContext()
		{
			MachineName = DefaultEmptyValue;
			MachineId = DefaultEmptyValue;
			ApplicationName = DefaultEmptyValue;
			ClusterIpAddress = IPAddress.None;
			Cluster = MachineId;

			EnvironmentName = DefaultEmptyValue;
			DeploymentSlice = DefaultEmptyValue;

			RegionName = DefaultEmptyValue;
			ServiceName = DefaultEmptyValue;
			BuildVersion = DefaultEmptyValue;

			IsCanary = false;
			IsPrivateDeployment = false;
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


		/// <summary>Get Machine Name from eviroment variable</summary>
		protected string GetMachineName() => Environment.MachineName ?? DefaultEmptyValue;


		/// <summary>Get ip address</summary>
		protected IPAddress GetIpAddress(string hostNameOrAddress) =>
			Array.Find(Dns.GetHostAddresses(hostNameOrAddress), address => address.AddressFamily == AddressFamily.InterNetwork)
			?? IPAddress.None;


		/// <summary>Get build version</summary>
		protected string GetBuildVersion()
		{
			FileVersionInfo buildVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			return string.Concat(buildVersion.FileBuildPart, ".", buildVersion.FilePrivatePart);
		}


		/// <summary>default empty value</summary>
		protected readonly string DefaultEmptyValue = "None";
	}
}
