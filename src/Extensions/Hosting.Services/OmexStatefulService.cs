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
	/// Omex implementation of stateful service fabric service
	/// </summary>
	public sealed class OmexStatefulService : StatefulService, IServiceFabricService<StatefulServiceContext>
	{
		private readonly OmexStatefulServiceRunner m_serviceParameters;

		internal OmexStatefulService(
			OmexStatefulServiceRunner serviceRunner,
			StatefulServiceContext serviceContext)
				: base(serviceContext) => m_serviceParameters = serviceRunner;

		/// <inheritdoc />
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() =>
			m_serviceParameters.ListenerBuilders.Select(b => new ServiceReplicaListener(b.Build, b.Name));

		/// <inheritdoc />
		protected override Task RunAsync(CancellationToken cancellationToken) =>
			Task.WhenAll(m_serviceParameters.ServiceActions.Select(r => r.RunAsync(cancellationToken)));
	}
}
