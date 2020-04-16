// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class ServiceFabricExecutionContext : EmptyExecutionContext
	{
		public ServiceFabricExecutionContext(IHostEnvironment hostEnvironment, IAccessor<ServiceContext> accessor)
		{
			MachineName = GetMachineName();
			// In generic host context application is what it's running, Service Fabric service in this case, so it's application name is service fabric service name
			ServiceName = hostEnvironment.ApplicationName;
			EnvironmentName = hostEnvironment.EnvironmentName ?? DefaultEmptyValue;
			IsPrivateDeployment = hostEnvironment.IsDevelopment();

			accessor.OnUpdated(UpdateState);
		}

		private string? GetRegionName() =>
			Environment.GetEnvironmentVariable("REGION_NAME"); // should be defined by Azure https://whatazurewebsiteenvironmentvariablesareavailable.azurewebsites.net/

		private string? GetClusterName() =>
			Environment.GetEnvironmentVariable("CLUSTER_NAME"); // We should define it

		private void UpdateState(ServiceContext context)
		{
			ICodePackageActivationContext activationContext = context.CodePackageActivationContext;
			ApplicationName = activationContext.ApplicationName ?? DefaultEmptyValue;
			BuildVersion = activationContext.CodePackageVersion;

			NodeContext nodeContext = context.NodeContext;
			MachineId = FormattableString.Invariant($"{MachineName}_{nodeContext.NodeName}");
			ClusterIpAddress = IPAddress.TryParse(nodeContext.IPAddressOrFQDN, out IPAddress ipAddress)
				? ipAddress
				: GetIpAddress(MachineName);

			RegionName = GetRegionName() ?? DefaultEmptyValue;
			Cluster = GetClusterName()
				?? nodeContext.IPAddressOrFQDN
				?? MachineId;
		}
	}
}
