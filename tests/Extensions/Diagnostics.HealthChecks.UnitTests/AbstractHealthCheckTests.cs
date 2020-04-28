// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.TimedScopes.UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class AbstractHealthCheckTests
	{
		[TestMethod]
		public async Task AbstractHealthCheck_PropagatesParametersAndResult()
		{
			HealthCheckResult expectedResult = HealthCheckResult.Healthy("test");
			HealthCheckContext expectedContext = new HealthCheckContext();
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
			Activity? activity = null;
			Exception exception = new ArrayTypeMismatchException();

			HealthCheckResult actualResult = await new TestHealthCheck((c, t) =>
			{
				activity = Activity.Current;
				throw exception;
			}
			).CheckHealthAsync(new HealthCheckContext());

			Assert.AreEqual(actualResult.Exception, exception);
			Assert.AreEqual(actualResult.Status, HealthStatus.Unhealthy);
			Assert.IsNotNull(activity);
			Assert.IsTrue(activity!.IsHealthCheck(), "Activity should be marked as HealthCheck activity");
			activity!.AssertResult(TimedScopeResult.SystemError);
		}

		[TestMethod]
		public async Task AbstractHealthCheck_MarksActivityWithHealthCheckFlag()
		{
			Activity? activity = null;

			HealthCheckResult actualResult = await new TestHealthCheck((c, t) =>
			{
				activity = Activity.Current;
				return HealthCheckResult.Healthy();
			}).CheckHealthAsync(new HealthCheckContext());

			Assert.AreEqual(actualResult.Status, HealthStatus.Unhealthy);
			Assert.IsNotNull(activity);
			Assert.IsTrue(activity!.IsHealthCheck(), "Activity should be marked as HealthCheck activity");
			activity!.AssertResult(TimedScopeResult.Success);
		}

		private class TestHealthCheck : AbstractHealthCheck
		{
			private readonly Func<HealthCheckContext, CancellationToken, HealthCheckResult> m_internalFunction;

			public TestHealthCheck(Func<HealthCheckContext, CancellationToken, HealthCheckResult> internalFunction)
				: base(new SimpleScopeProvider()) =>
					m_internalFunction = internalFunction;

			protected override Task<HealthCheckResult> CheckHealthInternalAsync(HealthCheckContext context, CancellationToken token = default) =>
				Task.FromResult(m_internalFunction(context, token));
		}
	}
}
