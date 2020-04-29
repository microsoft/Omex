// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SfHealthInformation = System.Fabric.Health.HealthInformation;
using SfHealthState = System.Fabric.Health.HealthState;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class ServiceFabricHealthCheckPublisherTests
	{
		[TestMethod]
		public void PublishAsync_ReportsToServiceFabric()
		{
			string healthyCheckName = "HealthyCheck";
			string degradedCheckName = "DegradedCheck";
			string unhealthyCheckName = "UnhealthyCheck";

			MockServicePartition partition = new MockServicePartition();

			HealthReport report = new HealthReport(new Dictionary<string, HealthReportEntry>()
			{
				{ healthyCheckName, CreateEntry(HealthStatus.Healthy, "testDescription1") },
				{ degradedCheckName, CreateEntry(HealthStatus.Degraded, "testDescription2", new ArithmeticException()) },
				{ unhealthyCheckName, CreateEntry(HealthStatus.Unhealthy, "testDescription3", new ArgumentOutOfRangeException() ) }
			}, default);

			Accessor<IServicePartition> accessor = new Accessor<IServicePartition>();
			IHealthCheckPublisher publisher = new ServiceFabricHealthCheckPublisher(accessor);

			publisher.PublishAsync(report, default); // publish not failing if IServicePartition not available

			(accessor as IAccessorSetter<IServicePartition>).SetValue(partition);

			publisher.PublishAsync(report, default);

			AssertHealtItem(partition, healthyCheckName, report);
			AssertHealtItem(partition, degradedCheckName, report);
			AssertHealtItem(partition, degradedCheckName, report);
		}

		private void AssertHealtItem(MockServicePartition partition, string name, HealthReport report)
		{
			HealthReportEntry entry = report.Entries[name];
			SfHealthInformation? info = partition.HealthInformation.SingleOrDefault(h => string.Equals(h.Property, name, StringComparison.OrdinalIgnoreCase));
			Assert.IsNotNull(info, name);

			Assert.AreEqual(GetMachingState(entry.Status), info.HealthState, nameof(HealthReportEntry.Status));

			if (entry.Status == HealthStatus.Healthy)
			{
				Assert.AreEqual(info.Description, entry.Description, nameof(HealthReportEntry.Description));
			}
			else
			{
				StringAssert.Contains(info.Description, entry.Description, nameof(HealthReportEntry.Description));
				StringAssert.Contains(info.Description, entry.Exception.ToString(), nameof(HealthReportEntry.Exception));
				StringAssert.Contains(info.Description, entry.Duration.ToString(), nameof(HealthReportEntry.Duration));

				int index = 0;
				foreach (string tag in entry.Tags)
				{
					StringAssert.Contains(info.Description, tag, nameof(HealthReportEntry.Tags) + index);
					index++;
				}
			}
		}

		private SfHealthState GetMachingState(HealthStatus state) =>
			state switch
			{
				HealthStatus.Healthy => SfHealthState.Ok,
				HealthStatus.Degraded => SfHealthState.Warning,
				HealthStatus.Unhealthy => SfHealthState.Error,
				_ => SfHealthState.Invalid,
			};

		private HealthReportEntry CreateEntry(HealthStatus status, string description, Exception? exception = null) =>
			new HealthReportEntry(
				status,
				description,
				TimeSpan.FromMinutes(1),
				exception,
				new Dictionary<string, object>());

		private class MockServicePartition : IServicePartition
		{
			public List<SfHealthInformation> HealthInformation { get; } = new List<SfHealthInformation>();

			public ServicePartitionInformation PartitionInfo => throw new NotImplementedException();

			public void ReportFault(FaultType faultType) => throw new NotImplementedException();

			public void ReportLoad(IEnumerable<LoadMetric> metrics) => throw new NotImplementedException();

			public void ReportMoveCost(MoveCost moveCost) => throw new NotImplementedException();

			public void ReportPartitionHealth(SfHealthInformation healthInfo) => HealthInformation.Add(healthInfo);

			public void ReportPartitionHealth(SfHealthInformation healthInfo, System.Fabric.Health.HealthReportSendOptions sendOptions) =>
				ReportPartitionHealth(healthInfo);
		}
	}
}
