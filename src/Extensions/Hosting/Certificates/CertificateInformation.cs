// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Omex.Extensions.Hosting.Certificates
{
	internal readonly struct CertificateInformation
	{
		public readonly X509Certificate2 Certificate { get; }
		public readonly string CommonName { get; }

		public CertificateInformation(X509Certificate2 certificate)
		{
			Certificate = certificate;
			CommonName = GetCommonName(certificate);
		}

		private static string GetCommonName(X509Certificate2 cert) =>
			cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
	}
}
