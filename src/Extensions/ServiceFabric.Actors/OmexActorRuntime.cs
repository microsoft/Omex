// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.ServiceFabric.Abstractions;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Microsoft.Omex.Extensions.ServiceFabric.Actors
{

	/// <summary>
	/// Contains methods to register actor and actor service types with Service Fabric
	/// runtime. Registering the types allows the runtime to create instances of the
	/// actor and the actor service. See https://docs.microsoft.com/azure/service-fabric/service-fabric-reliable-actors-lifecycle
	/// for more information on the lifecycle of an actor.
	/// </summary>
	public class OmexActorRuntime : OmexApplicationStartup<OmexServiceFabricContext>
	{
		/// <inheritdoc/>
		public OmexActorRuntime(string? serviceTypeName = null) : base(serviceTypeName) { }


		/// <summary>
		/// Registers an actor service with Service Fabric runtime. This allows the runtime to create instances of the replicas for the actor service
		/// </summary>
		/// <typeparam name="TActor">Type implementing actor</typeparam>
		/// <typeparam name="TActorService">Type implementing actor service</typeparam>
		/// <param name="typeRegestration"></param>
		/// <param name="typeResolution"></param>
		/// <returns></returns>
		public Task RegisterActorAsync<TActor, TActorService>(
			Action<IServiceCollection>? typeRegestration = null,
			Action<IServiceProvider>? typeResolution = null)
			where TActor : ActorBase
			where TActorService : ActorService =>
			RunAsync(
				() => ActorRuntime.RegisterActorAsync<TActor>((context, information) => GetStartupObject<ActorService>(context, information)),
				typeRegestration,
				typeResolution);


		/// <inheritdoc/>
		protected override void LogSuccess(int processId, string serviceName) =>
			ServiceFabricAgentsEventSource.Instance.LogActorTypeRegistered(processId, serviceName);


		/// <inheritdoc/>
		protected override void LogFailure(Exception exception) =>
			ServiceFabricAgentsEventSource.Instance.LogActorHostInitializationFailed(exception.ToString());
	}
}
