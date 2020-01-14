// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.ServiceFabric.Abstractions
{
	internal class ServiceFabricMachineInformation : EmptyMachineInformation
	{
		public ServiceFabricMachineInformation(ServiceContext context, IOptions<MachineInformationSettings> settings)
		{
			MachineName = GetMachineName();
			DeploymentSlice = DefaultEmptyValue;
			IsCanary = false;
			MachineCount = 1;

			ServiceName = context.ServiceName.ToString();
			ICodePackageActivationContext activationContext = context.CodePackageActivationContext ?? FabricRuntime.GetActivationContext();
			MachineRole = activationContext.ApplicationName ?? DefaultEmptyValue;
			BuildVersion = activationContext.CodePackageVersion;

			NodeContext nodeContext = context.NodeContext ?? FabricRuntime.GetNodeContext();
			MachineId = FormattableString.Invariant($"{MachineName}_{nodeContext.NodeName}");
			MachineClusterIpAddress = IPAddress.TryParse(nodeContext.IPAddressOrFQDN, out IPAddress ipAddress)
				? ipAddress
				: GetIpAddress(MachineName);

			MachineInformationSettings settingsValue = settings.Value;
			EnvironmentName = settingsValue.Environment ?? DefaultEmptyValue;
			IsPrivateDeployment = string.Equals(EnvironmentName, PrEnvironmentName, StringComparison.OrdinalIgnoreCase);
			RegionName = settingsValue.Region ?? DefaultEmptyValue;
			MachineCluster = settingsValue.ClusterName
				?? nodeContext.IPAddressOrFQDN
				?? MachineId;
		}


		protected readonly string PrEnvironmentName = "Pr";
	}
}
