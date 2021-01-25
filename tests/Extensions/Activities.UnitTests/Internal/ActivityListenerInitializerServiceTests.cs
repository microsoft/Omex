// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Microsoft.Omex.Extensions.Activities;
using Moq;
using Microsoft.Extensions.Options;
using System;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Omex.Extensions.Testing.Helpers;

namespace Microsoft.Omex.Extensions.Activities.UnitTests
{
	[TestClass]
	public class ActivityListenerInitializerServiceTests
	{
		[TestMethod]
		public async Task ActivityListeners_ControlsActivityCreation()
		{
			Mock<IActivityStartObserver> mockStartObserver = new Mock<IActivityStartObserver>();
			Mock<IActivityStopObserver> mockStopObserver = new Mock<IActivityStopObserver>();

			IOptionsMonitor<OmexActivityListenerOptions>? optionsMonitor = DefaultActivityListenerConfiguratorTests.CreateOptionsMonitor();
			OmexActivityListenerOptions options = optionsMonitor.CurrentValue;

			options.ShouldListenTo = true;
			options.Sample = ActivitySamplingResult.AllDataAndRecorded;
			options.SampleUsingParentId = ActivitySamplingResult.AllDataAndRecorded;

			ActivityListenerInitializerService service = new ActivityListenerInitializerService(
				new IActivityStartObserver[] { mockStartObserver.Object },
				new IActivityStopObserver[] { mockStopObserver.Object },
				new DefaultActivityListenerConfigurator(optionsMonitor));

			ActivitySource source = new ActivitySource(nameof(ActivityListeners_ControlsActivityCreation));

			Assert.IsNull(source.StartActivity("ActivityBeforeStart"));

			await service.StartAsync(CancellationToken.None);

			AssertActivityReporting(source.StartActivity("ActivityDuringRun"), mockStartObserver, mockStopObserver);

			options.Sample = ActivitySamplingResult.None;

			Assert.IsNull(source.StartActivity("ActivityWhenConfigDisabled"));

			options.Sample = ActivitySamplingResult.AllDataAndRecorded;

			AssertActivityReporting(source.StartActivity("ActivityWhenConfigEnable"), mockStartObserver, mockStopObserver);

			await service.StopAsync(CancellationToken.None);

			Assert.IsNull(source.StartActivity("ActivityAfterStop"));
		}

		private void AssertActivityReporting(
			Activity? activity,
			Mock<IActivityStartObserver> mockStartObserver,
			Mock<IActivityStopObserver> mockStopObserver)
		{
			NullableAssert.IsNotNull(activity);
			mockStartObserver.Verify(s => s.OnStart(It.IsAny<Activity>(), null), Times.Once);
			mockStartObserver.Reset();

			activity.Dispose();

			mockStopObserver.Verify(s => s.OnStop(It.IsAny<Activity>(), null), Times.Once);
			mockStopObserver.Reset();
		}
	}
}
