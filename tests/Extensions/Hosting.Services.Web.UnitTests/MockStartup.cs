// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hosting.Services.Web.UnitTests
{
	internal class MockStartup
	{
		public MockStartup() { }


		public void ConfigureServices(IServiceCollection services) =>
			services.AddTransient<TypeRegistredInStartup>();


		public void Configure(IApplicationBuilder app, IHostEnvironment env)
		{
		}


		internal class TypeRegistredInStartup { }
	}
}
