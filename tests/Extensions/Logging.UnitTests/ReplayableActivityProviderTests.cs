// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Logging.Replayable;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class ReplayableActivityProviderTests
	{
		[TestMethod]
		public void CreatesReplayibleActivity()
		{
			Activity activity = CreateActivity(true);
			Assert.IsInstanceOfType(activity, typeof(ReplayableActivity));
		}

		[TestMethod]
		public void CreatesRegularActivity()
		{
			Activity activity = CreateActivity(false);
			Assert.IsNotInstanceOfType(activity, typeof(ReplayableActivity));
		}

		private Activity CreateActivity(bool replayEvents)
		{
			IOptions<OmexLoggingOptions> options = Options.Create(new OmexLoggingOptions { ReplayLogsInCaseOfError = replayEvents });
			ReplayableActivityProvider provider = new ReplayableActivityProvider(options);
			return provider.Create(new TimedScopeDefinition("TestName"));
		}
	}
}
