// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Experimentation;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// An empty implementation of <see cref="IExperimentManager"/>.
/// </summary>
internal sealed class EmptyExperimentManager : IExperimentManager
{
	/// <inheritdoc />
	public Task<IDictionary<string, object>> GetFlightsAsync(IDictionary<string, string> filters, CancellationToken cancellationToken) =>
		Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>());
}
