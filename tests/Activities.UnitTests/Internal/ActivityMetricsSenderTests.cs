// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Microsoft.Extensions.Configuration;
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
		[DataRow("")]
		[DataRow(null!)]
		public void ActivityMetricsSender_Construction_NonExistConfigSection_ReturnNotNull(string nonExistValue)
		{
			Mock<IExecutionContext> contextMock = new();
			Mock<IHostEnvironment> environmentMock = new();
			Mock<IConfiguration> mockConfig = new();
			Mock<IConfigurationSection> mockConfigSection = new();

			mockConfig.Setup(configuration => configuration.GetSection(ActivityMetricsSender.UseHistogramForActivityMonitoringConfigurationPath))
				.Returns(mockConfigSection.Object);
			mockConfigSection.Setup(section => section.Value).Returns(nonExistValue);

			ActivityMetricsSender sender = new(contextMock.Object, environmentMock.Object, mockConfig.Object);
			Assert.IsNotNull(sender);
		}

		[TestMethod]
		[DataRow("true")]
		[DataRow("false")]
		public void ActivityMetricsSender_Construction_TrueAndFalseParsableConfig_ReturnNotNull(string parsableConfigValue)
		{
			Mock<IExecutionContext> contextMock = new();
			Mock<IHostEnvironment> environmentMock = new();
			Mock<IConfiguration> mockConfig = new();
			Mock<IConfigurationSection> mockConfigSection = new();

			mockConfig.Setup(configuration => configuration.GetSection(ActivityMetricsSender.UseHistogramForActivityMonitoringConfigurationPath))
				.Returns(mockConfigSection.Object);
			mockConfigSection.Setup(section => section.Value).Returns(parsableConfigValue);

			ActivityMetricsSender sender = new(contextMock.Object, environmentMock.Object, mockConfig.Object);
			Assert.IsNotNull(sender);
		}

		[TestMethod]
		[DataRow("Not boolean passable")]
		[DataRow("truee")]
		[DataRow("falsee")]
		[DataRow("T")]
		[DataRow("F")]
		[DataRow("0")]
		[DataRow("1")]
		public void ActivityMetricsSender_Construction_NotParsableConfigValue_ThrowArgumentException(string notParsableConfigValue)
		{
			Mock<IExecutionContext> contextMock = new();
			Mock<IHostEnvironment> environmentMock = new();
			Mock<IConfiguration> mockConfig = new();
			Mock<IConfigurationSection> mockConfigSection = new();

			mockConfig.Setup(configuration => configuration.GetSection(ActivityMetricsSender.UseHistogramForActivityMonitoringConfigurationPath))
				.Returns(mockConfigSection.Object);
			mockConfigSection.Setup(section => section.Value).Returns(notParsableConfigValue);

			Assert.ThrowsException<ArgumentException>(() =>
			{
				new ActivityMetricsSender(contextMock.Object, environmentMock.Object, mockConfig.Object);
			});
		}

		[TestMethod]
		[DataRow("true")]	// Means Histogram instrument is used
		[DataRow("false")]	// Means Counter instrument is used
		public void SendActivityMetric_ProduceMetrics(string useHistogramForActivity)
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

			Mock<IConfiguration> mockConfiguration = new();
			Mock<IConfigurationSection> mockConfigSection = new();
			mockConfiguration.Setup(configuration => configuration.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);
			mockConfigSection.Setup(section => section.Value).Returns(useHistogramForActivity);

			ActivityMetricsSender sender = new(context, environment, mockConfiguration.Object);

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
			VerifyTagsExist(listener, testActivity, false, context, environment, s_activityCounterName);

			Activity testHealthCheckActivity = new(nameof(testHealthCheckActivity));
			testHealthCheckActivity.Start()
				.MarkAsHealthCheck()
				.AddTag(s_healthCheckTag, "TagValue")
				.AddBaggage("HealthCheckBaggage", "BaggageValue")
				.Stop();

			sender.SendActivityMetric(testHealthCheckActivity);
			VerifyTagsExist(listener, testHealthCheckActivity, true, context, environment, s_heathCheckActivityCounterName);
		}

		private void VerifyTagsExist(Listener listener, Activity activity, bool isHealthCheck, IExecutionContext context, IHostEnvironment environment, string instrumentationName)
		{
			MeasurementResult result = isHealthCheck ?
				listener.Results.First(m => m.Tags.ContainsKey(s_healthCheckTag) && environment.EnvironmentName.Equals(m.Tags[s_environmentTagName]))
				: listener.Results.First(m => !m.Tags.ContainsKey(s_healthCheckTag) && environment.EnvironmentName.Equals(m.Tags[s_environmentTagName]));

			Assert.AreEqual(instrumentationName, result.Instrument.Name);
			Assert.AreEqual(activity.Duration.TotalMilliseconds, result.Measurement);
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

		private static readonly string s_healthCheckTag = "HealthCheckTag";

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
