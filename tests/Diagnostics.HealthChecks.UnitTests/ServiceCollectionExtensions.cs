// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class ServiceCollectionExtensions
	{
		[TestMethod]
		public void AddHealthCheckHttpClient_UsesDefaultCertificateValidation()
		{
			const string clientName = "HealthCheck";
			ServiceCollection services = new();

			IHttpClientBuilder builder = services.AddHealthCheckHttpClient(clientName);
			using ServiceProvider provider = services.BuildServiceProvider();

			Assert.AreEqual(clientName, builder.Name);
			HttpClientHandler handler = GetPrimaryHttpClientHandler(provider, clientName);
			Assert.IsNull(handler.ServerCertificateCustomValidationCallback,
				"Health-check clients must use platform certificate validation in every environment.");
		}

		[TestMethod]
		public void AddServiceFabricHealthChecks_UsesDefaultCertificateValidationAndPreservesClientSettings()
		{
			using ServiceProvider provider = new ServiceCollection()
				.AddServiceFabricHealthChecks()
				.Services
				.BuildServiceProvider();

			HttpClientHandler handler = GetPrimaryHttpClientHandler(provider, HealthCheckConstants.HttpClientLogicalName);

			Assert.IsNull(handler.ServerCertificateCustomValidationCallback,
				"The legacy health-check client must use platform certificate validation.");
			Assert.IsFalse(handler.AllowAutoRedirect);
			Assert.AreSame(CredentialCache.DefaultCredentials, handler.Credentials);
		}

		private static HttpClientHandler GetPrimaryHttpClientHandler(IServiceProvider provider, string clientName)
		{
			HttpMessageHandler handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler(clientName);
			while (handler is DelegatingHandler delegatingHandler)
			{
				NullableAssert.IsNotNull(delegatingHandler.InnerHandler);
				handler = delegatingHandler.InnerHandler;
			}

			Assert.IsInstanceOfType(handler, typeof(HttpClientHandler));
			return (HttpClientHandler)handler;
		}

		[TestMethod]
		public void AddHealthCheckPublisher_RegisterPublisherWithoutOverride()
		{
			IServiceProvider provider = new ServiceCollection()
				.AddHealthCheckPublisher<MockPublisher>()
				.AddHealthCheckPublisher<MockPublisher>()
				.BuildServiceProvider();

			IHealthCheckPublisher[] publishers = provider
				.GetRequiredService<IEnumerable<IHealthCheckPublisher>>()
				.ToArray();

			Assert.HasCount(1, publishers, "Published should be registered once");
			Assert.IsInstanceOfType(publishers[0], typeof(MockPublisher));
		}

		[TestMethod]
		[Obsolete("The health check implementation based on AbstractHealthCheck is obsolete.")]
		public void AddServiceFabricHealthChecks_RegisterPublisherAndChecks()
		{
			string checkName = "MockCheck";
			IHealthCheck check = new MockCheck();

			IServiceProvider provider = new ServiceCollection()
				.AddSingleton(new Mock<IAccessor<IServicePartition>>().Object)
				.AddServiceFabricHealthChecks()
				.AddCheck(checkName, check)
				.Services
				.BuildServiceProvider();

			IHealthCheckPublisher[] publishers = provider
				.GetRequiredService<IEnumerable<IHealthCheckPublisher>>()
				.ToArray();

			Assert.IsTrue(
				publishers.Any(p => p is OmexHealthCheckPublisher),
				FormattableString.Invariant($"{nameof(OmexHealthCheckPublisher)} publisher should be registered"));

			IOptions<HealthCheckServiceOptions> options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
			HealthCheckRegistration? registration = options.Value.Registrations.SingleOrDefault(r => string.Equals(checkName, r.Name, StringComparison.Ordinal));

			NullableAssert.IsNotNull(registration, "HealthCheck should be registered");

			Assert.AreEqual(check, registration.Factory(provider));
		}

		public class MockPublisher : IHealthCheckPublisher
		{
			public Task PublishAsync(HealthReport report, CancellationToken cancellationToken) => Task.CompletedTask;
		}

		public class MockCheck : IHealthCheck
		{
			public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken) =>
				await Task.FromResult(HealthCheckResult.Healthy());
		}
	}
}
