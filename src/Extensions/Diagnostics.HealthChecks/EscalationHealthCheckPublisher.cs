// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{

	internal class EscalationHealthCheckPublisher : IHealthCheckPublisher
	{
		public EscalationHealthCheckPublisher()
		{
		}

		public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
