// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Logging.Replayable;

namespace Microsoft.Omex.Extensions.Logging
{
	internal sealed class OmexLogEventSender : ILogEventSender, ILogEventReplayer
	{
		static OmexLogEventSender()
		{
			Process process = Process.GetCurrentProcess();
			s_processName = string.Format(CultureInfo.InvariantCulture, "{0} (0x{1:X4})", process.ProcessName, process.Id);
		}


		public OmexLogEventSender(OmexLogEventSource eventSource, IMachineInformation machineInformation, IServiceContext context)
		{
			m_eventSource = eventSource;
			m_machineInformation = machineInformation;
			m_serviceContext = context;
		}


		public void LogMessage(string activityId, ActivityTraceId traceId, string category, LogLevel level, EventId eventId, int threadId, string message)
		{
			if (!IsEnabled(level))
			{
				return;
			}

			Guid partitionId = m_serviceContext.PartitionId;
			long replicaId = m_serviceContext.ReplicaOrInstanceId;
			string applicationName = m_machineInformation.MachineRole;
			string serviceName = m_machineInformation.ServiceName;
			string buildVersion = m_machineInformation.BuildVersion;
			string machineId = m_machineInformation.MachineId;

			string tagName = eventId.Name;
			// In case if tag created using Tag.Create (line number and file in description) it's better to display decimal number 
			string tagId = string.IsNullOrEmpty(eventId.Name) ? TagIdAsString(eventId.Id) : eventId.Id.ToString(CultureInfo.InvariantCulture);
			string traceIdAsString = traceId.ToHexString();

			//Event methods should have all information as parameters so we are passing them each time
			// Posible Breaking changes:
			// 1. CorrelationId type changed from Guid ?? Guid.Empty
			// 2. TransactionId type Changed from uint ?? 0u
			// 3. ThreadId type Changed from string
			// 4. TagName to events so it should be also send
			switch (level)
			{
				case LogLevel.None:
					break;
				case LogLevel.Trace:
					m_eventSource.LogSpamServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId, activityId, traceIdAsString, "Spam", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Debug:
					m_eventSource.LogVerboseServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId, activityId, traceIdAsString, "Verbose", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Information:
					m_eventSource.LogInfoServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId, activityId, traceIdAsString, "Info", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Warning:
					m_eventSource.LogWarningServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId, activityId, traceIdAsString, "Warning", category, tagId, tagName, threadId, message);
					break;
				case LogLevel.Error:
				case LogLevel.Critical:
					m_eventSource.LogErrorServiceMessage(applicationName, serviceName, machineId, buildVersion, s_processName, partitionId, replicaId, activityId, traceIdAsString, "Error", category, tagId, eventId.Name, threadId, message);
					break;
				default:
					throw new ArgumentException("Unknown EventLevel: " + level);
			}
		}


		public bool IsEnabled(LogLevel level) =>
			level switch
			{
				LogLevel.None => false,
				_ => m_eventSource.IsEnabled()
			};


		public bool IsReplayableMessage(LogLevel logLevel) =>
			logLevel switch
			{
				LogLevel.Trace => true,
				LogLevel.Debug => true,
				_ => false
			};


		public void ReplayLogs(Activity activity)
		{
			if (activity is ReplayableActivity replayableActivity)
			{
				foreach (LogMessageInformation log in replayableActivity.GetLogEvents())
				{
					LogMessage(activity.Id, activity.TraceId, log.Category, LogLevel.Information, log.EventId, log.ThreadId, log.Message);
				}
			}
		}


		private readonly OmexLogEventSource m_eventSource;
		private readonly IServiceContext m_serviceContext;
		private readonly IMachineInformation m_machineInformation;
		private static readonly string s_processName;


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
		private static string TagIdAsString(int tagId) // Convert should be done more efficient
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
					int charVal = tagId & 0x3F;
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
				if (characters.Length > 0)
				{
					Array.Reverse(characters);
					return new string(characters);
				}
			}

			return "0000";
		}
	}
}
