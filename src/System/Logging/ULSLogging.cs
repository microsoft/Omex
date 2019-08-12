// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Text;

namespace Microsoft.Omex.System.Logging
{
	/// <summary>
	/// Unified logging, allows registering for logging events sent by
	/// code implementations and to raise logging events
	/// </summary>
	public static class ULSLogging
	{

		/// <summary>
		/// Log a trace
		/// </summary>
		/// <param name="tagid">tagid (uniqueid) of the trace</param>
		/// <param name="category">logging category</param>
		/// <param name="level">logging level</param>
		/// <param name="message">message to log</param>
		/// <param name="parameters">additional parameters</param>
		public static void LogTraceTag(uint tagid, Category category, Level level, string message, params object[] parameters)
		{
			LogEvent?.Invoke(LogEventSender, new LogEventArgs(null, false, tagid, category, level, message, string.Empty, parameters));
		}


		/// <summary>
		/// Event handler for log events
		/// </summary>
		public static event EventHandler<LogEventArgs> LogEvent;


		/// <summary>
		/// Raise a log event
		/// </summary>
		/// <param name="sender">sender of event</param>
		/// <param name="e">event arguments</param>
		public static void RaiseLogEvent(object sender, LogEventArgs e)
		{
			if (e != null)
			{
				LogEvent?.Invoke(sender ?? LogEventSender, e);
			}
		}


		/// <summary>
		/// Report an exception
		/// </summary>
		/// <param name="tagid">tag</param>
		/// <param name="category">category</param>
		/// <param name="exception">exception</param>
		/// <param name="message">message</param>
		/// <param name="parameters">message format parameters</param>
		public static void ReportExceptionTag(uint tagid, Category category, Exception exception, string message,
			params object[] parameters)
		{
			LogEvent?.Invoke(LogEventSender, new ReportExceptionEventArgs(tagid, category, exception, message, parameters));
		}


		/// <summary>
		/// Optional log event sender, added as sender of event
		/// </summary>
		public static object LogEventSender { get; set; }


		// ASSERTTAG_IGNORE_FINISH
		// The preceding comment tells ULS auto-tagging to not tag above

		/// <summary>
		/// Get the Tag id as a string
		/// </summary>
		/// <param name="tagId">tag id</param>
		/// <returns>the tag as string</returns>
		/// <remarks>
		/// In terms of the conversion from integer tag value to equivalent string reprsentation, the following scheme is used:
		/// 1. If the integer tag &lt;= 0x0000FFFF, treat the tag as special tag called numeric only tag.
		/// Hence the string representation is direct conversion i.e. tag id 6700 == 6700
		/// 2. Else, if it's an alphanumeric tag, there are 2 different schemes to pack those. viz. 4 letter and 5 letter representations.
		/// 2.1 four letter tags are converted by transforming each byte into it's equivalent ASCII. e.g. 0x61626364 => abcd
		/// 2.2 five letter tags are converted by transforming lower 30 bits of the integer value into the symbol space a-z,0-9.
		/// The conversion is done by treating each group of 6 bits as an index into the symbol space a,b,c,d, ... z, 0, 1, 2, ....9
		/// eg. 0x000101D0 = 00 000000 000000 010000 000111 010000 2 = aaqhq
		/// </remarks>
		public static string TagIdAsString(uint tagId)
		{
			if (tagId <= 0xFFFF)
			{
				// Use straight numeric values
				return tagId.ToString("x4", CultureInfo.InvariantCulture);
			}
			else if (tagId <= 0x3FFFFFFF)
			{
				// Use the lower 30 bits, grouped into 6 bits each, index into
				// valuespace 'a'-'z','0'-'9' (reverse order)
				char[] chars = new char[5];
				for (int i = 4; i >= 0; i--)
				{
					uint charVal = tagId & 0x3F;
					tagId = tagId >> 6;
					if (charVal > 25)
					{
						if (charVal > 35)
						{
							chars[i] = '?';
						}
						else
						{
							chars[i] = (char)(charVal + 22);
						}
					}
					else
					{
						chars[i] = (char)(charVal + 97);
					}
				}
				return new string(chars);
			}
			else
			{
				// Each byte represented as ASCII (reverse order)
				byte[] bytes = BitConverter.GetBytes(tagId);
				char[] characters = Encoding.ASCII.GetChars(bytes);
				if (characters != null && characters.Length > 0)
				{
					Array.Reverse(characters);
					return new string(characters);
				}
			}
			return "0000";
		}
	}
}