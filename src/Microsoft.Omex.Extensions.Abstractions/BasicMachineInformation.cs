// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace Microsoft.Omex.Extensions.Logging.Diagnostics
{
	/// <summary>
	/// Information about the current machine
	/// </summary>
	public class BasicMachineInformation : IMachineInformation
	{
		/// <summary>
		/// Agent name
		/// </summary>
		public virtual string AgentName => DefaultEmptyValue;


		/// <summary>
		/// Build version string
		/// </summary>
		public virtual string BuildVersion => s_buildVersionString.Value;


		/// <summary>
		/// The deployment slice
		/// </summary>
		public virtual string DeploymentSlice => DefaultEmptyValue;


		/// <summary>
		/// The environment name
		/// </summary>
		public virtual string EnvironmentName => DefaultEmptyValue;


		/// <summary>
		/// Is Canary environment?
		/// </summary>
		public virtual bool IsCanary => false;


		/// <summary>
		/// Is Dev Fabric environment?
		/// </summary>
		public virtual bool IsDevFabric => false;


		/// <summary>
		/// Is private deployment?
		/// </summary>
		public virtual bool IsPrivateDeployment => true;


		/// <summary>
		/// The name of the deployment cluster to which this machine belongs.
		/// </summary>
		public virtual string MachineCluster => MachineId;


		/// <summary>
		/// The ip adress of the deployment cluster to which this machine belongs.
		/// </summary>
		public virtual IPAddress MachineClusterIpAddress => s_ipAddress.Value;


		/// <summary>
		/// The number of machines in the service pool.
		/// </summary>
		public virtual int MachineCount => 1;


		/// <summary>
		/// The reporting identifier for the current machine.
		/// </summary>
		public virtual string MachineId => s_machineId.Value;


		/// <summary>
		/// The role of the host machine
		/// </summary>
		public virtual string MachineRole => DefaultEmptyValue;


		/// <summary>
		/// The region name
		/// </summary>
		public virtual string RegionName => DefaultEmptyValue;


		/// <summary>
		/// The service name
		/// </summary>
		public virtual string ServiceName => DefaultEmptyValue;


		private static readonly Lazy<string> s_buildVersionString = new Lazy<string>(
			() =>
			{
				try
				{
					FileVersionInfo buildVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

					if (buildVersion == null)
					{
						return string.Empty;
					}

					return buildVersion.FileBuildPart + "." + buildVersion.FilePrivatePart;
				}
				catch (Exception)
				{
					return string.Empty;
				}

			}, LazyThreadSafetyMode.PublicationOnly);



		private static Lazy<IPAddress> s_ipAddress = new Lazy<IPAddress>(() =>
		{
			try
			{
				IPAddress[] ipAddresses = Dns.GetHostAddresses(s_machineId.Value);
				return Array.Find(ipAddresses, address => address.AddressFamily == AddressFamily.InterNetwork) ?? IPAddress.None;
			}
			catch (Exception)
			{
				return IPAddress.None;
			}
		}, LazyThreadSafetyMode.PublicationOnly);


		private static Lazy<string> s_machineId = new Lazy<string>(() =>
		{
			try
			{
				return Environment.MachineName;
			}
			catch (Exception)
			{
				return DefaultEmptyValue;
			}
		}, LazyThreadSafetyMode.PublicationOnly);


        /// <summary>
        /// The default empty value
        /// </summary>
        protected const string DefaultEmptyValue = "None";
	}
}
