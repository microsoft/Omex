// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Omex.Extensions.Hosting.Certificates
{
	/// <summary>
	/// Certificate reader interface
	/// </summary>
	public interface ICertificateReader
	{
		/// <summary>
		/// Returns all certificates which match commonName parameter
		/// </summary>
		/// <param name="commonName">Common name of certificates</param>
		/// <param name="refreshCache">Refresh cache before looking for certificate</param>
		/// <param name="storeName">Certificate store</param>
		IEnumerable<X509Certificate2> GetCertificatesByCommonName(string commonName, bool refreshCache = false, StoreName storeName = StoreName.My);

		/// <summary>
		/// Returns certificate which match commonName parameter
		/// If there is more than one certificate with given thumbprint return the most suitable certificate
		/// A certificate is considered active if notBefore &lt;= now &gt;= notAfter
		/// A certificate is considered better than another certificate if
		/// 1.) Is active and the other one is inactive
		/// 2.) Both are active but the other one expires sooner
		/// 3.) Both are expired, but the other one expired earlier
		/// </summary>
		/// <param name="commonName">Common name of certificates</param>
		/// <param name="refreshCache">Refresh cache before looking for certificate</param>
		/// <param name="storeName">Certificate store</param>
		X509Certificate2? GetCertificateByCommonName(string commonName, bool refreshCache = false, StoreName storeName = StoreName.My);

		/// <summary>
		/// Get certificate by thumbprint
		/// </summary>
		/// <param name="thumbprint">Thumbprint of certificate</param>
		/// <param name="refreshCache">Refresh cache before looking for certificate</param>
		/// <param name="storeName">Certificate store</param>
		/// <remarks>It preferable to getting certificate by common name.
		/// Please use this method only in cases where retrieving by common name might not work, e.g. a certificate used to decrypt an access token.
		/// </remarks>
		X509Certificate2? GetCertificateByThumbprint(string thumbprint, bool refreshCache = false, StoreName storeName = StoreName.My);
	}
}
