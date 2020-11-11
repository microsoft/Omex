// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class ServiceFabricExecutionContext : BaseExecutionContext
	{
		// defined by SF https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-environment-variables-reference
		internal const string ServiceNameVariableName = "Fabric_ServiceName";
		internal const string ApplicationNameVariableName = "Fabric_ApplicationName";
		internal const string NodeNameVariableName = "Fabric_NodeName";
		internal const string NodeIPOrFQDNVariableName = "Fabric_NodeIPOrFQDN";

		public ServiceFabricExecutionContext(IHostEnvironment hostEnvironment) : base(hostEnvironment)
		{
			ServiceName = GetVariable(ServiceNameVariableName) ?? DefaultEmptyValue;
			ApplicationName = GetVariable(ApplicationNameVariableName) ?? DefaultEmptyValue;

			string nodeName = GetVariable(NodeNameVariableName) ?? DefaultEmptyValue;
			MachineId = FormattableString.Invariant($"{MachineName}_{nodeName}");

			string? nodeIPAddressOrFQDN = GetVariable(NodeIPOrFQDNVariableName);

			if (IPAddress.TryParse(nodeIPAddressOrFQDN, out IPAddress ipAddress))
			{
				ClusterIpAddress = ipAddress;
			}

			if (string.IsNullOrEmpty(Cluster) || Cluster == DefaultEmptyValue)
			{
				Cluster = nodeIPAddressOrFQDN ?? MachineId;
			}
		}
	}
}
