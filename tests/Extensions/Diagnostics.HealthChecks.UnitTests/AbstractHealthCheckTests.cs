// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.Omex.Extensions.Testing.Helpers.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class AbstractHealthCheckTests
	{
		[TestMethod]
		public async Task AbstractHealthCheck_PropagatesParametersAndResult()
		{
			HealthCheckResult expectedResult = HealthCheckResult.Healthy("test");
			HealthCheckContext expectedContext = HealthCheckContextHelper.CreateCheckContext();
			CancellationToken expectedToken = new CancellationTokenSource().Token;

			HealthCheckContext? actualContext = default;
			CancellationToken actualToken = default;

			HealthCheckResult actualResult = await new TestHealthCheck((context, token) =>
				{
					actualContext = context;
					actualToken = token;
					return expectedResult;
				}).CheckHealthAsync(expectedContext, expectedToken);

			Assert.AreEqual(expectedResult, actualResult);
			Assert.AreEqual(expectedContext, actualContext);
			Assert.AreEqual(expectedToken, actualToken);
		}

		[TestMethod]
		public async Task AbstractHealthCheck_WhenExceptionThrown_ReturnsUnhealtyState()
		{
			using TestActivityListener listener = new TestActivityListener();
			Activity? activity = null;
			Exception exception = new ArrayTypeMismatchException();

			HealthCheckResult actualResult = await new TestHealthCheck((c, t) =>
			{
				activity = Activity.Current;
				throw exception;
			}
			).CheckHealthAsync(HealthCheckContextHelper.CreateCheckContext());

			Assert.AreEqual(actualResult.Exception, exception);
			Assert.AreEqual(actualResult.Status, HealthStatus.Unhealthy);
			CollectionAssert.AreEquivalent(actualResult.Data.ToArray(), TestHealthCheck.TestParameters);
			NullableAssert.IsNotNull(activity);
			Assert.IsTrue(activity.IsHealthCheck(), "Activity should be marked as HealthCheck activity");
			activity.AssertResult(ActivityResult.SystemError);
		}

		[TestMethod]
		public async Task AbstractHealthCheck_MarksActivityWithHealthCheckFlag()
		{
			using TestActivityListener listener = new TestActivityListener();
			Activity? activity = null;

			HealthCheckResult actualResult = await new TestHealthCheck((c, t) =>
			{
				activity = Activity.Current;
				return HealthCheckResult.Healthy();
			}).CheckHealthAsync(HealthCheckContextHelper.CreateCheckContext());

			Assert.AreEqual(actualResult.Status, HealthStatus.Healthy);
			NullableAssert.IsNotNull(activity);
			Assert.IsTrue(activity!.IsHealthCheck(), "Activity should be marked as HealthCheck activity");
			activity.AssertResult(ActivityResult.Success);
		}

		private class TestHealthCheck : AbstractHealthCheck<HealthCheckParameters>
		{
			private readonly Func<HealthCheckContext, CancellationToken, HealthCheckResult> m_internalFunction;

			public TestHealthCheck(Func<HealthCheckContext, CancellationToken, HealthCheckResult> internalFunction)
				: base(new HealthCheckParameters(TestParameters), new NullLogger<TestHealthCheck>(), new ActivitySource("Test")) =>
					m_internalFunction = internalFunction;

			protected override Task<HealthCheckResult> CheckHealthInternalAsync(HealthCheckContext context, CancellationToken token = default) =>
				Task.FromResult(m_internalFunction(context, token));

			public static KeyValuePair<string, object>[] TestParameters = new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("TestKey", "TestValue")
				};
		}
	}
}
