// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Omex.Extensions.Hosting.Certificates
{
	internal interface ICertificateStore
	{
		/// <summary>
		/// Reads all X509Certificate2s from store with name storeName on local machine
		/// </summary>
		/// <param name="storeName">Name of the store to read from</param>
		/// <param name="storeLocation">Location of store</param>
		/// <returns>X509 certificates from the store</returns>
		IEnumerable<CertificateInformation> GetAllCertificates(StoreName storeName, StoreLocation storeLocation = StoreLocation.LocalMachine);
	}
}
