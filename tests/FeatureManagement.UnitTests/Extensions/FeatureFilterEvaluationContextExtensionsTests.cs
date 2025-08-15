// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Extensions;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class FeatureFilterEvaluationContextExtensionsTests
{
	#region Evaluate (query-string parameter)

	[TestMethod]
	public void Evaluate_WithHttpContextAccessor_DelegatesToStringOverload()
	{
		// ARRANGE
		IConfigurationRoot configRoot = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>())
			.Build();
		FeatureFilterEvaluationContext context = new()
		{
			Parameters = configRoot,
		};
		DefaultHttpContext httpContext = new();
		httpContext.Request.QueryString = new QueryString("?param=abc");
		HttpContextAccessor httpContextAccessor = new() { HttpContext = httpContext };
		Func<MockSettings, List<string>> extract = s => ["abc"];

		// ACT
		bool result = context.Evaluate<MockSettings>(httpContextAccessor, "param", extract);

		// ASSERT
		Assert.IsTrue(result);
	}

	#endregion

	#region Evaluate (string)

	[TestMethod]
	public void Evaluate_WithWildcardInSettings_ReturnsTrue()
	{
		// ARRANGE
		IConfigurationRoot configRoot = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?> { { "Values", "*" } })
			.Build();
		FeatureFilterEvaluationContext context = new()
		{
			Parameters = configRoot,
		};
		Func<MockSettings, List<string>> extract = s => ["*"];

		// ACT
		bool result = context.Evaluate<MockSettings>("any", extract);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	[DataRow("abc", "abc", true)]
	[DataRow("abc", "ABC", true)]
	[DataRow("abc", "def", false)]
	public void Evaluate_WithValueInSettings_ReturnsExpected(string value, string settingValue, bool expected)
	{
		// ARRANGE
		IConfigurationRoot configRoot = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>())
			.Build();
		FeatureFilterEvaluationContext context = new()
		{
			Parameters = configRoot,
		};
		Func<MockSettings, List<string>> extract = s => [settingValue];

		// ACT
		bool result = context.Evaluate<MockSettings>(value, extract);

		// ASSERT
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void Evaluate_WithMultipleValuesInSettings_ReturnsTrueIfAnyMatch()
	{
		// ARRANGE
		IConfigurationRoot configRoot = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>())
			.Build();
		FeatureFilterEvaluationContext context = new()
		{
			Parameters = configRoot,
		};
		Func<MockSettings, List<string>> extract = s => ["def", "deg", "abc"];

		// ACT
		bool result = context.Evaluate<MockSettings>("abc", extract);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Evaluate_WithNoMatch_ReturnsFalse()
	{
		// ARRANGE
		IConfigurationRoot configRoot = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>())
			.Build();
		FeatureFilterEvaluationContext context = new()
		{
			Parameters = configRoot,
		};
		Func<MockSettings, List<string>> extract = s => ["def", "deg"];

		// ACT
		bool result = context.Evaluate<MockSettings>("abc", extract);

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Evaluate_WithEmptySettingsList_ReturnsFalse()
	{
		// ARRANGE
		IConfigurationRoot configRoot = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>())
			.Build();
		FeatureFilterEvaluationContext context = new FeatureFilterEvaluationContext
		{
			Parameters = configRoot,
		};
		Func<MockSettings, List<string>> extract = s => [];

		// ACT
		bool result = context.Evaluate<MockSettings>("abc", extract);

		// ASSERT
		Assert.IsFalse(result);
	}

	#endregion

	private sealed class MockSettings
	{
		public List<string> Values { get; set; } = [];
	}
}
