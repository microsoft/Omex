// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Activities
{
	internal class ActivityObserver : IActivityStartObserver, IActivityStopObserver
	{
		private readonly IActivitiesEventSender m_eventSender;
		private readonly ILogger<ActivityObserver> m_logger;
		private readonly ObjectPool<StringBuilder> m_stingBuilderPool;

		public ActivityObserver(IActivitiesEventSender eventSender, ILogger<ActivityObserver> logger, ObjectPoolProvider objectPoolProvider)
		{
			m_eventSender = eventSender;
			m_logger = logger;
			m_stingBuilderPool = objectPoolProvider.CreateStringBuilderPool();
		}

		public void OnStart(Activity activity, object? payload = null) =>
			m_logger.LogInformation(0x696e3739 /* tag_in79 */, $"Starting Activity {activity.OperationName} (Id:{activity.Id}; ParentId:{activity.ParentId})");

		public void OnStop(Activity activity, object? payload = null)
		{
			m_eventSender.SendActivityMetric(activity);

			bool isSuccessful = true;
			foreach (KeyValuePair<string, string?> pair in activity.Tags)
			{
				if (ActivityTagKeys.Result.Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
				{
					if (!ActivityResultStrings.Success.Equals(pair.Value, StringComparison.OrdinalIgnoreCase))
					{
						isSuccessful = false;
					}
					break;
				}
			}

			StringBuilder builder = m_stingBuilderPool.Get();

			string message = builder
				.Append("Ending Activity ").Append(activity.OperationName)
				.Append(" with ").Append(isSuccessful ? "success" : "failure")
				.Append(' ')
				.AppendObjStart()
				.AppendParamName("Id").Append(activity.Id).AppendSeparator()
				.AppendParamName("Duration").Append(activity.Duration.TotalMilliseconds).AppendSeparator()
				.AppendParamName("Baggage").AppendPairs(activity.Baggage).AppendSeparator()
				.AppendParamName("Tags").AppendPairs(activity.TagObjects).AppendSeparator()
				.AppendObjEnd()
				.ToString();

			m_stingBuilderPool.Return(builder);

			if (isSuccessful)
			{
				m_logger.LogInformation(0x696e3761 /* tag_in7a */, message);
			}
			else
			{
				m_logger.LogWarning(0x696e3762 /* tag_in7b */, message);
			}
		}
	}
}
