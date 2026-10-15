// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
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
		[DataRow("https://localhost:8080/health")]
		[DataRow("https://127.0.0.1:8080/health")]
		[DataRow("https://[::1]:8080/health")]
		[DataRow("https://10.0.0.4:8080/health")]
		[DataRow("https://[fd00::1]:8080/health")]
		public void AddHealthCheckHttpClient_ForLocalOrIpEndpoint_IgnoresCertificateErrors(string url)
		{
			Assert.IsTrue(
				Validate(CertificateCallback(Environments.Production, validated: false), url, SslPolicyErrors.RemoteCertificateNameMismatch),
				FormattableString.Invariant($"Certificate errors should be ignored for local endpoint '{url}'"));
		}

		[TestMethod]
		[DataRow("https://service.contoso.com/health")]
		[DataRow("https://my-service.westus.cloudapp.azure.com/health")]
		public void AddHealthCheckHttpClient_ForDnsEndpoint_ValidatesCertificate(string url)
		{
			Func<HttpRequestMessage, SslPolicyErrors, bool> callback = CertificateCallback(Environments.Production, validated: false);

			Assert.IsFalse(
				Validate(callback, url, SslPolicyErrors.RemoteCertificateNameMismatch),
				FormattableString.Invariant($"Certificate errors should be rejected for DNS endpoint '{url}'"));

			Assert.IsTrue(
				Validate(callback, url, SslPolicyErrors.None),
				FormattableString.Invariant($"A valid certificate should be accepted for DNS endpoint '{url}'"));
		}

		[TestMethod]
		[DataRow("https://service.contoso.com/health")]
		[DataRow("https://localhost:8080/health")]
		public void AddHealthCheckHttpClient_InDevelopment_IgnoresCertificateErrors(string url)
		{
			Assert.IsTrue(
				Validate(CertificateCallback(Environments.Development, validated: false), url, SslPolicyErrors.RemoteCertificateChainErrors),
				FormattableString.Invariant($"Certificate errors should be ignored in Development for '{url}'"));
		}

		[TestMethod]
		[DataRow("Production")]
		[DataRow("Staging")]
		[DataRow("EDog")]
		[DataRow("Int")]
		[DataRow(null)]
		public void AddValidatedHealthCheckHttpClient_OutsideDevelopment_ValidatesLocalEndpoints(string? environmentName)
		{
			Func<HttpRequestMessage, SslPolicyErrors, bool> callback = CertificateCallback(environmentName, validated: true);

			Assert.IsFalse(
				Validate(callback, "https://localhost:8080/health", SslPolicyErrors.RemoteCertificateNameMismatch),
				FormattableString.Invariant($"Local certificate errors should be rejected in '{environmentName ?? "<unregistered>"}'"));

			Assert.IsFalse(
				Validate(callback, "https://10.0.0.4:8080/health", SslPolicyErrors.RemoteCertificateNameMismatch),
				FormattableString.Invariant($"IP certificate errors should be rejected in '{environmentName ?? "<unregistered>"}'"));

			Assert.IsTrue(
				Validate(callback, "https://localhost:8080/health", SslPolicyErrors.None),
				"A valid certificate should always be accepted");
		}

		[TestMethod]
		public void AddValidatedHealthCheckHttpClient_InDevelopment_IgnoresCertificateErrors() =>
			Assert.IsTrue(
				Validate(CertificateCallback(Environments.Development, validated: true), "https://localhost:8080/health", SslPolicyErrors.RemoteCertificateChainErrors),
				"Certificate errors should be ignored in Development");

		private static bool Validate(Func<HttpRequestMessage, SslPolicyErrors, bool> callback, string url, SslPolicyErrors errors)
		{
			using HttpRequestMessage request = new(HttpMethod.Get, url);
			return callback(request, errors);
		}

		private static Func<HttpRequestMessage, SslPolicyErrors, bool> CertificateCallback(string? environmentName, bool validated)
		{
			ServiceCollection services = new();

			if (environmentName != null)
			{
				Mock<IHostEnvironment> environmentMock = new();
				environmentMock.SetupGet(e => e.EnvironmentName).Returns(environmentName);
				services.AddSingleton(environmentMock.Object);
			}

			if (validated)
			{
				services.AddValidatedHealthCheckHttpClient(ClientName);
			}
			else
			{
				services.AddHealthCheckHttpClient(ClientName);
			}

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
			HttpClientHandler handler = (HttpClientHandler)builder.PrimaryHandler;

			NullableAssert.IsNotNull(handler.ServerCertificateCustomValidationCallback, "A validation callback should be configured");

			return (request, errors) => handler.ServerCertificateCustomValidationCallback(request, null, null, errors);
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
