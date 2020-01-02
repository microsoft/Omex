// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.ServiceFabric.Abstractions;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.ServiceFabric.Services
{
	/// <summary> Static class that provides methods to register reliable services with </summary>
	public class OmexServiceRuntime : OmexApplicationStartup<OmexServiceFabricContext>
	{
		/// <inheritdoc/>
		public OmexServiceRuntime(string? serviceTypeName = null) : base(serviceTypeName) { }


		/// <summary>Registers a reliable stateful service with Service Fabric runtime</summary>
		/// <param name="serviceTypeName">The service type name as provied in service manifest</param>
		/// <param name="typeRegestration"> Method to register aditional types in DI </param>
		/// <param name="typeResolution">  Method to use type resolver </param>
		/// <returns>A task that represents the asynchronous register operation</returns>
		public Task RegisterStatelessServiceAsync<TService>(
			string serviceTypeName,
			Action<IServiceCollection>? typeRegestration = null,
			Action<IServiceProvider>? typeResolution = null)
			where TService : StatelessService =>
			RunAsync(
				() => ServiceRuntime.RegisterServiceAsync(ServiceTypeName, c => GetStartupObject<TService>(c)),
				typeRegestration,
				typeResolution);


		/// <summary>Registers a reliable stateful service with Service Fabric runtime</summary>
		/// <param name="serviceTypeName">The service type name as provied in service manifest</param>
		/// <param name="typeRegestration"> Method to register aditional types in DI </param>
		/// <param name="typeResolution">  Method to use type resolver </param>
		/// <returns>A task that represents the asynchronous register operation</returns>
		public Task RegisterStatefulServiceAsync<TService>(
			string serviceTypeName,
			Action<IServiceCollection>? typeRegestration = null,
			Action<IServiceProvider>? typeResolution = null)
			where TService : StatefulServiceBase =>
			RunAsync(
				() => ServiceRuntime.RegisterServiceAsync(ServiceTypeName, c => GetStartupObject<TService>(c)),
				typeRegestration,
				typeResolution);


		/// <inheritdoc/>
		protected override void LogSuccess(int processId, string serviceName) =>
			ServiceFabricServicesEventSource.Instance.LogServiceTypeRegistered(processId, serviceName);


		/// <inheritdoc/>
		protected override void LogFailure(Exception exception) =>
			ServiceFabricServicesEventSource.Instance.LogServiceHostInitializationFailed(exception.ToString());
	}
}
