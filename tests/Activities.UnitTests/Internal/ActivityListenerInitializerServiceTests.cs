// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Activities.UnitTests
{
	[TestClass]
	public class ActivityListenerInitializerServiceTests
	{
		[TestMethod]
		public async Task ActivityListeners_ControlsActivityCreation()
		{
			Mock<IActivityStartObserver> mockStartObserver = new();
			Mock<IActivityStopObserver> mockStopObserver = new();

			IOptionsMonitor<OmexActivityListenerOptions>? optionsMonitor = DefaultActivityListenerConfiguratorTests.CreateOptionsMonitor();
			OmexActivityListenerOptions options = optionsMonitor.CurrentValue;

			options.ShouldListenTo = true;
			options.Sample = ActivitySamplingResult.AllDataAndRecorded;
			options.SampleUsingParentId = ActivitySamplingResult.AllDataAndRecorded;

			ActivityListenerInitializerService service = new(
				[mockStartObserver.Object],
				[mockStopObserver.Object],
				new DefaultActivityListenerConfigurator(optionsMonitor));

			ActivitySource source = new(nameof(ActivityListeners_ControlsActivityCreation));

			Assert.IsNull(source.StartActivity("ActivityBeforeStart"));

			await service.StartAsync(CancellationToken.None);

			AssertActivityReporting(source.StartActivity("ActivityDuringRun"), mockStartObserver, mockStopObserver);

			options.Sample = ActivitySamplingResult.None;

			Assert.IsNull(source.StartActivity("ActivityWhenConfigDisabled"));

			options.Sample = ActivitySamplingResult.AllDataAndRecorded;

			AssertActivityReporting(source.StartActivity("ActivityWhenConfigEnable"), mockStartObserver, mockStopObserver);

			// check that Activity from Diagnostic listener also captured
			using (DiagnosticListener listener = new(nameof(ActivityListeners_ControlsActivityCreation)))
			{
				using Activity activity = new("DiagnosticsListenerActivity");

				listener.StartActivity(activity, null);
				mockStartObserver.Verify(s => s.OnStart(activity, null), Times.Once);
				mockStartObserver.Reset();

				listener.StopActivity(activity, null);
				mockStopObserver.Verify(s => s.OnStop(activity, null), Times.Once);
				mockStopObserver.Reset();
			}

			await service.StopAsync(CancellationToken.None);

			Assert.IsNull(source.StartActivity("ActivityAfterStop"));
		}

		private void AssertActivityReporting(
			Activity? activity,
			Mock<IActivityStartObserver> mockStartObserver,
			Mock<IActivityStopObserver> mockStopObserver)
		{
			NullableAssert.IsNotNull(activity);
			mockStartObserver.Verify(s => s.OnStart(activity, null), Times.Once);
			mockStartObserver.Reset();

			activity.Dispose();

			mockStopObserver.Verify(s => s.OnStop(activity, null), Times.Once);
			mockStopObserver.Reset();
		}
	}
}
