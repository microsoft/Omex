﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
		private readonly OmexStatefulServiceRegistrator m_serviceRegistrator;
		private OmexStateManager m_omexStateManager;

		internal OmexStatefulService(
			OmexStatefulServiceRegistrator serviceRegistrator,
			StatefulServiceContext serviceContext)
				: base(serviceContext)
		{
			serviceRegistrator.ContextAccessor.SetValue(Context);
			serviceRegistrator.StateAccessor.SetValue(StateManager);
			m_serviceRegistrator = serviceRegistrator;
			m_omexStateManager = new OmexStateManager(StateManager, ReplicaRole.Unknown);
		}

		/// <inheritdoc />
		protected override Task OnOpenAsync(ReplicaOpenMode openMode, CancellationToken cancellationToken)
		{
			m_serviceRegistrator.PartitionAccessor.SetValue(Partition);
			return base.OnOpenAsync(openMode, cancellationToken);
		}
		
		/// <inheritdoc/>
		protected override Task OnChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
		{
			m_omexStateManager = new OmexStateManager(StateManager, newRole);
			m_serviceRegistrator.RoleAccessor.SetValue(m_omexStateManager);
			return base.OnChangeRoleAsync(newRole, cancellationToken);
		}
		
		/// <inheritdoc />
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() =>
			m_serviceRegistrator.ListenerBuilders.Select(b => new ServiceReplicaListener(c => b.Build(this), b.Name));

		/// <inheritdoc />
		protected override Task RunAsync(CancellationToken cancellationToken) =>
			Task.WhenAll(m_serviceRegistrator.ServiceActions.Select(r => r.RunAsync(this, cancellationToken)));
	}
}
