// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class OmexStatefulServiceRunner : OmexServiceRunner<OmexStatefulService, StatefulServiceContext>
	{
		private readonly IAccessorSetter<IReliableStateManager> m_stateAccessor;

		public OmexStatefulServiceRunner(
			IHostEnvironment environment,
			IAccessorSetter<OmexStatefulService> serviceAccessor,
			IAccessorSetter<IReliableStateManager> stateAccessor,
			IAccessorSetter<StatefulServiceContext> contextAccessor,
			IEnumerable<IListenerBuilder<StatefulServiceContext>> listenerBuilders,
			IEnumerable<IServiceAction<StatefulServiceContext>> serviceActions)
				: base(environment, serviceAccessor, contextAccessor, listenerBuilders, serviceActions)
		{
			m_stateAccessor = stateAccessor;
		}

		public override Task RunServiceAsync(CancellationToken cancellationToken) =>
			ServiceRuntime.RegisterServiceAsync(ApplicationName, ServiceFactory, cancellationToken: cancellationToken);

		private StatefulService ServiceFactory(StatefulServiceContext context)
		{
			ContextAccessor.SetValue(context);
			OmexStatefulService service = new OmexStatefulService(this, context);
			ServiceAccessor.SetValue(service);
			m_stateAccessor.SetValue(service.StateManager);
			return service;
		}
	}
}
