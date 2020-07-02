// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting.Certificates
{
	internal class CertificateReader : ICertificateReader
	{
		private readonly ICertificateStore m_certificateStore;
		private readonly ILogger<CertificateReader> m_logger;
		private readonly ConcurrentDictionary<StoreName, CertificateInformation[]> m_certificatesCache;

		public CertificateReader(ICertificateStore certificateStore, ILogger<CertificateReader> logger)
		{
			m_certificateStore = certificateStore;
			m_logger = logger;
			m_certificatesCache = new ConcurrentDictionary<StoreName, CertificateInformation[]>();
		}

		public X509Certificate2? GetCertificateByThumbprint(string thumbprint, bool refreshCache, StoreName storeName)
		{
			Validation.ThrowIfNullOrWhiteSpace(thumbprint, nameof(thumbprint));

			X509Certificate2? matchingCertificate = null;
			foreach (CertificateInformation info in LoadCertificates(storeName, refreshCache))
			{
				if (string.Equals(info.Certificate.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase))
				{
					matchingCertificate = info.Certificate;
					break;
				}
			}

			if (matchingCertificate == null)
			{
				m_logger.LogError(Tag.Create(), "Could not find certificate with {0} thumbprint.", thumbprint);
			}
			else
			{
				m_logger.LogDebug(Tag.Create(), "Found certificate with {0} thumbprint.", thumbprint);
			}

			return matchingCertificate;
		}

		public IEnumerable<X509Certificate2> GetCertificatesByCommonName(string commonName, bool refreshCache, StoreName storeName)
		{
			Validation.ThrowIfNullOrWhiteSpace(commonName, nameof(commonName));

			bool certFound = false;
			foreach (CertificateInformation info in LoadCertificates(storeName, refreshCache))
			{
				if (string.Equals(info.CommonName, commonName, StringComparison.Ordinal))
				{
					certFound = true;
					yield return info.Certificate;
				}
			}

			if (certFound)
			{
				m_logger.LogDebug(Tag.Create(), "Found at least one certificate with {0} as common name.", commonName);
			}
			else
			{
				m_logger.LogError(Tag.Create(), "Could not find certificate with {0} as common name.", commonName);
			}
		}

		public X509Certificate2? GetCertificateByCommonName(string commonName, bool refreshCache, StoreName storeName) =>
			PickBestCertificate(GetCertificatesByCommonName(commonName, refreshCache, storeName));

		private CertificateInformation[] LoadCertificates(StoreName storeName, bool refreshCache)
		{
			if (refreshCache || !m_certificatesCache.TryGetValue(storeName, out CertificateInformation[] certificates))
			{
				m_logger.LogInformation(Tag.Create(), "Updating certificates cache for store '{0}'.", storeName);

				certificates = m_certificateStore.GetAllCertificates(storeName, StoreLocation.LocalMachine).ToArray();

				m_logger.LogInformation(Tag.Create(), "Inserting '{0}' certificates to certificates cache for store '{1}'.", certificates.Length, storeName);

				m_certificatesCache[storeName] = certificates;
			}

			return certificates ?? Array.Empty<CertificateInformation>();
		}

		private static X509Certificate2? PickBestCertificate(IEnumerable<X509Certificate2> certificates)
		{
			DateTime now = DateTime.Now;
			X509Certificate2? selectedCert = null;
			bool isSelectedValid = false;
			foreach (X509Certificate2 current in certificates)
			{
				bool isValid = current.NotAfter >= now && current.NotBefore <= now;

				if (selectedCert == null
					|| (!isSelectedValid && isValid)
					|| (isSelectedValid == isValid && (selectedCert.NotAfter < current.NotAfter)))
				{
					selectedCert = current;
					isSelectedValid = isValid;
				}
			}

			return selectedCert;
		}
	}
}
