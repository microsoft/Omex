// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace Microsoft.Omex.Extensions.Abstractions
{
	internal class BasicMachineInformation : IMachineInformation
	{
		public virtual string AgentName => DefaultEmptyValue;


		public virtual string BuildVersion => s_buildVersionString.Value;


		public virtual string DeploymentSlice => DefaultEmptyValue;


		public virtual string EnvironmentName => DefaultEmptyValue;


		public virtual bool IsCanary => false;


		public virtual bool IsPrivateDeployment => true;


		public virtual string MachineCluster => MachineId;


		public virtual IPAddress MachineClusterIpAddress => s_ipAddress.Value;


		public virtual int MachineCount => 1;


		public virtual string MachineId => s_machineId.Value;


		public virtual string MachineRole => DefaultEmptyValue;


		public virtual string RegionName => DefaultEmptyValue;


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


		protected const string DefaultEmptyValue = "None";
	}
}
