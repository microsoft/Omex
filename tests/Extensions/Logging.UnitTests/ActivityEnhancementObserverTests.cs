// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class ActivityEnhancementObserverTests
	{
		[TestMethod]
		[Obsolete]
		public void OnStart_WhenSettingEnabled_ObsolterCorrelationIdSet()
		{
			Guid? correlation = StartAndGetCorrelation(true);
			Assert.IsNotNull(correlation);
			Assert.AreNotEqual(Guid.Empty, correlation);
		}

		[TestMethod]
		[Obsolete]
		public void OnStart_WhenSettingDisabled_ObsolterCorrelationIdNotSet()
		{
			Guid? correlation = StartAndGetCorrelation(false);
			Assert.IsNull(correlation);
		}

		[Obsolete]
		private Guid? StartAndGetCorrelation(bool addCorrelation)
		{
			IOptions<OmexLoggingOptions> options = Options.Create(new OmexLoggingOptions { AddObsoleteCorrelationToActivity = addCorrelation });
			ActivityEnhancementObserver observer = new ActivityEnhancementObserver(options);
			string name = FormattableString.Invariant($"{nameof(StartAndGetCorrelation)}|{nameof(addCorrelation)}:{addCorrelation}");
			Activity activity = new Activity(name);
			observer.OnStart(activity, null);
			return activity.GetObsoleteCorrelationId();
		}
	}
}
