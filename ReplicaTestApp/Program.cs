using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using System.Collections.Generic;
using ServiceFabric.Mocks;
using Microsoft.Omex.Extensions.Hosting.Services.Internal;

namespace ReplicaTestApp
{
    internal static class Program
    {

		public class AccessorSetter<T> : IAccessorSetter<T> where T : class
		{
			private T? m_value;
			public void SetValue(T value) => m_value = value;
			public T GetValue() => m_value!;
		}

		/// <summary>
		/// This is the entry point of the service host process.
		/// </summary>
		private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("ReplicaTestAppType",
                    context => new ReplicaTestApp(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(ReplicaTestApp).Name);

				// Prevents this host process from terminating so services keeps running.


				// Initialize the stateful service registrator
				ServiceRegistratorOptions serviceRegistratorOptions = new ServiceRegistratorOptions { ServiceTypeName = "MyServiceType" };
				IOptions<ServiceRegistratorOptions> options = Options.Create(serviceRegistratorOptions);
				IAccessorSetter<StatefulServiceContext> contextAccessor = new AccessorSetter<StatefulServiceContext>(); // Initialize appropriately
				IAccessorSetter<IStatefulServicePartition> partitionAccessor = new AccessorSetter<IStatefulServicePartition>(); // Initialize appropriately
				IAccessorSetter<IReliableStateManager> stateAccessor = new AccessorSetter<IReliableStateManager>(); // Initialize appropriately
				IAccessorSetter<OmexStateManager> roleAccessor = new AccessorSetter<OmexStateManager>(); // Initialize appropriately
				IEnumerable<IListenerBuilder<OmexStatefulService>> listenerBuilders = new List<IListenerBuilder<OmexStatefulService>>(); // Initialize appropriately
				IEnumerable<IServiceAction<OmexStatefulService>> serviceActions = new List<IServiceAction<OmexStatefulService>>(); // Initialize appropriately

				//OmexStatefulServiceRegistrator serviceRegistrator = new OmexStatefulServiceRegistrator(
				//	options,
				//	contextAccessor,
				//	partitionAccessor,
				//	stateAccessor,
				//	roleAccessor,
				//	listenerBuilders,
				//	serviceActions);

				// Create a mock StatefulServiceContext
				NodeContext nodeContext = new NodeContext("nodeName", new NodeId(0, 0), 0, "nodeType", "ipAddress");
				ICodePackageActivationContext codePackageActivationContext = new MockCodePackageActivationContext(
					"applicationName",
					"applicationTypeName",
					"codePackageName",
					"codePackageVersion",
					"contextId",
					"logDirectory",
					"tempDirectory",
					"workDirectory",
					"serviceManifestName",
					"serviceManifestVersion"
				);
				StatefulServiceContext context = new StatefulServiceContext(nodeContext, codePackageActivationContext, "serviceTypeName", new Uri("fabric:/AppName/ServiceName"), null, Guid.NewGuid(), long.MaxValue);

				IReliableStateManager reliableStateManager = new MockReliableStateManager();
				OmexStateManager omexStateManager = new(reliableStateManager, ReplicaRole.Primary);

				// Call OnChangeRoleAsync with appropriate parameters
				//await statefulService.ChangeRoleAsyncTest(ReplicaRole.Primary, CancellationToken.None);

				ReplicaRole currentRole = omexStateManager.GetRole();
				Console.WriteLine($"Current Replica Role: {currentRole}");
				Console.WriteLine($"IsReadable {omexStateManager.IsReadable}");
				Console.WriteLine($"IsWritablee {omexStateManager.IsWritable}");


				Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
