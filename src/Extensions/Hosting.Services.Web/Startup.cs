// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.ServiceFabric.Services
{
	/// <summary>
	/// Startup class for ASP.NET Core service
	/// </summary>
	internal class OmexStartup
	{
		/// <summary>
		/// Constructor for Startup
		/// </summary>
		/// <param name="configuration">Configuration is passed by ASP.NET Core runtime</param>
		public OmexStartup(IConfiguration configuration) => Configuration = configuration;


		/// <summary>
		/// Key-value configuration properties
		/// </summary>
		public IConfiguration Configuration { get; }


		/// <summary>
		/// This method gets called by the runtime. Use this method to add services to the container.
		/// </summary>
		/// <param name="services">Services collection, DI container</param>
		public void ConfigureServices(IServiceCollection services)
		{
		}


		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app">Middleware application builder, passed by ASP.NET Core runtime</param>
		/// <param name="env">Hosting environment</param>
		public void Configure(IApplicationBuilder app, IHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpContextWrapper();
			app.UseStartEndRequest();
			app.AddUlsLogging();

			app.UseDefaultExceptionHandler();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/healthz");
			});
		}
	}
}
