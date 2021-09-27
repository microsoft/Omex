// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Omex implementation of stateless service fabric service
	/// </summary>
	public sealed class OmexStatelessService : StatelessService, IServiceFabricService<StatelessServiceContext>
	{
		private readonly OmexStatelessServiceRegistrator m_serviceRegistrator;

		internal OmexStatelessService(
			OmexStatelessServiceRegistrator serviceRegistrator,
			StatelessServiceContext serviceContext)
				: base(serviceContext)
		{
			serviceRegistrator.ContextAccessor.SetValue(serviceContext);
			m_serviceRegistrator = serviceRegistrator;
		}


		/// <inheritdoc />
		protected override Task OnOpenAsync(CancellationToken cancellationToken)
		{
			m_serviceRegistrator.PartitionAccessor.SetValue(Partition);
			return base.OnOpenAsync(cancellationToken);
		}

		/// <inheritdoc />
		protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners() =>
			m_serviceRegistrator.ListenerBuilders.Select(b => new ServiceInstanceListener(c => b.Build(this), b.Name));

		/// <inheritdoc />
		protected override Task RunAsync(CancellationToken cancellationToken) =>
			Task.WhenAll(m_serviceRegistrator.ServiceActions.Select(r => r.RunAsync(this, cancellationToken)));
	}
}
