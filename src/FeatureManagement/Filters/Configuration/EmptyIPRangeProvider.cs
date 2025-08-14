// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters.Configuration;

using System.Net;

/// <summary>
/// Provides an empty implementation of the IP range provider that returns no IP ranges.
/// </summary>
public sealed class EmptyIPRangeProvider : IIPRangeProvider
{
	/// <inheritdoc/>
	public IPNetwork[]? GetIPRanges(string rangeName) =>
		null;
}
