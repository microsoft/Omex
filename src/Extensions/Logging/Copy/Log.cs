/*
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using Microsoft.Office.Web.OfficeMarketplace.Shared.Extensions;
using Microsoft.Omex.Gating;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Log class, deals with logging to the underlying system
	/// </summary>
	public class Log
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logProcessor">log processor, handles the logging events</param>
		/// <param name="correlation">correlation management instance</param>
		/// <param name="logEventCache">Log event cache</param>
		/// <owner alias="rgrimes"/>
		public Log(ILogProcessor logProcessor, Correlation correlation, ILogEventCache logEventCache)
		{
			// NOTE: can't use Code.ExpectsArgument below as it would cause reentrancy

			if (correlation == null)
			{
				throw new ArgumentNullException(nameof(correlation));
			}

			if (logProcessor == null)
			{
				throw new ArgumentNullException(nameof(logProcessor));
			}

			if (logEventCache == null)
			{
				throw new ArgumentNullException(nameof(logEventCache));
			}

			Correlation = correlation;
			LogProcessor = logProcessor;
			LogEventCache = logEventCache;
		}


		/// <summary>
		/// Log processor, handles the logging events
		/// </summary>
		private ILogProcessor LogProcessor { get; }


		/// <summary>
		/// Correlation management instance
		/// </summary>
		protected Correlation Correlation { get; }


		/// <summary>
		/// Log event cache.
		/// </summary>
		private ILogEventCache LogEventCache { get; }


		/// <summary>
		/// Event handler for logging events
		/// </summary>
		public event EventHandler<LogEventArgs> Handler;


		// The following comment tells ULS auto-tagging to not tag below
		// ASSERTTAG_IGNORE_START


		/// <summary>
		/// Log a code error. This raises a ShipAssert event
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">category</param>
		/// <param name="skipLog">should the logging be skipped</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		/// <owner alias="davidah"/>
		public void LogCodeErrorTag(uint tagid, UlsCategory category, bool skipLog,
			string message, params object[] parameters)
		{
			if (skipLog)
			{
				return;
			}

			RaiseLogEvent(new ShipAssertEventArgs(Correlation.CurrentCorrelation,
				Correlation.ShouldLogDirectly,
				tagid, category, false, message, GetRequestDetails(), parameters));
		}


		/// <summary>
		/// Report an exception
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">category</param>
		/// <param name="exception">exception</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		/// <owner alias="davidah"/>
		public void ReportExceptionTag(uint tagid, UlsCategory category, Exception exception, string message,
			params object[] parameters)
		{
			RaiseLogEvent(new ReportExceptionEventArgs(
				Correlation.CurrentCorrelation,
				Correlation.ShouldLogDirectly,
				tagid, category, exception, message, GetGateIds(), parameters));
			if (exception.IsFatalException())
			{
				throw exception;
			}
		}


		/// <summary>
		/// Log system status. This raises a LogEvent of level 'Info'
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">category</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		public void LogSystemStatusTag(uint tagid, UlsCategory category, string message,
			params object[] parameters)
		{
			RaiseLogEvent(new LogEventArgs(
				Correlation.CurrentCorrelation,
				Correlation.ShouldLogDirectly,
				tagid, category, Levels.Info, message, GetGateIds(), parameters));
		}


		/// <summary>
		/// Log trace tag. This raises a LogEvent of a specified level.
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">category</param>
		/// <param name="level">level to log at</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		public void LogTraceTag(uint tagid, UlsCategory category, Level level, string message,
			params object[] parameters)
		{
			RaiseLogEvent(new LogEventArgs(
				Correlation.CurrentCorrelation,
				Correlation.ShouldLogDirectly,
				tagid, category, level, message, GetGateIds(), parameters));
		}


		/// <summary>
		/// Log a verbose message for debugging / informational purposes. This raises a
		/// LogEvent of level 'Verbose'
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">category</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		/// <owner alias="davidah"/>
		public void LogVerboseTag(uint tagid, UlsCategory category, string message,
			params object[] parameters)
		{
			RaiseLogEvent(new LogEventArgs(
				Correlation.CurrentCorrelation,
				Correlation.ShouldLogDirectly,
				tagid, category, Levels.Verbose, message, GetGateIds(), parameters));
		}


		/// <summary>
		/// Log a content error. This raises a LogEvent of level 'Warning'
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">logging category</param>
		/// <param name="requestDetails">Details associated with the content request</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		public void LogContentErrorTag(uint tagid, UlsCategory category, string requestDetails,
			string message, params object[] parameters)
		{
			RaiseLogEvent(new LogEventArgs(
				Correlation.CurrentCorrelation,
				Correlation.ShouldLogDirectly,
				tagid, category, Levels.Warning, message, requestDetails, parameters));
		}

		// ASSERTTAG_IGNORE_FINISH
		// The preceding comment tells ULS auto-tagging to not tag above


		/// <summary>
		/// Transaction monitor
		/// </summary>
		private TransactionMonitor m_transationMonitor;


		/// <summary>
		/// Transaction monitor
		/// </summary>
		/// <owner alias="davidah"/>
		public TransactionMonitor TransactionMonitor
		{
			get
			{
				TransactionMonitor monitor = m_transationMonitor;
				if (monitor == null)
				{
					monitor = new TransactionMonitor(Correlation, this);
					m_transationMonitor = monitor;
				}
				return monitor;
			}
		}


		/// <summary>
		/// Raise the logging event
		/// </summary>
		/// <param name="args">arguments</param>
		/// <owner alias="davidah"/>
		public void RaiseLogEvent(LogEventArgs args)
		{
			if (args == null)
			{
				return;
			}

			if (!args.ShouldLogDirectly)
			{
				EventHandler<LogEventArgs> handler = Handler;
				if (handler != null)
				{
					handler(this, args);
				}
			}

			LogProcessor.ProcessLogEvent(this, args);
			LogEventCache.AddLogEvent(args);
		}


		/// <summary>
		/// Add request details to the message
		/// </summary>
		/// <returns>modified message</returns>
		private static string GetRequestDetails()
		{
			return GetGateIds();
		}


		/// <summary>
		/// Add a list of active gate id's (if any)
		/// </summary>
		/// <returns>gates information</returns>
		/// <owner alias="davidah"/>
		protected static string GetGateIds()
		{
			IGateContext context;
			bool hasGateContext = CrossCuttingConcerns.InstanceContainer.DoesTypeExist(out context);
			if (hasGateContext && context != null)
			{
				string gateIds = context.CurrentGatesAsString(", ");
				if (!string.IsNullOrWhiteSpace(gateIds))
				{
					return string.Format(CultureInfo.InvariantCulture, "Gates: '{0}'. ", gateIds);
				}
			}
			return string.Empty;
		}
	}
}
*/
