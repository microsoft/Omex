// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Scrubbing;
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
				.AddOmexServiceContext<MockServiceContext>()
				.AddOmexLogging(null!);

			IServiceContext context = ValidateTypeRegistration<IServiceContext>(collection);

			Assert.IsInstanceOfType(context,
				typeof(MockServiceContext),
				"Call of AddOmexServiceContext before AddOmexLogging should override IServiceCollection implementation");
		}

		[TestMethod]
		public void AddOmexLoggerOnServiceCollection_RegistersLogger()
		{
			IServiceCollection collection = new ServiceCollection().AddOmexLogging(null!);
			ValidateTypeRegistration<ILogger<ServiceCollectionTests>>(collection);
		}

		[TestMethod]
		public void AddOmexLoggerOnLogBuilder_RegistersLogger()
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddOmexLogging(null!);
			ValidateTypeRegistration<ILogger<ServiceCollectionTests>>(builder.Services);
		}

		private T ValidateTypeRegistration<T>(IServiceCollection collection)
			where T : class
		{
			T obj = collection
				.AddOmexLogging(null!)
				.BuildServiceProvider(new ServiceProviderOptions
				{
					ValidateOnBuild = true,
					ValidateScopes = true
				}).GetRequiredService<T>();

			Assert.IsNotNull(obj);

			return obj;
		}

		[TestMethod]
		public void AddOmexLoggerOnLogBuilder_ContainHostBuilder_SettingOmexLoggingEnabled_False_OmexLoggerNotRegistered()
		{
			HostBuilderContext hostBuilderContext = new(new Dictionary<object, object>());
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					{ "Monitoring:OmexLoggingEnabled", "false"},
				})
				.Build();

			hostBuilderContext.Configuration = configuration;
			IServiceCollection collection = new ServiceCollection()
				.AddOmexServiceContext<MockServiceContext>()
				.AddOmexLogging(hostBuilderContext);

			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				OmexLogEventSource logEventSource = collection
					.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true })
					.GetRequiredService<OmexLogEventSource>();
			});

			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				ILogEventSender eventSender = collection
					.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true })
					.GetRequiredService<ILogEventSender>();
			});

			Assert.ThrowsException<InvalidOperationException>(() =>
			{
				ILoggerProvider omexLoggerProvider = collection
					.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true })
					.GetRequiredService<ILoggerProvider>();
			});
		}

		[TestMethod]
		[DataTestMethod]
		[DataRow(true)]
		[DataRow(null!)]
		public void AddOmexLoggerOnLogBuilder_ContainHostBuilder_SettingOmexLoggingEnabled_TrueOrMissing_OmexLoggerRegistered(bool? isEnabled)
		{
			HostBuilderContext hostBuilderContext = new(new Dictionary<object, object>());
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(isEnabled == null ? new Dictionary<string, string?>() : new Dictionary<string, string?>
				{
					{ "Monitoring:OmexLoggingEnabled", isEnabled.ToString()},
				})
				.Build();

			hostBuilderContext.Configuration = configuration;
			IServiceCollection collection = new ServiceCollection()
				.AddOmexServiceContext<MockServiceContext>()
				.AddOmexLogging(hostBuilderContext);

			OmexLogEventSource logEventSource = collection
				.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true })
				.GetRequiredService<OmexLogEventSource>();

			ILogEventSender eventSender = collection
				.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true })
				.GetRequiredService<ILogEventSender>();

			ILoggerProvider omexLoggerProvider = collection
				.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true })
				.GetRequiredService<ILoggerProvider>();

			Assert.IsInstanceOfType<OmexLogEventSource>(logEventSource);
			Assert.IsInstanceOfType<ILogEventSender>(eventSender);
			Assert.IsInstanceOfType<OmexLoggerProvider>(omexLoggerProvider);
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
