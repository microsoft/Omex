// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;

namespace Microsoft.Omex.System.Logging
{
	/// <summary>
	/// Event arguments for a log event
	/// </summary>
	public class LogEventArgs : EventArgs
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="tagid">tagid (uniqueid) of the trace</param>
		/// <param name="category">logging category</param>
		/// <param name="level">logging level</param>
		/// <param name="message">message to log</param>
		/// <param name="parameters">additional parameters</param>
		public LogEventArgs(uint tagid, Category category, Level level, string message, params object[] parameters)
		{
			Category = category;
			Level = level;
			TagId = tagid;
			Message = message;
			MessageParameters = parameters;
		}


		/// <summary>
		/// Logging category
		/// </summary>
		public Category Category { get; }


		/// <summary>
		/// Logging level
		/// </summary>
		public Level Level { get; }


		/// <summary>
		/// Tag Id
		/// </summary>
		public uint TagId { get; }


		/// <summary>
		/// Message
		/// </summary>
		public virtual string Message { get; }


		/// <summary>
		/// Optional parameters for the message
		/// </summary>
		public object[] MessageParameters { get; }

		/// <summary>
		/// Should log directly to the underlying log handler
		/// </summary>
		public bool ShouldLogDirectly { get; }

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
						return (MessageParameters == null || MessageParameters.Length == 0) ? Message :
							string.Format(CultureInfo.InvariantCulture, Message, MessageParameters);
					}
					catch (FormatException ex)
					{
						// Explicitly not including call stack in this message.
						return string.Format(CultureInfo.InvariantCulture, "Incorrect formatting string for message: '{0}'. Exception Message: {1}.",
							Message, ex.Message);
					}
				}

				return string.Empty;
			}
		}
	}
}