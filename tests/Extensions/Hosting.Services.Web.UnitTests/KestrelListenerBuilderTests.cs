// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Hosting.Services.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;

namespace Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class KestrelListenerBuilderTests
	{
		[TestMethod]
		public void StatelessService_TypesRegistered() =>
			CheckTypeRegistration<OmexStatelessService, StatelessServiceContext>(MockStatelessServiceContextFactory.Default);

		[TestMethod]
		public void StatefulService_TypesRegistered() =>
			CheckTypeRegistration<OmexStatefulService, StatefulServiceContext>(MockStatefulServiceContextFactory.Default);

		private void CheckTypeRegistration<TService, TContext>(TContext context)
			where TService : class, IServiceFabricService<TContext>
			where TContext : ServiceContext
		{
			ListenerValidator<TService, TContext> validator = new ListenerValidator<TService, TContext>();

			KestrelListenerBuilder<MockStartup, TService, TContext> builder =
				new KestrelListenerBuilder<MockStartup, TService, TContext>(
					validator.ListenerName,
					new Mock<IServiceProvider>().Object,
					validator.IntegrationOptions,
					validator.BuilderAction);

			validator.ValidateListenerBuilder(context, builder);
			validator.ValidateBuildFunction(context, builder);
		}
	}
}
