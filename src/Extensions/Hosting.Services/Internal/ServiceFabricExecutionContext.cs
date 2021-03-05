// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	// TODO: should be removed after our services will set service executable version properly
	internal sealed class ServiceFabricExecutionContext : BaseExecutionContext
	{
		public ServiceFabricExecutionContext(IHostEnvironment hostEnvironment, IAccessor<ServiceContext> accessor)
			: base(hostEnvironment) =>
				accessor.OnFirstSet(UpdateState);

		private void UpdateState(ServiceContext context) =>
			BuildVersion = context.CodePackageActivationContext.CodePackageVersion;
	}
}
