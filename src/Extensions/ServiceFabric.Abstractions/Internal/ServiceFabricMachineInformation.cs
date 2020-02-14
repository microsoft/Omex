// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal class ServiceFabricMachineInformation : EmptyMachineInformation
	{
		public ServiceFabricMachineInformation(IHostEnvironment hostEnvironment)
		{
			MachineName = GetMachineName();
			DeploymentSlice = DefaultEmptyValue;
			IsCanary = false;
			MachineCount = 1;

			ServiceName = hostEnvironment.ApplicationName;
			ICodePackageActivationContext activationContext = FabricRuntime.GetActivationContext();
			MachineRole = activationContext.ApplicationName ?? DefaultEmptyValue;
			BuildVersion = activationContext.CodePackageVersion;

			NodeContext nodeContext = FabricRuntime.GetNodeContext();
			MachineId = FormattableString.Invariant($"{MachineName}_{nodeContext.NodeName}");
			MachineClusterIpAddress = IPAddress.TryParse(nodeContext.IPAddressOrFQDN, out IPAddress ipAddress)
				? ipAddress
				: GetIpAddress(MachineName);

			EnvironmentName = hostEnvironment.EnvironmentName ?? DefaultEmptyValue;
			IsPrivateDeployment = string.Equals(EnvironmentName, PrEnvironmentName, StringComparison.OrdinalIgnoreCase);
			RegionName = GetRegionName() ?? DefaultEmptyValue;
			MachineCluster = GetClusterName()
				?? nodeContext.IPAddressOrFQDN
				?? MachineId;
		}


		private string GetRegionName() =>
			Environment.GetEnvironmentVariable("REGION_NAME"); // should be defined by Azure


		private string GetClusterName() =>
			Environment.GetEnvironmentVariable("CLUSTER_NAME"); // We should define it


		protected readonly string PrEnvironmentName = "Pr";
	}


	//internal class OldServiceFabricMachineInformation : EmptyMachineInformation
	//{
	//	public OldServiceFabricMachineInformation(ServiceContext context, IOptions<MachineInformationSettings> settings)
	//	{
	//		MachineName = GetMachineName();
	//		DeploymentSlice = DefaultEmptyValue;
	//		IsCanary = false;
	//		MachineCount = 1;

	//		ServiceName = context.ServiceName.ToString();
	//		ICodePackageActivationContext activationContext = context.CodePackageActivationContext ?? FabricRuntime.GetActivationContext();
	//		MachineRole = activationContext.ApplicationName ?? DefaultEmptyValue;
	//		BuildVersion = activationContext.CodePackageVersion;

	//		NodeContext nodeContext = context.NodeContext ?? FabricRuntime.GetNodeContext();
	//		MachineId = FormattableString.Invariant($"{MachineName}_{nodeContext.NodeName}");
	//		MachineClusterIpAddress = IPAddress.TryParse(nodeContext.IPAddressOrFQDN, out IPAddress ipAddress)
	//			? ipAddress
	//			: GetIpAddress(MachineName);

	//		MachineInformationSettings settingsValue = settings.Value;
	//		EnvironmentName = settingsValue.Environment ?? DefaultEmptyValue;
	//		IsPrivateDeployment = string.Equals(EnvironmentName, PrEnvironmentName, StringComparison.OrdinalIgnoreCase);
	//		RegionName = settingsValue.Region ?? DefaultEmptyValue;
	//		MachineCluster = settingsValue.ClusterName
	//			?? nodeContext.IPAddressOrFQDN
	//			?? MachineId;
	//	}


	//	protected readonly string PrEnvironmentName = "Pr";
	//}

	//internal class MachineInformationSettings
	//{
	//	public string? Environment { get; set; }
	//	public string? Region { get; set; }
	//	public string? ClusterName { get; set; }
	//}
}
