// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Compatability;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.ServiceFabric
{
	/// <summary> Static class that provides methods to register reliable services with </summary>
	public static class OmexServiceRuntime
	{
		/// <summary>Registers a reliable stateful service with Service Fabric runtime</summary>
		/// <param name="serviceTypeName">The service type name as provied in service manifest</param>
		/// <param name="typeRegestration"> Method to register aditional types in DI </param>
		/// <param name="typeResolution">  Method to use type resolver </param>
		/// <returns>A task that represents the asynchronous register operation</returns>
		public static Task RegisterStatelessServiceAsync<TService>(
			string serviceTypeName,
			Action<IServiceCollection>? typeRegestration = null,
			Action<IServiceProvider>? typeResolution = null)
			where TService : StatelessService =>
			ExecuteServiceRegistrationAsync(
				serviceTypeName,
				() => ServiceRuntime.RegisterServiceAsync(
					serviceTypeName,
					context => GetService<TService, StatelessServiceContext>(context, typeRegestration, typeResolution)));


		/// <summary>Registers a reliable stateful service with Service Fabric runtime</summary>
		/// <param name="serviceTypeName">The service type name as provied in service manifest</param>
		/// <param name="typeRegestration"> Method to register aditional types in DI </param>
		/// <param name="typeResolution">  Method to use type resolver </param>
		/// <returns>A task that represents the asynchronous register operation</returns>
		public static Task RegisterStatefulServiceAsync<TService>(
			string serviceTypeName,
			Action<IServiceCollection>? typeRegestration = null,
			Action<IServiceProvider>? typeResolution = null)
			where TService : StatefulServiceBase =>
			ExecuteServiceRegistrationAsync(
				serviceTypeName,
				() => ServiceRuntime.RegisterServiceAsync(
					serviceTypeName,
					context => GetService<TService, StatefulServiceContext>(context, typeRegestration, typeResolution)));


		private static async Task ExecuteServiceRegistrationAsync(
			string serviceTypeName,
			Func<Task> serviceRegestration)
		{
			try
			{
				Task task = serviceRegestration();
				ServiceFabricEventSource.Instance.LogServiceTypeRegistered(Process.GetCurrentProcess().Id, serviceTypeName);
				await task;
			}
			catch (Exception exception)
			{
				ServiceFabricEventSource.Instance.LogServiceHostInitializationFailed(exception.ToString());
				throw;
			}
		}


		private static TService GetService<TService, TContext>(
			TContext context,
			Action<IServiceCollection>? typeRegestration = null,
			Action<IServiceProvider>? typeResolution = null)
			where TContext : class
		{
			IServiceCollection collection = new ServiceCollection()
				.AddOmexLogging<OmexServiceContext>()
				.AddTimedScopes()
				.AddSingleton(context);

			typeRegestration?.Invoke(collection);

			IServiceProvider provider = collection.BuildServiceProvider();

			provider.InitializeOmexCompatabilityClasses();

			typeResolution?.Invoke(provider);

			return provider.GetService<TService>();
		}

		private class OmexServiceContext : IServiceContext
		{
			protected OmexServiceContext(ServiceContext context, long replicaOrInstanceId)
			{
				PartitionId = context.PartitionId;
				ReplicaOrInstanceId = replicaOrInstanceId;
			}


			public Guid PartitionId { get; }


			public long ReplicaOrInstanceId { get; }
		}
	}
}
