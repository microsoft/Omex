// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class HealthCheckParametersTests
	{
		[DataTestMethod]
		[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
		public void Constructor_SetsReportData(KeyValuePair<string, object>[] reportData)
		{
			HealthCheckParameters parameters = new(reportData);
			CollectionAssert.AreEquivalent(reportData, parameters.ReportData.ToArray(), nameof(HttpHealthCheckParameters.ReportData));
		}

		public static IEnumerable<object[]> GetData()
		{
			yield return new object[] { Array.Empty<KeyValuePair<string, object>>() };
			yield return new object[]
			{
				new Dictionary<string, object>
				{
					{ "key2", "SomeValue" },
					{ "key1", new object() }
				}.ToArray()
			};
		}
	}
}
