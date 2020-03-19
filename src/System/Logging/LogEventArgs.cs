// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Microsoft.Omex.System.Logging
{
	/// <summary>
	/// Event arguments used with log events. Base class for all logging events.
	/// Uses non-domain specific category id, trace level and event severity for
	/// compatibility.
	/// </summary>
	public class LogEventArgs : EventArgs
	{
		/// <summary>
		/// Capacity of parameters
		/// </summary>
		private const int ParameterCapacity = 64;

		/// <summary>
		/// Tag id
		/// </summary>
		public uint TagId { get; }

		/// <summary>
		/// Get the Tag id as a string
		/// </summary>
		public string TagIdAsString => ULSLogging.TagIdAsString(TagId);

		/// <summary>
		/// Category id
		/// </summary>
		public Category CategoryId { get; }

		/// <summary>
		/// Thread id
		/// </summary>
		public int ThreadId { get; }

		/// <summary>
		/// Get the category id as a string
		/// </summary>
		public virtual string CategoryIdAsString => CategoryId.Name;

		/// <summary>
		/// Message
		/// </summary>
		public virtual string Message { get; }

		/// <summary>
		/// Optional parameters for the message
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Explicitly need an array to match underlying logging.")]
		public object[] MessageParameters { get; }

		/// <summary>
		/// Optional details string in addition to the message/message parameters value
		/// </summary>
		public string Details { get; }

		/// <summary>
		/// The fully formatted message
		/// </summary>
		public string FullMessage
		{
			get
			{
				if (Message != null)
				{
					try
					{
						string message = (MessageParameters == null || MessageParameters.Length == 0) ? Message :
							string.Format(CultureInfo.InvariantCulture, Message, MessageParameters);

						return Details == null ? message : string.Concat(Details, message);
					}
					catch (FormatException ex)
					{
						// Explicitly not including call stack in this message.
						return string.Format(CultureInfo.InvariantCulture, "Incorrect string: '{0}'. Parameters: '{1}'. Exception Message: {2}.",
							Message, Join(MessageParameters), ex.Message);
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Join the message parameters into a string
		/// </summary>
		/// <param name="parameters">parameters</param>
		/// <returns>string representing the parameters, as in 'X', 'Y'</returns>
		private static string Join(object[] parameters)
		{
			if (parameters == null || parameters.Length == 0)
			{
				return string.Empty;
			}

			StringBuilder builder = new StringBuilder(ParameterCapacity);
			foreach (object param in parameters)
			{
				builder.AppendFormat(CultureInfo.InvariantCulture, "{0}, ",
					param == null ? string.Empty : param.ToString());
			}

			return builder.ToString();
		}

		/// <summary>
		/// Trace level
		/// </summary>
		public Level Level { get; }

		/// <summary>
		/// Get the trace level as a string
		/// </summary>
		public virtual string LevelAsString => Level.ToString();

		/// <summary>
		/// Should log directly to the underlying log handler
		/// </summary>
		public bool ShouldLogDirectly { get; }

		/// <summary>
		/// Unique Sequence Number for this event within the context of the correlation
		/// </summary>
		public long SequenceNumber { get; }

		/// <summary>
		/// Server Timestamp (Stopwatch.GetTimestamp) when event occurred
		/// </summary>
		public long ServerTimestamp { get; }

		/// <summary>
		/// Server UTC Time when event occurred
		/// </summary>
		public DateTime ServerTimeUtc { get; }

		/// <summary>
		/// Construct the event arguments
		/// </summary>
		/// <param name="shouldLogDirectly">should log directly to underlying log handler</param>
		/// <param name="tagId">tag id</param>
		/// <param name="categoryId">category id</param>
		/// <param name="traceLevel">trace level</param>
		/// <param name="message">message</param>
		/// <param name="details">additional details to include in message which is not part of message formatting and parameters</param>
		/// <param name="parameters">parameters for the message</param>
		public LogEventArgs(bool shouldLogDirectly, uint tagId, Category categoryId, Level traceLevel, string message, string details, params object[] parameters)
		{
			TagId = tagId;
			CategoryId = categoryId;
			ThreadId = Thread.CurrentThread.ManagedThreadId;
			Details = details;
			Level = traceLevel;
			Message = message;
			MessageParameters = parameters;
			ServerTimestamp = Stopwatch.GetTimestamp();
			ServerTimeUtc = DateTime.UtcNow;
			ShouldLogDirectly = shouldLogDirectly;
			SequenceNumber = -1;
		}
	}
}
