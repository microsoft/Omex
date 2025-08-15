// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Filters.Configuration;

using System.Net;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class EmptyIPRangeProviderTests
{
	private readonly EmptyIPRangeProvider m_provider = new();

	#region GetIPRanges

	[TestMethod]
	[DataRow("")]
	[DataRow("  ")]
	[DataRow("test")]
	public void GetIPRanges_WhenCalledWithAnyRangeName_ReturnsNull(string rangeName)
	{
		// ACT
		IPNetwork[]? result = m_provider.GetIPRanges(rangeName);

		// ASSERT
		Assert.IsNull(result);
	}

	#endregion
}
