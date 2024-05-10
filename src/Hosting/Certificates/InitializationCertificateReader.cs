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
#pragma warning disable CS0618 // InitializationLogger using OmexLogger is obsolete and is pending for removal by 1 July 2024. Code: 8913598
		public static ICertificateReader Instance { get; } = new CertificateReader(new CertificateStore(), InitializationLogger.Instance);
#pragma warning restore CS0618 // InitializationLogger using OmexLogger is obsolete and is pending for removal by 1 July 2024. Code: 8913598
	}
}
