// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class ServiceAccessorTests
	{
		[TestMethod]
		public void StatelessContextPasedInConstructor_ProperlyHandlesActions() =>
			ContextOnInitialization(ServiceFabric.Mocks.MockStatelessServiceContextFactory.Default);


		[TestMethod]
		public void StatefulContextPasedInConstructor_ProperlyHandlesActions() =>
			ContextOnInitialization(ServiceFabric.Mocks.MockStatefulServiceContextFactory.Default);


		[TestMethod]
		public void StatelessContextAfterInitialization_ProperlyHandlesActions() =>
			ContextAfterInitialization(ServiceFabric.Mocks.MockStatelessServiceContextFactory.Default);


		[TestMethod]
		public void StatefulContextAfterInitialization_ProperlyHandlesActions() =>
			ContextAfterInitialization(ServiceFabric.Mocks.MockStatefulServiceContextFactory.Default);


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
			ServiceContextAccessor<TContext>  accessor = new ServiceContextAccessor<TContext>();
			IServiceContextAccessor<TContext> publicAccessor = accessor;

			TContext? recivedContext = null;
			publicAccessor.OnContextAvailable(c => recivedContext = c);
			accessor.SetContext(context);

			Assert.AreEqual(context, recivedContext);
		}
	}
}
