﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
		public static ICertificateReader Instance { get; } = new CertificateReader(new CertificateStore(), InitializationLogger.Instance);
	}
}
