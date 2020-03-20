// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Omex.Extensions.Hosting.Services.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class KestrelListenerBuilderTests
	{
		[TestMethod]
		public void StatelessService_TypesRegistered() => CheckTypeRegistration(new MockStatelessService(), service => service.Context);

		[TestMethod]
		public void StatefulService_TypesRegistered() => CheckTypeRegistration(new MockStatefulService(), service => service.Context);

		private void CheckTypeRegistration<TService, TContext>(TService service, Func<TService,TContext> getContex)
			where TContext : ServiceContext
		{
			ListenerValidator<TService, TContext> validator = new ListenerValidator<TService, TContext>();

			KestrelListenerBuilder<MockStartup, TService, TContext> builder =
				new KestrelListenerBuilder<MockStartup, TService, TContext>(
					validator.ListenerName,
					validator.IntegrationOptions,
					getContex,
					validator.BuilderAction);

			validator.ValidateListenerBuilder(getContex(service), builder);
			validator.ValidateBuildFunction(service, builder);
		}
	}
}
