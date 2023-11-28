// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Represent the individual health check options associated with an <see cref="T:Microsoft.Omex.Extensions.Diagnostics.HealthChecks.CertificatesHealthCheck"/>.
	/// </summary>
	public class CertificatesHealthCheckOptions
	{
		/// <summary>
		/// The subject names of the certificates that need to be validated.
		/// </summary>
		[Required]
		public List<string> CertSubjectNames { get; set; } = new();
	}
}
