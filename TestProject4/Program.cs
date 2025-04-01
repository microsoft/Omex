using System;
using System.Collections.Generic;
using System.Fabric;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Mocks;

public class Program
{
	public static void Main(string[] args)
	{
		// Initialize the stateful service registrator
		ServiceRegistratorOptions serviceRegistratorOptions = new ServiceRegistratorOptions { ServiceTypeName = "MyServiceType" };
		IOptions<ServiceRegistratorOptions> options = Options.Create(serviceRegistratorOptions);
		IAccessorSetter<StatefulServiceContext> contextAccessor = new AccessorSetter<StatefulServiceContext>(); // Initialize appropriately
		IAccessorSetter<IStatefulServicePartition> partitionAccessor = new AccessorSetter<IStatefulServicePartition>(); // Initialize appropriately
		IAccessorSetter<IReliableStateManager> stateAccessor = new AccessorSetter<IReliableStateManager>(); // Initialize appropriately
		IAccessorSetter<OmexStatefulServiceRegistrator.ReplicaRoleWrapper> roleAccessor = new AccessorSetter<OmexStatefulServiceRegistrator.ReplicaRoleWrapper>(); // Initialize appropriately
		IEnumerable<IListenerBuilder<OmexStatefulService>> listenerBuilders = new List<IListenerBuilder<OmexStatefulService>>(); // Initialize appropriately
		IEnumerable<IServiceAction<OmexStatefulService>> serviceActions = new List<IServiceAction<OmexStatefulService>>(); // Initialize appropriately

		OmexStatefulServiceRegistrator serviceRegistrator = new OmexStatefulServiceRegistrator(
			options,
			contextAccessor,
			partitionAccessor,
			stateAccessor,
			roleAccessor,
			listenerBuilders,
			serviceActions);

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

		// Initialize the OmexStatefulService
		OmexStatefulService statefulService = new OmexStatefulService(serviceRegistrator, context);

		// Get the current replica role
		ReplicaRole currentRole = statefulService.GetCurrentReplicaRole();
		Console.WriteLine($"Current Replica Role: {currentRole}");
	}
}

public class AccessorSetter<T> : IAccessorSetter<T> where T : class
{
	private T? m_value;
	public void SetValue(T value) => m_value = value;
	public T GetValue() => m_value!;
}
