// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	public static class HealthCheckContextHelper
	{
		public static HealthCheckContext CreateCheckContext() =>
			new()
			{
				Registration = new HealthCheckRegistration(
					"TestName",
					new Mock<IHealthCheck>().Object,
					HealthStatus.Unhealthy,
					Enumerable.Empty<string>())
			};
	}
}
