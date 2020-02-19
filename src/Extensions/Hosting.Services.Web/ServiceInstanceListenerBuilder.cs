//// Copyright (c) Microsoft Corporation. All rights reserved.
//// Licensed under the MIT license.

//using System;
//using System.Fabric;
//using System.Fabric.Description;
//using System.IO;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
//using Microsoft.ServiceFabric.Services.Communication.Runtime;

//namespace Microsoft.Omex.Extensions.ServiceFabric.Services
//{
//	/// <summary>
//	/// Creates ServiceInstanceListener with all of Omex dependencies inicialized
//	/// </summary>
//	public class ServiceInstanceListenerBuilder
//	{
//		internal ServiceInstanceListenerBuilder(
//			DependenciesConfiguration dependenciesConfiguration,
//			Type startupType,
//			string name,
//			ServiceFabricIntegrationOptions options)
//		{
//			m_dependenciesConfiguration = dependenciesConfiguration;
//			m_startupType = startupType;
//			m_name = name;
//			m_options = options;
//		}


//		internal ServiceInstanceListener Build() => new ServiceInstanceListener(CreateListener, m_name);


//		private readonly Type m_startupType;
//		private readonly string m_name;
//		private readonly ServiceFabricIntegrationOptions m_options;
//		private readonly DependenciesConfiguration m_dependenciesConfiguration;


//		private void ConfigureServices(StatelessServiceContext collection, IServiceCollection services)
//		{
//			services.AddSingleton<ServiceContext>(collection);
//			services.AddSingleton<StatelessServiceContext>(collection);
//			m_dependenciesConfiguration.ConfigureServices(services);
//		}


//		private IWebHost WebHostBuild(StatelessServiceContext context, string url, AspNetCoreCommunicationListener listener)
//		{
//			EndpointResourceDescription endpointConfig = context.CodePackageActivationContext.GetEndpoint(m_name);
//			url += endpointConfig.PathSuffix;

//			IWebHost webHost = new WebHostBuilder()
//				.UseKestrel()
//				.ConfigureServices(collection => ConfigureServices(context, collection))
//				.UseContentRoot(Directory.GetCurrentDirectory())
//				.UseStartup(m_startupType)
//				.UseServiceFabricIntegration(listener, m_options)
//				.UseUrls(url)
//#if !NETFRAMEWORK
//				.UseDefaultServiceProvider(config =>
//				{
//					config.ValidateOnBuild = true;
//					config.ValidateScopes = true;
//				})
//#endif
//				.Build();

//			m_dependenciesConfiguration.SetServices(webHost.Services);

//			return webHost;
//		}


//		private ICommunicationListener CreateListener(StatelessServiceContext context) =>
//			new KestrelCommunicationListener(context, m_name, (url, listener) => WebHostBuild(context, url, listener));
//	}
//}
