// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Hosting.Certificates
{
	/// <summary>
	/// CertificateReader to use until DI container build.
	/// </summary>
	/// <remarks>
	/// Please prefer using instance resolved from DI container.
	/// </remarks>
	public static class InitializationCertificateReader
	{
		/// <summary>
		/// Instance of CertificateReader
		/// </summary>
#pragma warning disable OMEX188 // InitializationLogger using OmexLogger is obsolete and is pending for removal by 1 July 2024.
		public static ICertificateReader Instance { get; } = new CertificateReader(new CertificateStore(), InitializationLogger.Instance);
#pragma warning restore OMEX188 // InitializationLogger using OmexLogger is obsolete and is pending for removal by 1 July 2024.
	}
}
