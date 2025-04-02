// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.Omex.Extensions.Hosting.Services;
using static Microsoft.Omex.Extensions.Hosting.Services.OmexStatefulServiceRegistrator;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Omex implementation of stateful service fabric service
	/// </summary>
	public sealed class OmexStatefulService : StatefulService, IServiceFabricService<StatefulServiceContext>
	{
		private readonly OmexStatefulServiceRegistrator m_serviceRegistrator;
		private readonly IAccessorSetter<ReplicaRoleWrapper> m_replicaRoleAccessor;
		// Add a default initialization for m_replicaRoleWrapper to avoid CS8618
		private ReplicaRoleWrapper m_replicaRoleWrapper = new ReplicaRoleWrapper();

		/// <summary>
		/// Initializes a new instance of the <see cref="OmexStatefulService"/> class.
		/// </summary>
		/// <param name="serviceRegistrator">The service registrator.</param>
		/// <param name="serviceContext">The stateful service context.</param>
		public OmexStatefulService(
			OmexStatefulServiceRegistrator serviceRegistrator,
			StatefulServiceContext serviceContext)
			: base(serviceContext)
		{
			serviceRegistrator.ContextAccessor.SetValue(Context);
			serviceRegistrator.StateAccessor.SetValue(StateManager);
			m_serviceRegistrator = serviceRegistrator;
			m_replicaRoleAccessor = serviceRegistrator.RoleAccessor;
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
			m_replicaRoleWrapper = new ReplicaRoleWrapper { Role = newRole };
			m_serviceRegistrator.RoleAccessor.SetValue(m_replicaRoleWrapper);
			return base.OnChangeRoleAsync(newRole, cancellationToken);
		}

		/// <inheritdoc />
		public Task ChangeRoleAsyncTest(ReplicaRole newRole, CancellationToken cancellationToken)
		{
			m_replicaRoleWrapper = new ReplicaRoleWrapper { Role = newRole };
			m_serviceRegistrator.RoleAccessor.SetValue(m_replicaRoleWrapper);
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() =>
					m_serviceRegistrator.ListenerBuilders.Select(b => new ServiceReplicaListener(c => b.Build(this), b.Name));

		/// <inheritdoc />
		protected override Task RunAsync(CancellationToken cancellationToken) =>
			Task.WhenAll(m_serviceRegistrator.ServiceActions.Select(r => r.RunAsync(this, cancellationToken)));

		/// <summary>
		/// Gets the current replica role.
		/// </summary>
		public ReplicaRole GetCurrentReplicaRole() => m_replicaRoleWrapper?.Role ?? ReplicaRole.Unknown;
	}
}
