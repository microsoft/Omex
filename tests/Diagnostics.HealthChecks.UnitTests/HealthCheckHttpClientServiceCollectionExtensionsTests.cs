// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class HealthCheckHttpClientServiceCollectionExtensionsTests
	{
		private const string ClientName = "TestHealthCheckClient";

		[TestMethod]
		public void AddHealthCheckHttpClient_InDevelopment_IgnoresCertificateErrors()
		{
			HttpClientHandler handler = BuildPrimaryHandler(Environments.Development);

			NullableAssert.IsNotNull(
				handler.ServerCertificateCustomValidationCallback,
				"Certificate validation should be bypassed in Development");
		}

		[TestMethod]
		[DataRow("Production")]
		[DataRow("Staging")]
		[DataRow("EDog")]
		[DataRow("Int")]
		[DataRow(null)]
		public void AddHealthCheckHttpClient_OutsideDevelopment_ValidatesCertificates(string? environmentName)
		{
			HttpClientHandler handler = BuildPrimaryHandler(environmentName);

			Assert.IsNull(
				handler.ServerCertificateCustomValidationCallback,
				FormattableString.Invariant($"Certificate validation should not be bypassed in '{environmentName ?? "<unregistered>"}'"));
		}

		private static HttpClientHandler BuildPrimaryHandler(string? environmentName)
		{
			ServiceCollection services = new();

			if (environmentName != null)
			{
				Mock<IHostEnvironment> environmentMock = new();
				environmentMock.SetupGet(e => e.EnvironmentName).Returns(environmentName);
				services.AddSingleton(environmentMock.Object);
			}

			services.AddHealthCheckHttpClient(ClientName);

			IServiceProvider provider = services.BuildServiceProvider();

			HttpClientFactoryOptions options = provider
				.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>()
				.Get(ClientName);

			TestHttpMessageHandlerBuilder builder = new(ClientName, provider);
			foreach (Action<HttpMessageHandlerBuilder> action in options.HttpMessageHandlerBuilderActions)
			{
				action(builder);
			}

			Assert.IsInstanceOfType(builder.PrimaryHandler, typeof(HttpClientHandler));
			return (HttpClientHandler)builder.PrimaryHandler;
		}

		private sealed class TestHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
		{
			public TestHttpMessageHandlerBuilder(string name, IServiceProvider services)
			{
				Name = name;
				Services = services;
			}

			public override string? Name { get; set; }

			public override HttpMessageHandler PrimaryHandler { get; set; } = new HttpClientHandler();

			public override IList<DelegatingHandler> AdditionalHandlers { get; } = new List<DelegatingHandler>();

			public override IServiceProvider Services { get; }

			public override HttpMessageHandler Build() => PrimaryHandler;
		}
	}
}
