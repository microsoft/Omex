// Copyright (C) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.AspNetCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests.Composables;

[TestClass]
public class HealthCheckActivityMarkersTests
{
	[TestMethod]
	public void FilterLivenessCheckActionFilter_ReturnsOkForShortCircuit()
	{
		ActivitySource activitySource = new(nameof(HealthCheckActivityMarkersTests));
		DefaultHttpContext httpContext = new();
		ActionExecutingContext context = new(
			new ActionContext(
				httpContext: httpContext,
				routeData: new(),
				actionDescriptor: new(),
				modelState: new()
			),
			new List<IFilterMetadata>(),
			new Dictionary<string, object?>(),
			new Mock<Controller>().Object);

		LivenessCheckActionFilterAttribute filterAttribute = new();
		Activity.Current = new Activity(nameof(FilterLivenessCheckActionFilter_ReturnsOkForShortCircuit))
			.MarkAsLivenessCheck()
			.Start();

		filterAttribute.OnActionExecuting(context);

		Assert.IsTrue(context.Result is OkObjectResult);

		Activity.Current = null;
	}

	[TestMethod]
	public void FilterLivenessCheckActionFilter_ReturnsContextResultForNonShortCircuit()
	{
		ActivitySource activitySource = new(nameof(HealthCheckActivityMarkersTests));
		DefaultHttpContext httpContext = new();
		ActionExecutingContext context = new(
			new ActionContext(
				httpContext: httpContext,
				routeData: new(),
				actionDescriptor: new(),
				modelState: new()
			),
			new List<IFilterMetadata>(),
			new Dictionary<string, object?>(),
			new Mock<Controller>().Object);

		LivenessCheckActionFilterAttribute filterAttribute = new();
		Activity.Current = new Activity(nameof(FilterLivenessCheckActionFilter_ReturnsContextResultForNonShortCircuit))
			.Start();

		filterAttribute.OnActionExecuting(context);

		Assert.IsNull(context.Result);

		Activity.Current = null;
	}

	private static Mock<IMemoryCache> GetMemoryCacheMock(bool value, string repeatOnceMemoryCacheKey)
	{
		Mock<ICacheEntry> entry = new();
		entry.Setup(e => e.Value)
			.Returns(value);

		Mock<IMemoryCache> memoryCacheMock = new();
		memoryCacheMock.Setup(c => c.CreateEntry(repeatOnceMemoryCacheKey))
			.Returns(entry.Object);

		object? castedValue = value;

		memoryCacheMock.Setup(c => c.TryGetValue(repeatOnceMemoryCacheKey, out castedValue))
			.Returns(true);

		return memoryCacheMock;
	}
}
