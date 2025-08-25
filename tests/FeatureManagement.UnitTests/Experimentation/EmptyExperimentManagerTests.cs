// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Experimentation;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.FeatureManagement.Experimentation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class EmptyExperimentManagerTests
{
	private readonly EmptyExperimentManager m_experimentManager = new();

	#region GetFlightsAsync

	[TestMethod]
	public async Task GetFlightsAsync_WithEmptyFilters_ReturnsEmptyDictionary()
	{
		// ARRANGE
		Dictionary<string, object> filters = [];

		// ACT
		IDictionary<string, object> result = await m_experimentManager.GetFlightsAsync(filters, CancellationToken.None);

		// ASSERT
		Assert.IsNotNull(result);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task GetFlightsAsync_WithFilters_ReturnsEmptyDictionary()
	{
		// ARRANGE
		Dictionary<string, object> filters = new()
		{
			{ "customerId", "customer123" },
			{ "market", "en-US" },
			{ "clientVersion", "1.0.0" },
		};

		// ACT
		IDictionary<string, object> result = await m_experimentManager.GetFlightsAsync(filters, CancellationToken.None);

		// ASSERT
		Assert.IsNotNull(result);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task GetFlightsAsync_WithCancellationToken_ReturnsEmptyDictionary()
	{
		// ARRANGE
		Dictionary<string, object> filters = new()
		{
			{ "experimentId", "exp123" },
		};
		using CancellationTokenSource cts = new();

		// ACT
		IDictionary<string, object> result = await m_experimentManager.GetFlightsAsync(filters, cts.Token);

		// ASSERT
		Assert.IsNotNull(result);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task GetFlightsAsync_WithCancelledToken_ReturnsEmptyDictionary()
	{
		// ARRANGE
		Dictionary<string, object> filters = [];
		using CancellationTokenSource cts = new();
		cts.Cancel();

		// ACT
		IDictionary<string, object> result = await m_experimentManager.GetFlightsAsync(filters, cts.Token);

		// ASSERT
		Assert.IsNotNull(result);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task GetFlightsAsync_MultipleCallsWithDifferentParameters_AlwaysReturnsNewEmptyDictionary()
	{
		// ARRANGE
		Dictionary<string, object> filters1 = new() { { "key1", "value1" } };
		Dictionary<string, object> filters2 = new() { { "key2", "value2" } };

		// ACT
		IDictionary<string, object> result1 = await m_experimentManager.GetFlightsAsync(filters1, CancellationToken.None);
		IDictionary<string, object> result2 = await m_experimentManager.GetFlightsAsync(filters2, CancellationToken.None);

		// ASSERT
		Assert.IsNotNull(result1);
		Assert.IsNotNull(result2);
		Assert.AreNotSame(result1, result2, "Each call should return a new dictionary instance");
		Assert.IsEmpty(result1);
		Assert.IsEmpty(result2);
	}

	[TestMethod]
	public void GetFlightsAsync_WhenCalled_ReturnsCompletedTask()
	{
		// ARRANGE
		Dictionary<string, object> filters = [];

		// ACT
		Task<IDictionary<string, object>> task = m_experimentManager.GetFlightsAsync(filters, CancellationToken.None);

		// ASSERT
		Assert.IsTrue(task.IsCompleted, "Task should be completed immediately");
		Assert.IsFalse(task.IsFaulted, "Task should not be faulted");
		Assert.IsFalse(task.IsCanceled, "Task should not be canceled");
	}

	#endregion
}
