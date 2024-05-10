// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class ServiceCollectionTests
	{
		[TestMethod]
		public void AddOmexServiceContext_RegisterServiceContext()
		{
			IServiceCollection collection = new ServiceCollection()
				.AddOmexServiceContext<MockServiceContext>();

			ValidateTypeRegistration<IServiceContext>(collection);
		}

		[TestMethod]
		public void AddOmexServiceContext_OverridesContextType()
		{
			IServiceCollection collection = new ServiceCollection()
				.AddOmexServiceContext<MockServiceContext>();

#pragma warning disable CS0618 // AddOmexLogging method is obsolete. Code: 8913598
			collection.AddOmexLogging();
#pragma warning restore CS0618 // AddOmexLogging method is obsolete. Code: 8913598

			IServiceContext context = ValidateTypeRegistration<IServiceContext>(collection);

			Assert.IsInstanceOfType(context,
				typeof(MockServiceContext),
				"Call of AddOmexServiceContext before AddOmexLogging should override IServiceCollection implementation");
		}

		[TestMethod]
		[Obsolete("AddOmexLogging method is obsolete. Code: 8913598")]
		public void AddOmexLoggerOnServiceCollection_RegistersLogger()
		{
			IServiceCollection collection = new ServiceCollection().AddOmexLogging();
			ValidateTypeRegistration<ILogger<ServiceCollectionTests>>(collection);
		}

		[TestMethod]
		[Obsolete("AddOmexLogging method is obsolete. Code: 8913598")]
		public void AddOmexLoggerOnLogBuilder_RegistersLogger()
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddOmexLogging();
			ValidateTypeRegistration<ILogger<ServiceCollectionTests>>(builder.Services);
		}

		private T ValidateTypeRegistration<T>(IServiceCollection collection)
			where T : class
		{
#pragma warning disable CS0618 // AddOmexLogging method is obsolete. Code: 8913598
			T obj = collection
				.AddOmexLogging()
#pragma warning restore CS0618 // AddOmexLogging method is obsolete. Code: 8913598
				.BuildServiceProvider(new ServiceProviderOptions
				{
					ValidateOnBuild = true,
					ValidateScopes = true
				}).GetRequiredService<T>();

			Assert.IsNotNull(obj);

			return obj;
		}

		private class MockLoggingBuilder : ILoggingBuilder
		{
			public IServiceCollection Services { get; } = new ServiceCollection();
		}

		private class MockServiceContext : IServiceContext
		{
			public Guid PartitionId => Guid.Empty;

			public long ReplicaOrInstanceId => 0;
		}
	}
}
