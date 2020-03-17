// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Logging
{
	internal class ActivityEnhensementObserver : IActivityStartObserver
	{
		private readonly IOptions<OmexLoggingOptions> m_options;

		public ActivityEnhensementObserver(IOptions<OmexLoggingOptions> options)
		{
			m_options = options;
		}

		public void OnStart(Activity activity)
		{
			if (m_options.Value.AddObsoleteCorrelationToActivity)
			{
#pragma warning disable CS0618
				if (activity.GetObsoleteCorrelationId() != null)
				{
					activity.SetObsoleteCorrelationId(Guid.NewGuid());
				}
#pragma warning disable CS0618
			}
		}
	}
}
