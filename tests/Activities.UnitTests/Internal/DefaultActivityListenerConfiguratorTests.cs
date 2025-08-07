// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Activities.UnitTests
{
	[TestClass]
	public class DefaultActivityListenerConfiguratorTests
	{
		[TestMethod]
		[DataRow(true, ActivitySamplingResult.AllData, ActivitySamplingResult.None)]
		[DataRow(false, ActivitySamplingResult.None, ActivitySamplingResult.AllData)]
		public void AddOmexActivitySource_ActivityCreationEnabled(bool shouldListen, ActivitySamplingResult sample, ActivitySamplingResult sampleUsingParent)
		{
			IOptionsMonitor<OmexActivityListenerOptions> optionsMock = CreateOptionsMonitor();
			optionsMock.CurrentValue.ShouldListenTo = shouldListen;
			optionsMock.CurrentValue.Sample = sample;
			optionsMock.CurrentValue.SampleUsingParentId = sampleUsingParent;

			IActivityListenerConfigurator configurator = new DefaultActivityListenerConfigurator(optionsMock);

			ActivityCreationOptions<ActivityContext> sampleContext = new();
			ActivityCreationOptions<string> sampleUsingParentContext = new();

			Assert.AreEqual(shouldListen, configurator.ShouldListenTo(new ActivitySource("Some")));
			Assert.AreEqual(sample, configurator.Sample(ref sampleContext));
			Assert.AreEqual(sampleUsingParent, configurator.SampleUsingParentId(ref sampleUsingParentContext));
		}

		internal static IOptionsMonitor<OmexActivityListenerOptions> CreateOptionsMonitor()
		{
			OmexActivityListenerOptions options = new();
			Mock<IOptionsMonitor<OmexActivityListenerOptions>> optionsMock = new();
			optionsMock.Setup(m => m.CurrentValue).Returns(options);

			return optionsMock.Object;
		}
	}
}
