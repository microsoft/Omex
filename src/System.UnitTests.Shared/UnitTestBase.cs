// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.UnitTests.Shared.Logging;
using Xunit;
using UntaggedLogging = Microsoft.Omex.System.Logging.ULSLogging;

namespace Microsoft.Omex.System.UnitTests.Shared
{
	/// <summary>
	/// Unit test base class
	/// </summary>
	public abstract class UnitTestBase : IDisposable
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="trackLogEvents">should log events be tracked</param>
		/// <param name="failOnErrors">should tests fail if an error is logged</param>
		protected UnitTestBase(bool trackLogEvents = true, bool failOnErrors = true)
		{
			if (UntaggedLogging.LogEventSender == null)
			{
				UntaggedLogging.LogEventSender = new LogEventSender();
			}

			LogEventSender logEventSender = ULSLogging.LogEventSender as LogEventSender;
			if (logEventSender != null)
			{
				logEventSender.Sender = this;
			}

			TrackLogEvents = trackLogEvents;
			FailOnErrors = failOnErrors;
			LoggedEvents = new List<LogEventArgs>();

			UntaggedLogging.LogEvent += HandleLogEvent;
		}


		/// <summary>
		/// Should log events be tracked
		/// </summary>
		protected bool TrackLogEvents { get; set; }


		/// <summary>
		/// Should tests fail if an error is logged
		/// </summary>
		protected bool FailOnErrors { get; set; }


		/// <summary>
		/// The set of logged events
		/// </summary>
		protected IList<LogEventArgs> LoggedEvents { get; }


		/// <summary>
		/// Dispose of the test
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Teardown test method.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			UntaggedLogging.LogEvent -= HandleLogEvent;
			if (UntaggedLogging.LogEventSender is LogEventSender logEventSender)
			{
				logEventSender.Sender = null;
			}
		}


		/// <summary>
		/// Handle a log event
		/// </summary>
		/// <param name="sender">sender of event</param>
		/// <param name="e">event arguments</param>
		protected virtual void HandleLogEvent(object sender, LogEventArgs e)
		{
			if (!TrackLogEvents)
			{
				return;
			}

			if (UntaggedLogging.LogEventSender is LogEventSender logEventSender && logEventSender.Sender == this)
			{
				// This is an event we should care about as it is sent from this
				// type.
				LoggedEvents.Add(e);
				if (FailOnErrors)
				{
					Assert.NotEqual(Levels.Error, e.Level);
				}
			}
		}


		/// <summary>
		/// Verify the async function
		/// </summary>
		/// <param name="function">function containing test to verify</param>
		protected void VerifyAsync(Func<Task> function)
		{
			Task.Run(async () =>
			{
				await function();
			}).GetAwaiter().GetResult();
		}
	}
}