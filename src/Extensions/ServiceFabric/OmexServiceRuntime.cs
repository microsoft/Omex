// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Fabric;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.ServiceFabric
{
	public static class OmexServiceRuntime
	{
		private static async Task RegisterServiceAsync()
		{

		}

		//
		// Summary:
		//     /// Registers a reliable stateless service with Service Fabric runtime. ///
		//
		// Parameters:
		//   serviceTypeName:
		//     The service type name as provied in service manifest.
		//
		//   serviceFactory:
		//     A factory method to create stateless service objects.
		//
		// Returns:
		//     /// A task that represents the asynchronous register operation. ///
		public static async Task RegisterServiceAsync(string serviceTypeName, Func<StatelessServiceContext, StatelessService> serviceFactory)
		{
			try
			{


				Task task = ServiceRuntime.RegisterServiceAsync(serviceTypeName,
					context =>
						CrossCuttingConcerns.TypeResolverForStatelessService<ClusterDiagnosticsService>(context).Resolve<ClusterDiagnosticsService>());

				ServiceFabricEventSource.Instance.LogServiceTypeRegistered(Process.GetCurrentProcess().Id, serviceTypeName);

				await task;
			}
			catch (Exception exception)
			{
				ServiceFabricEventSource.Instance.LogServiceHostInitializationFailed(exception.ToString());
				throw;
			}
		}

		//
		// Summary:
		//     /// Registers a reliable stateful service with Service Fabric runtime. ///
		//
		// Parameters:
		//   serviceTypeName:
		//     The service type name as provied in service manifest.
		//
		//   serviceFactory:
		//     A factory method to create stateful service objects.
		//
		// Returns:
		//     /// A task that represents the asynchronous register operation. ///
		public static async Task RegisterServiceAsync(string serviceTypeName, Func<StatefulServiceContext, StatefulServiceBase> serviceFactory)
		{
			try
			{
				Task task = ServiceRuntime.RegisterServiceAsync(serviceTypeName,
					context => context.CodePackageActivationContext.
						CrossCuttingConcerns.TypeResolverForStatelessService<ClusterDiagnosticsService>(context).Resolve<ClusterDiagnosticsService>());

				ServiceFabricEventSource.Instance.LogServiceTypeRegistered(Process.GetCurrentProcess().Id, serviceTypeName);

				await task;
			}
			catch (Exception exception)
			{
				ServiceFabricEventSource.Instance.LogServiceHostInitializationFailed(exception.ToString());
				throw;
			}
		}


		private static async Task RegisterServiceAsync(string serviceTypeName, Func<IServiceCollection, Task> serviceFactory)
		{
			try
			{
				IServiceCollection collection = new ServiceCollection()
					.AddLogging()
					.AddSingleton<IFooService, FooService>()
					.AddSingleton<IMonitor, MyMonitor>();


				Task task = serviceFactory(collection);
				ServiceFabricEventSource.Instance.LogServiceTypeRegistered(Process.GetCurrentProcess().Id, serviceTypeName);
				await task;
			}
			catch (Exception exception)
			{
				ServiceFabricEventSource.Instance.LogServiceHostInitializationFailed(exception.ToString());
				throw;
			}
		}
	}

	internal sealed class OmexServiceContext : IServiceContext
	{
		public OmexServiceContext(StatelessServiceContext context) : this(context, context.ReplicaOrInstanceId)
		{
		}


		public OmexServiceContext(StatefulServiceContext context) : this(context, 0L)
		{
		}


		private OmexServiceContext(ServiceContext context, long replicaOrInstanceId)
		{
			PartitionId = context.PartitionId;
			ReplicaOrInstanceId = replicaOrInstanceId;
		}


		public Guid PartitionId { get; }


		public long ReplicaOrInstanceId { get; }
	}
}
