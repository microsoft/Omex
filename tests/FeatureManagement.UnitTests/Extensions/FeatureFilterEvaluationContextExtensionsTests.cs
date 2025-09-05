// Copyright (c) Microsoft Corporation. All rights reserved.
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
[TestCategory("Extensions")]
public sealed class FeatureFilterEvaluationContextExtensionsTests
{
	#region Evaluate

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
		DefaultHttpContext httpContext = new();
		httpContext.Request.QueryString = new QueryString("?param=any");
		HttpContextAccessor httpContextAccessor = new() { HttpContext = httpContext };
		static List<string> extract(MockSettings s) => ["*"];

		// ACT

		bool result = context.Evaluate<MockSettings>(httpContextAccessor, "param", extract);

		// ASSERT
		Assert.IsTrue(result);
	}

	[TestMethod]
	[DataRow("abc", "abc", true)]
	[DataRow("abc", "ABC", true)]
	[DataRow("abc", "def", false)]
	public void Evaluate_WithValueInSettings_ReturnsExpected(string queryValue, string settingValue, bool expected)
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
		httpContext.Request.QueryString = new QueryString($"?param={queryValue}");
		HttpContextAccessor httpContextAccessor = new() { HttpContext = httpContext };
		Func<MockSettings, List<string>> extract = s => [settingValue];

		// ACT
		bool result = context.Evaluate<MockSettings>(httpContextAccessor, "param", extract);

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
		DefaultHttpContext httpContext = new();
		httpContext.Request.QueryString = new QueryString("?param=abc");
		HttpContextAccessor httpContextAccessor = new() { HttpContext = httpContext };
		Func<MockSettings, List<string>> extract = s => ["def", "deg", "abc"];

		// ACT
		bool result = context.Evaluate<MockSettings>(httpContextAccessor, "param", extract);

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
		DefaultHttpContext httpContext = new();
		httpContext.Request.QueryString = new QueryString("?param=abc");
		HttpContextAccessor httpContextAccessor = new() { HttpContext = httpContext };
		Func<MockSettings, List<string>> extract = s => ["def", "deg"];

		// ACT
		bool result = context.Evaluate<MockSettings>(httpContextAccessor, "param", extract);

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
		DefaultHttpContext httpContext = new();
		httpContext.Request.QueryString = new QueryString("?param=abc");
		HttpContextAccessor httpContextAccessor = new() { HttpContext = httpContext };
		Func<MockSettings, List<string>> extract = s => [];

		// ACT
		bool result = context.Evaluate<MockSettings>(httpContextAccessor, "param", extract);

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Evaluate_WithMissingQueryParameter_ReturnsFalse()
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
		httpContext.Request.QueryString = new QueryString("?otherparam=abc");
		HttpContextAccessor httpContextAccessor = new() { HttpContext = httpContext };
		static List<string> extract(MockSettings s) => ["abc"];

		// ACT
		bool result = context.Evaluate<MockSettings>(httpContextAccessor, "param", extract);

		// ASSERT
		Assert.IsFalse(result);
	}

	#endregion

	private sealed class MockSettings
	{
		public List<string> Values { get; set; } = [];
	}
}
