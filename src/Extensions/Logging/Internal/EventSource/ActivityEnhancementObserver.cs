// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Logging
{
	internal class ActivityEnhancementObserver : IActivityStartObserver
	{
		private readonly IOptions<OmexLoggingOptions> m_options;

		public ActivityEnhancementObserver(IOptions<OmexLoggingOptions> options)
		{
			m_options = options;
		}

		public void OnStart(Activity activity, object? payload)
		{
			if (m_options.Value.AddObsoleteCorrelationToActivity)
			{
#pragma warning disable CS0618 // if we are supporting old correlation we need to add it for new activities
				if (activity.GetObsoleteCorrelationId() == null)
				{
					activity.SetObsoleteCorrelationId(Guid.NewGuid());
				}
#pragma warning disable CS0618
			}
		}
	}
}
