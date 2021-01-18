// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace Microsoft.Omex.Extensions.Activities
{
	internal sealed class DefaultActivityListenerCongigurator : IActivityListenerConfigurator
	{
		private readonly IOptionsMonitor<OmexActivityListenerOptions> m_optionMonitor;

		public DefaultActivityListenerCongigurator(IOptionsMonitor<OmexActivityListenerOptions> optionMonitor) =>
			m_optionMonitor = optionMonitor;

		public ActivitySamplingResult Sample(ref ActivityCreationOptions<ActivityContext> options) => true;

		public ActivitySamplingResult SampleUsingParentId(ref ActivityCreationOptions<string> options) => ActivitySamplingResult.AllDataAndRecorded;

		public bool ShouldListenTo(ActivitySource activity) => true;
	}
}
