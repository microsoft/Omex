// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Hosting.Services.Web;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class HostBuilderExtensionsTests
	{
		[TestMethod]
		public void BuildStatelessService_TypesRegisteredStateless() =>
			CheckTypeRegistration(
				new MockStatelessService(),
				service => service.Context,
				(v, h) => h.BuildStatelessService(
					"StatelessServiceName",
					b => b.AddKestrelListener<MockStartup>(v.ListenerName, v.IntegrationOptions, v.BuilderAction)));

		[TestMethod]
		public void BuildStateful_BuildStatelessService_TypesRegistered() =>
			CheckTypeRegistration(
				new MockStatefulService(),
				service => service.Context,
				(v, h) => h.BuildStatefulService(
					"StatefulServiceName",
					b => b.AddKestrelListener<MockStartup>(v.ListenerName, v.IntegrationOptions, v.BuilderAction)));

		private void CheckTypeRegistration<TService,TContext>(
			TService service,
			Func<TService,TContext> getContext,
			Func<ListenerValidator<TService, TContext>, IHostBuilder, IHost> buildAction)
			where TContext : ServiceContext
		{
			ListenerValidator<TService, TContext> validator = new ListenerValidator<TService, TContext>();

			IHostBuilder hostBuilder = new HostBuilder()
				.UseDefaultServiceProvider(options =>
				{
					options.ValidateOnBuild = true;
					options.ValidateScopes = true;
				});

			IHost host = buildAction(validator, hostBuilder);
			IListenerBuilder<TService> builder = host.Services.GetService<IListenerBuilder<TService>>();

			Assert.IsNotNull(builder);

			KestrelListenerBuilder<MockStartup, TService, TContext> kestrelBuilder
				= (KestrelListenerBuilder<MockStartup, TService, TContext>)builder;

			IWebHost webHost = validator.ValidateListenerBuilder(getContext(service), kestrelBuilder);
			validator.ValidateOmexTypesRegistered(webHost);
			validator.ValidateBuildFunction(service, kestrelBuilder);
		}
	}
}
