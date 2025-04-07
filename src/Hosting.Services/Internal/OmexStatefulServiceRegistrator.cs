// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.Omex.Extensions.Hosting.Services.Internal;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Registers OmexStatefulService with the Service Fabric runtime.
	/// </summary>
	internal sealed class OmexStatefulServiceRegistrator : OmexServiceRegistrator<OmexStatefulService, StatefulServiceContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OmexStatefulServiceRegistrator"/> class.
		/// </summary>
		/// <param name="options">The service registrator options.</param>
		/// <param name="contextAccessor">The context accessor.</param>
		/// <param name="partitionAccessor">The partition accessor.</param>
		/// <param name="stateAccessor">The state accessor.</param>
		/// <param name="roleAccessor">The role accessor.</param>
		/// <param name="listenerBuilders">The listener builders.</param>
		/// <param name="serviceActions">The service actions.</param>
		public OmexStatefulServiceRegistrator(
			IOptions<ServiceRegistratorOptions> options,
			IAccessorSetter<StatefulServiceContext> contextAccessor,
			IAccessorSetter<IStatefulServicePartition> partitionAccessor,
			IAccessorSetter<IReliableStateManager> stateAccessor,
			IAccessorSetter<OmexStateManager> roleAccessor,
			IEnumerable<IListenerBuilder<OmexStatefulService>> listenerBuilders,
			IEnumerable<IServiceAction<OmexStatefulService>> serviceActions)
				: base(options, contextAccessor, listenerBuilders, serviceActions)
		{
			PartitionAccessor = partitionAccessor;
			StateAccessor = stateAccessor;
			RoleAccessor = roleAccessor;
		}

		/// <summary>
		/// Registers the OmexStatefulService with the Service Fabric runtime.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous registration operation.</returns>
		public override Task RegisterAsync(CancellationToken cancellationToken) =>
			ServiceRuntime.RegisterServiceAsync(Options.ServiceTypeName, context => new OmexStatefulService(this, context), cancellationToken: cancellationToken);

		/// <summary>
		/// Gets the accessor for the reliable state manager.
		/// </summary>
		public IAccessorSetter<IReliableStateManager> StateAccessor { get; }

		/// <summary>
		/// Gets the accessor for the stateful service partition.
		/// </summary>
		public IAccessorSetter<IStatefulServicePartition> PartitionAccessor { get; }

		/// <summary>
		/// Gets the accessor for the Omex state manager.
		/// </summary>
		public IAccessorSetter<OmexStateManager> RoleAccessor { get; }
	}
}
