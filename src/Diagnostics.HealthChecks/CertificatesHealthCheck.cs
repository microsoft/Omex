// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;
using Microsoft.Omex.Extensions.Hosting.Certificates;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// This health check verifies whether the certificates have been loaded correctly.
/// </summary>
public class CertificatesHealthCheck : IHealthCheck
{
	private readonly IHealthCheck m_healthCheck;

	/// <summary>
	/// The constructor.
	/// </summary>
	/// <param name="parameters"></param>
	/// <param name="logger"></param>
	/// <param name="activitySource"></param>
	/// <param name="certificateReader"></param>
	/// <param name="options"></param>
	public CertificatesHealthCheck(
		HealthCheckParameters parameters,
		ILogger<CertificatesHealthCheck> logger,
		ActivitySource activitySource,
		ICertificateReader certificateReader,
		IOptions<CertificatesHealthCheckOptions> options)
	{
		HashSet<string> uniqueCertificateSubjectNames = new(options.Value.CertSubjectNames);

		m_healthCheck = new CertificatesHealthCheckInternal(
			parameters,
			uniqueCertificateSubjectNames,
			logger,
			certificateReader)
				.AsObservableHealthCheck(activitySource, logger, parameters: parameters);
	}

	/// <inheritdoc />
	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
		m_healthCheck.CheckHealthAsync(context, cancellationToken);

	private class CertificatesHealthCheckInternal : IHealthCheck
	{
		private readonly ILogger m_logger;
		private readonly ICertificateReader m_certificateReader;
		private readonly HashSet<string> m_uniqueCertificateSubjectNames;
		private readonly HealthCheckParameters m_parameters;

		public CertificatesHealthCheckInternal(
			HealthCheckParameters parameters,
			HashSet<string> uniqueCertificateSubjectNames,
			ILogger logger,
			ICertificateReader certificateReader)
		{
			m_logger = logger;
			m_certificateReader = certificateReader;
			m_uniqueCertificateSubjectNames = uniqueCertificateSubjectNames;
			m_parameters = parameters;
		}

		public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
		{
			if (m_uniqueCertificateSubjectNames.Count == 0)
			{
				string emptyCertNames = "No certificate names are configured to be checked.";
				m_logger.LogError(Tag.Create(), emptyCertNames);
				return Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy, emptyCertNames, data: m_parameters.ReportData));
			}

			HealthStatus healthStatus = HealthStatus.Healthy;
			StringBuilder message = new();

			foreach (string certName in m_uniqueCertificateSubjectNames)
			{
				X509Certificate2? x509Certificate2 = m_certificateReader.GetCertificateByCommonName(certName);
				ValidateCertificate(x509Certificate2, certName, message, ref healthStatus);
			}

			return Task.FromResult(new HealthCheckResult(healthStatus, message.ToString(), data: m_parameters.ReportData));
		}

		private void ValidateCertificate(X509Certificate2? certificate, string certName, StringBuilder message, ref HealthStatus healthStatus)
		{
			string errorMessage = string.Empty;
			DateTime localNow = DateTime.Now;

			if (certificate == null)
			{
				errorMessage = $"Certificate with subject name '{certName}' was not found.";
			}
			else if (certificate.NotBefore > localNow || certificate.NotAfter < localNow)
			{
				errorMessage = $"Certificate with subject name '{certName}' is outside its validity period '{certificate.NotBefore.ToUniversalTime():u}' - '{certificate.NotAfter.ToUniversalTime():u}'.";
			}
			else if (!certificate.HasPrivateKey)
			{
				errorMessage = $"Certificate with subject name '{certName}' does not have a private key.";
			}

			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				message.AppendLine(errorMessage);
				m_logger.LogError(Tag.Create(), errorMessage);
				healthStatus = HealthStatus.Unhealthy;
			}
			else
			{
				string loadedCertSuccessfully = $"Certificate with subject name '{certName}' loaded successfully.";
				message.AppendLine(loadedCertSuccessfully);
				m_logger.LogInformation(Tag.Create(), loadedCertSuccessfully);
			}
		}
	}
}
