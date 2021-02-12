// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class ServiceFabricExecutionContextTests
	{
		[TestMethod]
		public void Constructor_InitializesPropertiesProperly()
		{
			ServiceContext context = MockStatelessServiceContextFactory.Default;
			Accessor<ServiceContext> accessor = new Accessor<ServiceContext>();
			((IAccessorSetter<ServiceContext>)accessor).SetValue(context);

			IExecutionContext info = new ServiceFabricExecutionContext(new Mock<IHostEnvironment>().Object, accessor);

			Assert.AreEqual(context.CodePackageActivationContext.CodePackageVersion, info.BuildVersion);
		}
	}
}
