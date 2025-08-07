// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Activities.UnitTests.Internal
{
	[TestClass]
	public class ActivityMetricsSenderTests
	{
		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void SendActivityMetric_NoCustomDimensions_ProduceMetricPointSuccessfully(bool isHealthCheck)
		{
			// 1. Arrange
			(IExecutionContext context, IHostEnvironment environment, IOptions<ActivityOption> options) = PrepareEnvironment();

			ActivityMetricsSender sender = new(
				context,
				environment,
				isHealthCheck ? new CustomBaggageDimensions(["HealthCheckMarker"]) : new CustomBaggageDimensions(),
				new CustomTagObjectsDimensions(),
				options);
			Listener listener = new();

			Activity activity = new(nameof(activity));

			// 2. Act
			if (isHealthCheck)
			{
				activity.Start().MarkAsHealthCheck().Stop();
			}
			else
			{
				activity.Start().Stop();
			}

			sender.SendActivityMetric(activity);

			// 3. Assert
			VerifyTagsExist(listener, activity, context, environment, isHealthCheck ? OmexActivityConfiguration.HealthCheckActivitiesHistogramName : OmexActivityConfiguration.ActivitiesHistogramName);
		}

		[TestMethod]
		[DataRow(false)]
		[DataRow(true)]
		public void SendActivityMetric_CustomDimensionsAreRegistered_ProduceMetricPointWithRegisteredDimensions(bool isHealthCheck)
		{
			// 1. Arrange
			(IExecutionContext context, IHostEnvironment environment, IOptions<ActivityOption> options) = PrepareEnvironment();

			const string testBaggage1 = "TestBaggage1";
			const string testBaggage2 = "TestBaggage2";
			const string testTag1 = "TestTag1";
			const string testTag2 = "TestTag1";

			ActivityMetricsSender sender = new(
				context,
				environment,
				isHealthCheck ?
					new CustomBaggageDimensions(["HealthCheckMarker", testBaggage1, testBaggage2])
					: new CustomBaggageDimensions([testBaggage1, testBaggage2]),
				new CustomTagObjectsDimensions(
				[
					ActivityTagKeys.Result, ActivityTagKeys.Metadata, ActivityTagKeys.SubType,
					testTag1, testTag2
				]),
				options);

			Listener listener = new();

			Activity activity = new(nameof(activity));
			activity
				.SetBaggage(testBaggage1, "value1")
				.SetBaggage(testBaggage2, "value2")
				.SetTag(testTag1, "value3")
				.SetTag(testTag2, "value4")
				.MarkAsSuccess()
				.SetMetadata("TestMetadata")
				.SetSubType("TestSubType");

			// 2. Act
			if (isHealthCheck)
			{
				activity.Start().MarkAsHealthCheck().Stop();
			}
			else
			{
				activity.Start().Stop();
			}
			sender.SendActivityMetric(activity);

			// 3. Assert
			VerifyTagsExist(listener, activity, context, environment, isHealthCheck ? OmexActivityConfiguration.HealthCheckActivitiesHistogramName : OmexActivityConfiguration.ActivitiesHistogramName);
		}

		[TestMethod]
		public void SendActivityMetric_CustomDimensionsAreNotRegistered_FailToProduceMetricPoint()
		{
			// 1. Arrange
			(IExecutionContext context, IHostEnvironment environment, IOptions<ActivityOption> options) = PrepareEnvironment();

			const string testBaggage1 = "TestBaggage1";
			const string testBaggage2 = "TestBaggage2";
			const string testTag1 = "TestTag1";
			const string testTag2 = "TestTag1";

			ActivityMetricsSender sender = new(
				context,
				environment,
				new CustomBaggageDimensions([]), // Override by empty set
				new CustomTagObjectsDimensions([]),
				options); // Override by empty set

			Listener listener = new();

			Activity activity = new(nameof(activity));
			activity
				.Start()
				.SetBaggage(testBaggage1, "value1")
				.SetBaggage(testBaggage2, "value2")
				.SetTag(testTag1, "value3")
				.SetTag(testTag2, "value4")
				.MarkAsSuccess()
				.MarkAsHealthCheck()
				.SetMetadata("TestMetadata")
				.SetSubType("TestSubType")
				.Stop();

			sender.SendActivityMetric(activity);

			// 3. Assert
			MeasurementResult result = listener.Results.First(m => environment.EnvironmentName.Equals(m.Tags[s_environmentTagName]));

			foreach (KeyValuePair<string, object?> tagPair in activity.TagObjects)
			{
				Assert.ThrowsExactly<KeyNotFoundException>(() => result.Tags[tagPair.Key]);
			}

			foreach (KeyValuePair<string, string?> tagPair in activity.Baggage)
			{
				Assert.ThrowsExactly<KeyNotFoundException>(() => result.Tags[tagPair.Key]);
			}
		}

		[TestMethod]
		[DataRow(true, true, true, DisplayName = "Parent Name emmitted when parent is present and has a name")]
		[DataRow(true, false, false, DisplayName = "Parent Name not emmitted when parent is present but has no name")]
		[DataRow(false, false, false, DisplayName = "Parent Name not emmitted when parent is not present")]
		public void SendActivityMetric_WithSendParentName_ProducesMetricPointWithParentName(bool hasParentActivity, bool hasParentName, bool expectParentNameToBeEmitted)
		{
			// 1. Arrange
			ActivityOption activityOptions = new()
			{
				SetParentNameAsDimensionEnabled = true
			};
			(IExecutionContext context, IHostEnvironment environment, IOptions<ActivityOption> options) = PrepareEnvironment(activityOptions);

			ActivityMetricsSender sender = new(
				context,
				environment,
				new CustomBaggageDimensions([]),
				new CustomTagObjectsDimensions([]),
				options);
			Listener listener = new();

			string? parentName = hasParentName ? nameof(parentName) : null;
			using Activity? parent = hasParentActivity ? new(parentName!) : null;
			parent?.Start();

			using Activity activity = new(nameof(activity));
			activity.Start().Stop();

			parent?.Stop();

			sender.SendActivityMetric(activity);

			// 3. Assert
			MeasurementResult result = listener.Results.First(m => environment.EnvironmentName.Equals(m.Tags[s_environmentTagName]));

			if (expectParentNameToBeEmitted)
			{
				AssertTag(result, "ParentName", nameof(parentName));
			}
			else
			{
				AssertTagNotExist(result, "ParentName");
			}
		}

		private static void VerifyTagsExist(Listener listener, Activity activity, IExecutionContext context, IHostEnvironment environment, string instrumentationName)
		{
			MeasurementResult result = listener.Results.First(m => environment.EnvironmentName.Equals(m.Tags[s_environmentTagName]));

			Assert.AreEqual(instrumentationName, result.Instrument.Name);
			Assert.AreEqual(Convert.ToInt64(activity.Duration.TotalMilliseconds), result.Measurement);
			AssertTag(result, "Name", activity.OperationName);
			AssertTag(result, "Environment", environment.EnvironmentName);
			AssertTag(result, "RegionName", context.RegionName);
			AssertTag(result, "Cluster", context.Cluster);
			AssertTag(result, "ApplicationName", context.ApplicationName);
			AssertTag(result, "ServiceName", context.ServiceName);
			AssertTag(result, "BuildVersion", context.BuildVersion);
			AssertTag(result, "NodeName", context.NodeName);
			AssertTag(result, "MachineId", context.MachineId);
			AssertTag(result, "DeploymentSlice", context.DeploymentSlice);
			AssertTag(result, "IsCanary", context.IsCanary);
			AssertTag(result, "IsPrivateDeployment", context.IsPrivateDeployment);

			foreach (KeyValuePair<string, object?> tagPair in activity.TagObjects)
			{
				AssertTag(result, tagPair.Key, tagPair.Value);
			}

			foreach (KeyValuePair<string, string?> tagPair in activity.Baggage)
			{
				AssertTag(result, tagPair.Key, tagPair.Value);
			}
		}

		private static void AssertTag(MeasurementResult result, string key, object? expectedValue) => Assert.AreEqual(expectedValue, result.Tags[key]);

		private static void AssertTagNotExist(MeasurementResult result, string key) => CollectionAssert.DoesNotContain(result.Tags.Keys, key);

		private static (IExecutionContext, IHostEnvironment, IOptions<ActivityOption>) PrepareEnvironment(ActivityOption? options = null)
		{
			Activity.DefaultIdFormat = ActivityIdFormat.W3C;
			Activity.ForceDefaultIdFormat = true;

			Mock<IExecutionContext> contextMock = new();
			contextMock.Setup(e => e.RegionName).Returns("MiddleEarthRegion");
			contextMock.Setup(e => e.Cluster).Returns("TestCluster");
			contextMock.Setup(e => e.ApplicationName).Returns("TestApplication");
			contextMock.Setup(e => e.ServiceName).Returns("TestService");
			contextMock.Setup(e => e.BuildVersion).Returns("21.1.10514.10818");
			contextMock.Setup(e => e.NodeName).Returns("TestNode");
			contextMock.Setup(e => e.MachineId).Returns("TestMachineId");
			contextMock.Setup(e => e.DeploymentSlice).Returns("002");
			contextMock.Setup(e => e.IsCanary).Returns(true);
			contextMock.Setup(e => e.IsPrivateDeployment).Returns(false);
			IExecutionContext context = contextMock.Object;

			Mock<IHostEnvironment> environmentMock = new();
			environmentMock.Setup(e => e.EnvironmentName).Returns("TestEnv");
			IHostEnvironment environment = environmentMock.Object;

			Mock<IOptions<ActivityOption>> optionsMock = new();
			optionsMock.Setup(mock => mock.Value).Returns(options ?? new ActivityOption());
			IOptions<ActivityOption> activityOptions = optionsMock.Object;

			return (context, environment, activityOptions);
		}

		private static readonly string s_environmentTagName = "Environment";

		private class Listener
		{
			public Listener()
			{
				MeterListener listener = new()
				{
					InstrumentPublished = (instrument, meterListener) =>
					{
						if ((instrument.Name == OmexActivityConfiguration.ActivitiesHistogramName || instrument.Name == OmexActivityConfiguration.HealthCheckActivitiesHistogramName)
							&& instrument.Meter.Name == OmexActivityConfiguration.MeterName
							&& instrument.Meter.Version == "1.0.0")
						{
							meterListener.EnableMeasurementEvents(instrument, null);
						}
					}
				};

				Results = [];

				listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
				{
					Results.Add(new MeasurementResult(instrument, measurement, tags.ToArray().ToDictionary(p => p.Key, p => p.Value), state));
				});

				listener.Start();
			}

			public List<MeasurementResult> Results { get; set; }
		}

		private record MeasurementResult(Instrument Instrument, double Measurement, Dictionary<string, object?> Tags, object? State);
	}
}
