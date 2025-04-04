using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;

namespace ReplicaTestApp
{
	/// <summary>
	/// The FabricRuntime creates an instance of this class for each service type instance.
	/// </summary>
	internal sealed class ReplicaTestApp : StatefulService
	{
		public ReplicaTestApp(StatefulServiceContext context)
			: base(context)
		{ }

		/// <summary>
		/// Optional override to create listeners (like tcp, http) for this service instance.
		/// </summary>
		/// <returns>The collection of listeners.</returns>
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
		{
			return new ServiceReplicaListener[]
			{
					new ServiceReplicaListener(serviceContext =>
						new KestrelCommunicationListener(serviceContext, (url, listener) =>
						{
							ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

										WebApplicationBuilder builder = WebApplication.CreateBuilder();

							builder.Services
										.AddSingleton(serviceContext)
										.AddSingleton<IReliableStateManager>(StateManager);
							builder.WebHost
										.UseKestrel()
										.UseContentRoot(Directory.GetCurrentDirectory())
										.UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
										.UseUrls(url);
							builder.Services.AddControllers();
							builder.Services.AddEndpointsApiExplorer();
							WebApplication app = builder.Build();
							app.UseAuthorization();
							app.MapControllers();

							return app;

						}))
			};
		}
	}
}
