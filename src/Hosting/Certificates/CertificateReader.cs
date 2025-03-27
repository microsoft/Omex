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
		private readonly ConcurrentDictionary<StoreName, CertificateInformation[]> m_certificatesCache;

		public CertificateReader(ICertificateStore certificateStore)
		{
			m_certificateStore = certificateStore;
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

			return matchingCertificate;
		}

		public IEnumerable<X509Certificate2> GetCertificatesByCommonName(string commonName, bool refreshCache, StoreName storeName)
		{
			Validation.ThrowIfNullOrWhiteSpace(commonName, nameof(commonName));

			foreach (CertificateInformation info in LoadCertificates(storeName, refreshCache))
			{
				if (string.Equals(info.CommonName, commonName, StringComparison.Ordinal))
				{
					yield return info.Certificate;
				}
			}
		}

		public X509Certificate2? GetCertificateByCommonName(string commonName, bool refreshCache, StoreName storeName) =>
			PickBestCertificate(GetCertificatesByCommonName(commonName, refreshCache, storeName));

		private CertificateInformation[] LoadCertificates(StoreName storeName, bool refreshCache)
		{
			CertificateInformation[]? certificates;
			if (refreshCache || !m_certificatesCache.TryGetValue(storeName, out certificates))
			{
				certificates = m_certificateStore.GetAllCertificates(storeName, StoreLocation.LocalMachine).ToArray();
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
