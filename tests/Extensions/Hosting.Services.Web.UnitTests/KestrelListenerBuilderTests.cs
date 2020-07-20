// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Hosting.Services.UnitTests;
using Microsoft.Omex.Extensions.Hosting.Services.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hosting.Services.Web.UnitTests;
using Moq;

namespace Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class KestrelListenerBuilderTests
	{
		[TestMethod]
		public void StatelessService_TypesRegistered() =>
			CheckTypeRegistration<OmexStatelessService, StatelessServiceContext>(MockServiceFabricServices.MockOmexStatelessService);

		[TestMethod]
		public void StatefulService_TypesRegistered() =>
			CheckTypeRegistration<OmexStatefulService, StatefulServiceContext>(MockServiceFabricServices.MockOmexStatefulService);

		private void CheckTypeRegistration<TService, TContext>(TService service)
			where TService : IServiceFabricService<TContext>
			where TContext : ServiceContext
		{
			ListenerValidator<TService, TContext> validator = new ListenerValidator<TService, TContext>();
			ServiceProvider serviceProvider = new ServiceCollection()
				.AddOmexServiceFabricDependencies<TContext>()
				.BuildServiceProvider();

			KestrelListenerBuilder<MockStartup, TService, TContext> builder =
				new KestrelListenerBuilder<MockStartup, TService, TContext>(
					validator.ListenerName,
					serviceProvider,
					validator.IntegrationOptions,
					validator.BuilderAction,
					validator.KestrelOptionsAction);

			IWebHost host = validator.ValidateListenerBuilder(service.Context, builder);
			validator.ValidateBuildFunction(service, builder);
			validator.ValidateKestrelServerOptionsSet(host);
		}
	}
}
