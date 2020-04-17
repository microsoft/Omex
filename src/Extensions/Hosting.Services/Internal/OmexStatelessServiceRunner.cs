// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class OmexStatelessServiceRunner : OmexServiceRunner<OmexStatelessService, StatelessServiceContext>
	{
		public OmexStatelessServiceRunner(
			IHostEnvironment environment,
			IAccessorSetter<StatelessServiceContext> contextAccessor,
			IEnumerable<IListenerBuilder<OmexStatelessService>> listenerBuilders,
			IEnumerable<IServiceAction<OmexStatelessService>> serviceActions)
				: base(environment, contextAccessor, listenerBuilders, serviceActions) { }

		public override Task RunServiceAsync(CancellationToken cancellationToken) =>
			ServiceRuntime.RegisterServiceAsync(ApplicationName, ServiceFactory, cancellationToken: cancellationToken);

		private StatelessService ServiceFactory(StatelessServiceContext context)
		{
			ContextAccessor.SetValue(context);
			return new OmexStatelessService(this, context);
		}
	}
}
