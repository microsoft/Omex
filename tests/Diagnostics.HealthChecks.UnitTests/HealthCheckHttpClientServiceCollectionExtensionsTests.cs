// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
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
		public void AddHealthCheckHttpClient_ForLoopbackEndpoint_IgnoresCertificateErrors(string url)
		{
			Assert.IsTrue(
				Validate(CertificateCallback(Environments.Production, validated: false), url, SslPolicyErrors.RemoteCertificateNameMismatch),
				FormattableString.Invariant($"Certificate errors should be ignored for local endpoint '{url}'"));
		}

		[TestMethod]
		[DataRow("https://service.contoso.com/health")]
		[DataRow("https://my-service.westus.cloudapp.azure.com/health")]
		[DataRow("https://192.0.2.1:8080/health")]
		[DataRow("https://[2001:db8::1]:8080/health")]
		[DataRow("https://10.0.0.4:8080/health")]
		[DataRow("https://[fd00::1]:8080/health")]
		public void AddHealthCheckHttpClient_ForNonLoopbackEndpoint_ValidatesUnlessAssignedLocally(string url)
		{
			bool isAssignedLocally = IPAddress.TryParse(new Uri(url).DnsSafeHost, out IPAddress? address)
				&& LocalAddresses().Contains(address);
			Func<HttpRequestMessage, SslPolicyErrors, bool> callback = CertificateCallback(Environments.Production, validated: false);

			Assert.AreEqual(
				isAssignedLocally,
				Validate(callback, url, SslPolicyErrors.RemoteCertificateNameMismatch),
				FormattableString.Invariant($"Only an assigned local IP should bypass certificate validation for '{url}'"));

			Assert.IsTrue(
				Validate(callback, url, SslPolicyErrors.None),
				FormattableString.Invariant($"A valid certificate should be accepted for remote endpoint '{url}'"));
		}

		[TestMethod]
		[DataRow(null)]
		[DataRow(true)]
		[DataRow(false)]
		public void HealthCheckHttpClient_ForAssignedLocalIp_IgnoresCertificateErrors(bool? serviceFabricPublisher)
		{
			Func<HttpRequestMessage, SslPolicyErrors, bool> callback =
				CertificateCallback(Environments.Production, validated: false, serviceFabricPublisher);
			Func<HttpRequestMessage, SslPolicyErrors, bool> validatedCallback =
				CertificateCallback(Environments.Production, validated: true);
			IPAddress[] addresses = LocalAddresses();
			Assert.IsNotEmpty(addresses, "The test machine should have an assigned address");

			foreach (IPAddress address in addresses)
			{
				string url = new UriBuilder(Uri.UriSchemeHttps, address.ToString(), 8080, "health").Uri.OriginalString;
				Assert.IsTrue(Validate(callback, url, SslPolicyErrors.RemoteCertificateChainErrors),
					FormattableString.Invariant($"Local node IP '{address}' should retain the certificate exception"));
				Assert.IsFalse(Validate(validatedCallback, url, SslPolicyErrors.RemoteCertificateChainErrors),
					"The validated client should not relax validation for local node IPs");
			}
		}

		[TestMethod]
		[DataRow(null, "Production")]
		[DataRow(true, "Production")]
		[DataRow(false, "Production")]
		[DataRow(null, "Staging")]
		[DataRow(true, "Staging")]
		[DataRow(false, "Staging")]
		[DataRow(null, "EDog")]
		[DataRow(true, "EDog")]
		[DataRow(false, "EDog")]
		[DataRow(null, "Int")]
		[DataRow(true, "Int")]
		[DataRow(false, "Int")]
		[DataRow(null, null)]
		[DataRow(true, null)]
		[DataRow(false, null)]
		public void HealthCheckClients_OutsideDevelopment_ApplyLocalCertificatePolicyWithoutCredentials(bool? serviceFabricPublisher, string? environmentName)
		{
			Func<HttpRequestMessage, SslPolicyErrors, bool> callback =
				CertificateCallback(environmentName, validated: false, serviceFabricPublisher);

			for (int errorFlags = 0; errorFlags <= 7; errorFlags++)
			{
				SslPolicyErrors errors = (SslPolicyErrors)errorFlags;
				Assert.IsTrue(Validate(callback, "https://localhost:8080/health", errors));
				Assert.IsTrue(Validate(callback, "https://127.0.0.1:8080/health", errors));
				Assert.IsTrue(Validate(callback, "https://[::1]:8080/health", errors));
				Assert.AreEqual(errors == SslPolicyErrors.None, Validate(callback, "https://service.contoso.com/health", errors));
				Assert.AreEqual(errors == SslPolicyErrors.None, Validate(callback, "https://192.0.2.1/health", errors));
				Assert.AreEqual(errors == SslPolicyErrors.None, Validate(callback, "https://[2001:db8::1]/health", errors));
				using HttpRequestMessage requestWithoutUri = new();
				Assert.AreEqual(errors == SslPolicyErrors.None, callback(requestWithoutUri, errors));
			}
		}

		[TestMethod]
		[DataRow(null)]
		[DataRow(true)]
		[DataRow(false)]
		public void HealthCheckClients_InDevelopment_PreserveCertificateExceptionWithoutCredentials(bool? serviceFabricPublisher) =>
			Assert.IsTrue(Validate(
				CertificateCallback(Environments.Development, validated: false, serviceFabricPublisher),
				"https://service.contoso.com/health", SslPolicyErrors.RemoteCertificateChainErrors));

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

		private static IPAddress[] LocalAddresses() =>
			NetworkInterface.GetAllNetworkInterfaces()
				.SelectMany(networkInterface => networkInterface.GetIPProperties().UnicastAddresses)
				.Select(unicastAddress => unicastAddress.Address)
				.ToArray();

		private static Func<HttpRequestMessage, SslPolicyErrors, bool> CertificateCallback(string? environmentName, bool validated, bool? serviceFabricPublisher = null)
		{
			ServiceCollection services = new();
			string clientName = serviceFabricPublisher.HasValue ? HealthCheckConstants.HttpClientLogicalName : ClientName;

			if (environmentName != null)
			{
				Mock<IHostEnvironment> environmentMock = new();
				environmentMock.SetupGet(e => e.EnvironmentName).Returns(environmentName);
				services.AddSingleton(environmentMock.Object);
			}

			if (serviceFabricPublisher == true)
			{
				services.AddServiceFabricHealthChecks();
			}
			else if (serviceFabricPublisher == false)
			{
				services.AddRestHealthChecksPublisher();
			}
			else if (validated)
			{
				services.AddValidatedHealthCheckHttpClient(ClientName);
			}
			else
			{
				services.AddHealthCheckHttpClient(ClientName);
			}

			using ServiceProvider provider = services.BuildServiceProvider();

			HttpClientFactoryOptions options = provider
				.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>()
				.Get(clientName);

			TestHttpMessageHandlerBuilder builder = new(clientName, provider);
			foreach (Action<HttpMessageHandlerBuilder> action in options.HttpMessageHandlerBuilderActions)
			{
				action(builder);
			}

			Assert.IsInstanceOfType(builder.PrimaryHandler, typeof(HttpClientHandler));
			using HttpClientHandler handler = (HttpClientHandler)builder.PrimaryHandler;

			Assert.IsNull(handler.Credentials, "Health checks must not automatically send Windows credentials");
			Assert.IsFalse(handler.UseDefaultCredentials, "Default credentials should remain disabled");
			Assert.AreEqual(!serviceFabricPublisher.HasValue, handler.AllowAutoRedirect, "Publisher probes must not follow redirects");
			NullableAssert.IsNotNull(handler.ServerCertificateCustomValidationCallback, "A validation callback should be configured");

			var callback = handler.ServerCertificateCustomValidationCallback;
			return (request, errors) => callback(request, null, null, errors);
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
