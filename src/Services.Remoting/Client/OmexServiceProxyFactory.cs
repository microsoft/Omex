// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Microsoft.Omex.Extensions.Services.Remoting.Client
{
	// OLD:
	// SF infra configuration
			// CrossCuttingConcerns.InstanceContainer.RegisterSingleton<IServiceProxyFactory>(() =>
			// 	new ServiceProxyFactory(handler =>
			// 		new OmexServiceRemotingClientFactory(
			// 			new FabricTransportServiceRemotingClientFactory(
			// 				remotingCallbackMessageHandler: handler,
			// 				serializationProvider: new ServiceRemotingJsonSerializationProvider()))));

			// CrossCuttingConcerns.InstanceContainer.RegisterType<IHealthWebService>(() =>
			// 	CrossCuttingConcerns.InstanceContainer.Resolve<IServiceProxyFactory>().CreateServiceProxy<IHealthWebService>(
			// 		new Uri(SettingDefinitions.StoreCatalogFeedServiceAddress.GetValue()),
			// 		listenerName: SettingDefinitions.StoreCatalogFeedServiceTipRemotingListenerName.GetValue()));

	// Current:
	// services.AddTransient(provider =>
	// 		OmexServiceProxyFactory.Instance.CreateServiceProxy<IEcsStorageService>(
	// 			new Uri(provider.GetRequiredService<IOptions<EcsStorageServiceSettings>>().Value.EcsStorageServiceRemotingUri),
	// 			new ServicePartitionKey(0),
	// 			listenerName: provider.GetRequiredService<IOptions<EcsStorageServiceSettings>>().Value.EcsStorageServiceRemotingListenerName));


	// Ideal state - something like:
	// services.AddScoped<IServiceFactory, ServiceFactory>();
    //     services.AddScoped<ServiceA>()
    //         .AddScoped<IService, ServiceA>(c => c.GetService<ServiceA>());
    //     services.AddScoped<ServiceB>()
    //         .AddScoped<IService, ServiceB>(c => c.GetService<ServiceB>());

	// TODO: usage should end up something like:
	// services.AdScoped<IServiceRemotingClientFactory, OmexServiceRemotingClientFactory>();
	// services.AdScoped<ServiceProxyFactory>()
	// services.AddScoped<NamedService>

	/// <summary>
	/// Class to provide ServiceProxyFactory
	/// </summary>
	public class OmexServiceProxyFactory
	{
		private Dictionary<string, IServiceProxyFactory> m_proxyFactories = new Dictionary<string, IServiceProxyFactory>();

		public void AddSecureRemotingFactory(string name, FabricTransportRemotingSettings trnasportSettings)
		{
			if(m_proxyFactories.ContainsKey(name))
			{
				throw new InvalidOperationException($"A remoting factory with name '{name}' already exists.");
			}

			m_proxyFactories.Add(name)
		}

		private static ServiceProxyFactory s_insecureProxyFactory = new ServiceProxyFactory(handler =>
			new OmexServiceRemotingClientFactory(
				new FabricTransportServiceRemotingClientFactory(
					remotingCallbackMessageHandler: handler)));

		/// <summary>
		/// Binds transport settings to the service proxy factory, for use in secure remoting communication.
		/// </summary>
		// public static void WithTransportSettings(FabricTransportRemotingSettings transportSettings)
		// {
		// 	s_serviceProxyFactory = new ServiceProxyFactory(handler =>
		// 		new OmexServiceRemotingClientFactory(
		// 			new FabricTransportServiceRemotingClientFactory(
		// 				remotingSettings: transportSettings,
		// 				remotingCallbackMessageHandler: handler)));
		// }

		/// <summary>
		/// Instance of the ServiceProxyFactory
		/// </summary>
		[Obsolete("Static resolution causes issues with service partition lock-in and prevents usage of secure remoting. Support for this will be dropped. Migrate to using DI as soon as possible.")]
		public static ServiceProxyFactory Instance => s_insecureProxyFactory;
	}
}
