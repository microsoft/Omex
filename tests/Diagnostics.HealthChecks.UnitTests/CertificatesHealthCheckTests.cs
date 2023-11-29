// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests.Composables;
using Microsoft.Omex.Extensions.Hosting.Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{

	[TestClass]
	public class CertificatesHealthCheckTests
	{
		private readonly ILogger<CertificatesHealthCheck> m_logger = new NullLogger<CertificatesHealthCheck>();
		private readonly ActivitySource m_activitySource = new(nameof(ObservableHealthCheckTests));
		private readonly HealthCheckParameters m_parameters = new();
		private readonly CancellationTokenSource m_cancellationTokenSource = new();
		private readonly string m_certSubjectName = "MockSubjectName";

		private Mock<IOptions<CertificatesHealthCheckOptions>> m_certificatesHealthCheckOptionsMock = new();
		private Mock<ICertificateReader> m_certificateReaderMock = new();

		[TestInitialize]
		public void Initialize()
		{
			CertificatesHealthCheckOptions certificatesHealthCheckOptions = new() { CertSubjectNames = { m_certSubjectName } };
			m_certificatesHealthCheckOptionsMock.Setup(m => m.Value).Returns(certificatesHealthCheckOptions);
		}

		[TestMethod]
		public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenCertificatesListIsEmpty()
		{
			CertificatesHealthCheckOptions certificatesHealthCheckOptions = new() { CertSubjectNames = { } };
			m_certificatesHealthCheckOptionsMock.Setup(m => m.Value).Returns(certificatesHealthCheckOptions);

			CertificatesHealthCheck healthCheck = new(
				m_parameters,
				m_logger,
				m_activitySource,
				m_certificateReaderMock.Object,
				m_certificatesHealthCheckOptionsMock.Object);

			HealthCheckResult result = await healthCheck.CheckHealthAsync(
				HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
				m_cancellationTokenSource.Token);

			Assert.IsNotNull(result);
			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		}

		[TestMethod]
		public async Task CheckHealthAsync_ShouldReturnHealthy_WhenCertificateIsValid()
		{
			m_certificateReaderMock.Setup(m => m.GetCertificateByCommonName(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<StoreName>()))
				.Returns(CreateCert(m_certSubjectName));

			CertificatesHealthCheck healthCheck = new(
				m_parameters,
				m_logger,
				m_activitySource,
				m_certificateReaderMock.Object,
				m_certificatesHealthCheckOptionsMock.Object);

			HealthCheckResult result = await healthCheck.CheckHealthAsync(
				HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
				m_cancellationTokenSource.Token);

			Assert.IsNotNull(result);
			Assert.AreEqual(HealthStatus.Healthy, result.Status);
		}

		[TestMethod]
		public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenCertificateNoLongerValid()
		{
			m_certificateReaderMock.Setup(m => m.GetCertificateByCommonName(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<StoreName>()))
				.Returns(CreateCert(m_certSubjectName, null, DateTime.Now.AddDays(-1)));

			CertificatesHealthCheck healthCheck = new(
				m_parameters,
				m_logger,
				m_activitySource,
				m_certificateReaderMock.Object,
				m_certificatesHealthCheckOptionsMock.Object);

			HealthCheckResult result = await healthCheck.CheckHealthAsync(
				HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
				m_cancellationTokenSource.Token);

			Assert.IsNotNull(result);
			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		}

		[TestMethod]
		public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenCertificateNotYetValid()
		{
			m_certificateReaderMock.Setup(m => m.GetCertificateByCommonName(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<StoreName>()))
				.Returns(CreateCert(m_certSubjectName, DateTime.Now.AddDays(1)));

			CertificatesHealthCheck healthCheck = new(
				m_parameters,
				m_logger,
				m_activitySource,
				m_certificateReaderMock.Object,
				m_certificatesHealthCheckOptionsMock.Object);

			HealthCheckResult result = await healthCheck.CheckHealthAsync(
				HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
				m_cancellationTokenSource.Token);

			Assert.IsNotNull(result);
			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		}

		[TestMethod]
		public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenCertificateDoesNotHavePrivateKey()
		{
			X509Certificate2 certWithoutPrivateKey = new(CreateCert(m_certSubjectName).Export(X509ContentType.Cert));
			m_certificateReaderMock.Setup(m => m.GetCertificateByCommonName(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<StoreName>()))
				.Returns(certWithoutPrivateKey);

			CertificatesHealthCheck healthCheck = new(
				m_parameters,
				m_logger,
				m_activitySource,
				m_certificateReaderMock.Object,
				m_certificatesHealthCheckOptionsMock.Object);

			HealthCheckResult result = await healthCheck.CheckHealthAsync(
				HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
				m_cancellationTokenSource.Token);

			Assert.IsNotNull(result);
			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		}

		[TestMethod]
		public async Task CheckHealthAsync_ShouldReturnUnhealthy_WhenCertificateNotExists()
		{
			m_certificateReaderMock.Setup(m => m.GetCertificateByCommonName(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<StoreName>()))
				.Returns<X509Certificate2>(null);

			CertificatesHealthCheck healthCheck = new(
				m_parameters,
				m_logger,
				m_activitySource,
				m_certificateReaderMock.Object,
				m_certificatesHealthCheckOptionsMock.Object);

			HealthCheckResult result = await healthCheck.CheckHealthAsync(
				HealthCheckTestHelpers.GetHealthCheckContext(healthCheck),
				m_cancellationTokenSource.Token);

			Assert.IsNotNull(result);
			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		}

		private static X509Certificate2 CreateCert(string commonName, DateTimeOffset? notBefore = null, DateTimeOffset? notAfter = null) =>
			new CertificateRequest($"cn={commonName}", ECDsa.Create(), HashAlgorithmName.SHA256)
				.CreateSelfSigned(
					notBefore.GetValueOrDefault(DateTimeOffset.Now.Date.AddYears(-1)),
					notAfter.GetValueOrDefault(DateTimeOffset.Now.Date.AddYears(1)));
	}
}
