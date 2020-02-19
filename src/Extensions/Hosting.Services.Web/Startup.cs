//// Copyright (c) Microsoft Corporation. All rights reserved.
//// Licensed under the MIT license.

//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//namespace Microsoft.Omex.Extensions.ServiceFabric.Services
//{
//	internal class Startup
//	{
//		public Startup(IConfiguration configuration) => Configuration = configuration;


//		public IConfiguration Configuration { get; }


//		public void ConfigureServices(IServiceCollection services)
//		{
//		}


//		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//		{
//			if (env.IsDevelopment())
//			{
//				app.UseDeveloperExceptionPage();
//			}

//			app.UseHttpContextWrapper();
//			app.UseStartEndRequest();
//			app.AddUlsLogging();
//			app.UseMiddleware<ExceptionHandler>();
//			app.UseMiddleware<PerformanceOptimization>();

//			app.UseRouting();

//			app.UseAuthentication();
//			app.UseAuthorization();

//			app.UseEndpoints(endpoints =>
//			{
//				endpoints.MapHealthChecks("/healthz");
//				endpoints.MapControllers();
//			});
//		}
//	}
//}
