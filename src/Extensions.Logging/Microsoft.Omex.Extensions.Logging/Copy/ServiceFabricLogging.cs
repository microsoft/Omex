// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Service Fabric log processor
	/// </summary>
	public class ServiceFabricLogging : ILogProcessor
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="eventSource">The Service Fabric event source for logging</param>
		public ServiceFabricLogging(OmexLogsEventSource eventSource)
		{
			m_eventSource = Code.ExpectsArgument(eventSource, nameof(eventSource), TaggingUtilities.ReserveTag(0x2385774b /* tag_97x3l */));
		}


		/// <summary>
		/// Process a log event
		/// </summary>
		/// <param name="sender">Sender of the event</param>
		/// <param name="args">Args of the event</param>
		public void ProcessLogEvent(object sender, LogEventArgs args)
		{
			if (args == null)
			{
				// Nothing to log
				return;
			}

			if (args is ReportExceptionEventArgs exceptionArgs)
			{
				SendExceptionMessage(exceptionArgs);
				return;
			}

			SendMessage(args);
		}


		private void SendExceptionMessage(ReportExceptionEventArgs exceptionArgs)
		{
			string messageWithException = exceptionArgs.FullMessage;
			if (exceptionArgs.ExceptionData != null)
			{
				messageWithException += " Exception: " + exceptionArgs.ExceptionData.ToString();
			}

			m_eventSource.ServiceMessage(
				message: messageWithException,
				level: exceptionArgs.LevelAsString,
				category: exceptionArgs.CategoryIdAsString,
				tagId: exceptionArgs.TagIdAsString,
				threadId: exceptionArgs.ThreadId.ToString());
		}


		private void SendMessage(LogEventArgs args)
		{
			m_eventSource.ServiceMessage(
				message: args.FullMessage,
				level: args.LevelAsString,
				category: args.CategoryIdAsString,
				tagId: args.TagIdAsString,
				threadId: args.ThreadId.ToString());
		}


		/// <summary>
		/// Service Farbic logging event source
		/// </summary>
		private readonly OmexLogsEventSource m_eventSource;
	}
}
