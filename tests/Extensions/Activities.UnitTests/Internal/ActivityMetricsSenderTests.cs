// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Activities.UnitTests.Internal
{
	[TestClass]
	public class ActivityMetricsSenderTests
	{
		[TestMethod]
		public void SendActivityMetric_ProduceMetrics()
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
			IExecutionContext context= contextMock.Object;

			Mock<IHostEnvironment> environmentMock = new();
			environmentMock.Setup(e => e.EnvironmentName).Returns("TestEnv");
			IHostEnvironment environment = environmentMock.Object;

			ActivityMetricsSender sender = new(context, environment);
			Listener listener = new();

			Activity testActivity = new(nameof(testActivity));
			testActivity.Start()
				.SetSubType("TestSubType")
				.SetMetadata("TestMetadata")
				.AddTag("SomeTag", "SomeTagValue")
				.AddBaggage("SomeBaggage", "SomeBaggageValue")
				.AddTag("AnotherTag", "AnotherTagValue")
				.AddBaggage("AnotherBaggage", "AnotherBaggageValue")
				.Stop();

			sender.SendActivityMetric(testActivity);
			VerifyMeasurement(listener, testActivity, context, environment, s_activityCounterName);

			Activity testHealthCheckActivity = new(nameof(testHealthCheckActivity));
			testHealthCheckActivity.Start()
				.MarkAsHealthCheck()
				.AddTag("HealthCheckTag", "TagValue")
				.AddBaggage("HealthCheckBaggage", "BaggageValue")
				.Stop();

			sender.SendActivityMetric(testHealthCheckActivity);
			VerifyMeasurement(listener, testHealthCheckActivity, context, environment, s_heathCheckActivityCounterName);
		}

		private void VerifyMeasurement(Listener listener, Activity activity, IExecutionContext context, IHostEnvironment environment, string instrumentationName)
		{
			// We need to filter by both ActivityId and Enviroment since same activity might be captured and reported by diffirent UT
			MeasurementResult result = listener.Results.Single(m => activity.Id?.Equals(m.Tags[s_activityIdTagName]) == true && environment.EnvironmentName.Equals(m.Tags[s_environmentTagName]));

			Assert.AreEqual(instrumentationName, result.Instrument.Name);
			Assert.AreEqual(activity.Duration.TotalMilliseconds, result.Measurement);
			AssertTag(result, s_activityIdTagName, activity.Id);
			AssertTag(result, "TraceId", activity.TraceId);
			AssertTag(result, "Name", activity.OperationName);
			AssertTag(result, s_environmentTagName, environment.EnvironmentName);
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

		private static readonly string s_activityIdTagName = "ActivityId";

		private static readonly string s_environmentTagName = "Environment";

		private static readonly string s_activityCounterName = "Activities";

		private static readonly string s_heathCheckActivityCounterName = "HealthCheckActivities";

		private class Listener
		{
			public Listener()
			{
				MeterListener listener = new();
				listener.InstrumentPublished = (instrument, meterListener) =>
				{
					if ((instrument.Name == s_activityCounterName || instrument.Name == s_heathCheckActivityCounterName)
						&& instrument.Meter.Name == "Microsoft.Omex.Activities"
						&& instrument.Meter.Version == "1.0.0")
					{
						meterListener.EnableMeasurementEvents(instrument, null);
					}
				};

				Results = new List<MeasurementResult>();

				listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
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
