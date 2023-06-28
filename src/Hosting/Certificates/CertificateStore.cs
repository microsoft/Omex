// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Omex.Extensions.Hosting.Certificates
{
	internal class CertificateStore : ICertificateStore
	{
		/// <inheritdoc/>
		public IEnumerable<CertificateInformation> GetAllCertificates(StoreName storeName, StoreLocation storeLocation)
		{
			using X509Store store = new(storeName, storeLocation);
			store.Open(OpenFlags.ReadOnly);
			return store.Certificates
				.OfType<X509Certificate2>()
				.Select(cert => new CertificateInformation(cert));
		}
	}
}
