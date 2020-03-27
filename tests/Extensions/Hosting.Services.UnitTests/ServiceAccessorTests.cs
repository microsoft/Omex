// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class ServiceAccessorTests
	{
		[TestMethod]
		public void StatelessContextPasedInConstructor_ProperlyHandlesActions() =>
			ContextOnInitialization(MockStatelessServiceContextFactory.Default);

		[TestMethod]
		public void StatefulContextPasedInConstructor_ProperlyHandlesActions() =>
			ContextOnInitialization(MockStatefulServiceContextFactory.Default);

		[TestMethod]
		public void StatelessContextAfterInitialization_ProperlyHandlesActions() =>
			ContextAfterInitialization(MockStatelessServiceContextFactory.Default);

		[TestMethod]
		public void StatefulContextAfterInitialization_ProperlyHandlesActions() =>
			ContextAfterInitialization(MockStatefulServiceContextFactory.Default);

		private void ContextOnInitialization<TContext>(TContext context)
			where TContext : ServiceContext
		{
			ServiceContextAccessor<TContext> accessor = new ServiceContextAccessor<TContext>(context);
			IServiceContextAccessor<TContext> publicAccessor = accessor;

			Assert.AreEqual(context, publicAccessor.ServiceContext);
			TContext? recivedContext = null;
			publicAccessor.OnContextAvailable(c => recivedContext = c);

			Assert.AreEqual(context, recivedContext);
		}

		private void ContextAfterInitialization<TContext>(TContext context)
			where TContext : ServiceContext
		{
			ServiceContextAccessor<TContext> accessor = new ServiceContextAccessor<TContext>();
			IServiceContextAccessor<TContext> publicAccessor = accessor;

			TContext? receivedContext = null;
			publicAccessor.OnContextAvailable(c => receivedContext = c);
			accessor.SetContext(context);

			Assert.AreEqual(context, receivedContext);
		}
	}
}
