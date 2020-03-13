// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.Omex.Extensions.Hosting.Services.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks;

namespace Hosting.Services.Web.UnitTests
{
	[TestClass]
	public class KestrelListenerBuilderTests
	{
		[TestMethod]
		public void StatelessService_TypesRegistred() => CheckTypeRegistration(MockStatelessServiceContextFactory.Default);


		[TestMethod]
		public void StatefulService_TypesRegistred() => CheckTypeRegistration(MockStatefulServiceContextFactory.Default);


		private void CheckTypeRegistration<TContext>(TContext context)
			where TContext : ServiceContext
		{
			ListenerValidator<TContext> validator = new ListenerValidator<TContext>();

			KestrelListenerBuilder<MockStartup, TContext> builder =
				new KestrelListenerBuilder<MockStartup, TContext>(
					validator.ListenerName,
					validator.IntegrationOptions,
					validator.BuilderAction);

			validator.ValidateListenerBuilder(context, builder);
			validator.ValidateBuildFunction(context, builder);
		}
	}
}
