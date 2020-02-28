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
		public void CheckTypeRegistrationForStateless() => CheckTypeRegistration(MockStatelessServiceContextFactory.Default);


		[TestMethod]
		public void CheckTypeRegistrationForStateful() => CheckTypeRegistration(MockStatefulServiceContextFactory.Default);


		private void CheckTypeRegistration<TContext>(TContext context)
			where TContext : ServiceContext
		{
			ListenerValidator validator = new ListenerValidator();

			KestrelListenerBuilder<ListenerValidator.Startup, TContext> builder =
				new KestrelListenerBuilder<ListenerValidator.Startup, TContext>(
					validator.ListenerName,
					validator.IntegrationOptions,
					validator.BuilderAction);

			validator.ValidateListenerBuilder(context, builder);
			validator.ValidateBuildFunction(context, builder);
		}
	}
}
