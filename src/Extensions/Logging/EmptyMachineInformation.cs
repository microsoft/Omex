// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>Base class for machine information</summary>
	public class EmptyMachineInformation : IMachineInformation
	{
		/// <summary>Create instance of machine information</summary>
		public EmptyMachineInformation()
		{
			MachineName = DefaultEmptyValue;
			MachineId = DefaultEmptyValue;
			MachineRole = DefaultEmptyValue;
			MachineClusterIpAddress = IPAddress.None;
			MachineCount = 1;
			MachineCluster = MachineId;

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
		public string MachineRole { get; protected set; }


		/// <inheritdoc/>
		public IPAddress MachineClusterIpAddress { get; protected set; }


		/// <inheritdoc/>
		public int MachineCount { get; protected set; }


		/// <inheritdoc/>
		public string MachineCluster { get; protected set; }


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
			return buildVersion != null
				? string.Concat(buildVersion.FileBuildPart, ".", buildVersion.FilePrivatePart)
				: string.Empty;
		}


		/// <summary>default empty value</summary>
		protected readonly string DefaultEmptyValue = "None";
	}
}
