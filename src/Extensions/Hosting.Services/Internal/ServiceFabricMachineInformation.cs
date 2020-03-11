// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class ServiceFabricMachineInformation : EmptyMachineInformation
	{
		public ServiceFabricMachineInformation(IHostEnvironment hostEnvironment, IServiceContextAccessor<ServiceContext> accessor)
		{
			MachineName = GetMachineName();
			MachineCount = 1;
			ServiceName = hostEnvironment.ApplicationName;
			EnvironmentName = hostEnvironment.EnvironmentName ?? DefaultEmptyValue;
			IsPrivateDeployment = hostEnvironment.IsDevelopment();
			RegionName = GetRegionName() ?? DefaultEmptyValue;

			accessor.OnContextAvailable(UpdateState);
		}


		private string? GetRegionName() =>
			Environment.GetEnvironmentVariable("REGION_NAME"); // should be defined by Azure http://whatazurewebsiteenvironmentvariablesareavailable.azurewebsites.net/


		private string? GetClusterName() =>
			Environment.GetEnvironmentVariable("CLUSTER_NAME"); // We should define it


		private void UpdateState(ServiceContext context)
		{
			ICodePackageActivationContext activationContext = context.CodePackageActivationContext;
			MachineRole = activationContext.ApplicationName ?? DefaultEmptyValue;
			BuildVersion = activationContext.CodePackageVersion;

			NodeContext nodeContext = context.NodeContext;
			MachineId = FormattableString.Invariant($"{MachineName}_{nodeContext.NodeName}");
			MachineClusterIpAddress = IPAddress.TryParse(nodeContext.IPAddressOrFQDN, out IPAddress ipAddress)
				? ipAddress
				: GetIpAddress(MachineName);

			MachineCluster = GetClusterName()
				?? nodeContext.IPAddressOrFQDN
				?? MachineId;
		}
	}
}
