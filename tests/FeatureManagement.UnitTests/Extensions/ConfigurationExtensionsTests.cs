// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Extensions;

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class ConfigurationExtensionsTests
{
	#region GetOrCreate

	[TestMethod]
	public void GetOrCreate_WhenSettingsExist_ReturnsSettings()
	{
		// ARRANGE
		IConfigurationRoot configRoot = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?> { { "Value", "TestValue" } })
			.Build();

		// ACT
		MockSettings result = configRoot.GetOrCreate<MockSettings>();

		// ASSERT
		Assert.AreEqual("TestValue", result.Value);
	}

	[TestMethod]
	public void GetOrCreate_WhenSettingsDoNotExist_ReturnsNewInstance()
	{
		// ARRANGE
		IConfigurationRoot configRoot = new ConfigurationBuilder().Build();

		// ACT
		MockSettings result = configRoot.GetOrCreate<MockSettings>();

		// ASSERT
		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(MockSettings));
		Assert.AreEqual(string.Empty, result.Value);
	}

	#endregion

	private sealed class MockSettings
	{
		public string Value { get; set; } = string.Empty;
	}
}
