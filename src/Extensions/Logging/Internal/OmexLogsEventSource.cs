// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Logging
{
	[EventSource(Name = "Microsoft-OMEX-Logs")]
	internal sealed class OmexLogsEventSource : EventSource
	{
		static OmexLogsEventSource()
		{
			Process process = Process.GetCurrentProcess();
			ProcessName = string.Format("{0} (0x{1:X4})", process.ProcessName, process.Id);
		}


		private static string ProcessName { get; }


		public OmexLogsEventSource(IMachineInformation machineInformation, IServiceContext context)
		{
			m_machineInformation = machineInformation;
			m_serviceContext = context;
		}


		private readonly IMachineInformation m_machineInformation;
		private readonly IServiceContext m_serviceContext;

		private string ApplicationName => m_machineInformation.MachineRole;
		private string ServiceName => m_machineInformation.ServiceName;
		private string AgentName => m_machineInformation.AgentName;
		private string BuildVersion => m_machineInformation.BuildVersion;

		private Guid PartitionId => m_serviceContext?.PartitionId ?? Guid.Empty;
		private long ReplicaId => m_serviceContext?.ReplicaOrInstanceId ?? 0;

		private Guid CorrelationId => Activity.Current?.VisibleId ?? Guid.Empty;
		private uint TransactionId => Activity.Current?.TransactionId ?? 0u;


		[NonEvent]
		public void ServiceMessage(string message, LogLevel level, string category, EventId eventId, int threadId)
		{
			if (!IsEnabled())
			{
				return;
			}

			// Both converts should be done more efficient
			string tagId = TagIdAsString(eventId.Id);
			string threadIdAsString = threadId.ToString(); 

			switch (level)
			{
				case LogLevel.Trace:
				case LogLevel.Debug:
					LogVerboseServiceMessage(message, category, tagId, threadIdAsString);
					break;
				case LogLevel.Information:
					LogInfoServiceMessage(message, category, tagId, threadIdAsString);
					break;
				case LogLevel.Warning:
					LogWarningServiceMessage(message, category, tagId, threadIdAsString);
					break;
				case LogLevel.Error:
					LogErrorServiceMessage(message, category, tagId, threadIdAsString);
					break;
				case LogLevel.Critical:
					// here spam correspondes to EWT LogAlways that's highest level
					LogSpamServiceMessage(message, category, tagId, threadIdAsString);
					break;
				case LogLevel.None:
					break;
				default:
					throw new ArgumentException("Unknown EventLevel: " + level);
			}
		}


		[Event((int)EventIds.LogInfoEventId, Level = EventLevel.Informational, Message = "{13}", Version = 6)]
		private void LogInfoServiceMessage(
			string category,
			string tagId,
			string threadId,
			string message) =>
			WriteEvent((int)EventIds.LogInfoEventId, ApplicationName, ServiceName, AgentName, BuildVersion, ProcessName, PartitionId, ReplicaId, CorrelationId, TransactionId, "Info", category, tagId, threadId, message);


		[Event((int)EventIds.LogErrorEventId, Level = EventLevel.Error, Message = "{13}", Version = 6)]
		private void LogErrorServiceMessage(
			string category,
			string tagId,
			string threadId,
			string message) =>
			WriteEvent((int)EventIds.LogErrorEventId, ApplicationName, ServiceName, AgentName, BuildVersion, ProcessName, PartitionId, ReplicaId, CorrelationId, TransactionId, "Error", category, tagId, threadId, message);


		[Event((int)EventIds.LogWarningEventId, Level = EventLevel.Warning, Message = "{13}", Version = 6)]
		private void LogWarningServiceMessage(
			string category,
			string tagId,
			string threadId,
			string message) =>
			WriteEvent((int)EventIds.LogWarningEventId, ApplicationName, ServiceName, AgentName, BuildVersion, ProcessName, PartitionId, ReplicaId, CorrelationId, TransactionId, "Warning", category, tagId, threadId, message);


		[Event((int)EventIds.LogVerboseEventId, Level = EventLevel.Verbose, Message = "{13}", Version = 6)]
		private void LogVerboseServiceMessage(
			string category,
			string tagId,
			string threadId,
			string message) =>
			WriteEvent((int)EventIds.LogVerboseEventId, ApplicationName, ServiceName, AgentName, BuildVersion, ProcessName, PartitionId, ReplicaId, CorrelationId, TransactionId, "Verbose", category, tagId, threadId, message);


		[Event((int)EventIds.LogSpamEventId, Level = EventLevel.LogAlways, Message = "{13}", Version = 6)]
		private void LogSpamServiceMessage(
			string category,
			string tagId,
			string threadId,
			string message) =>
			WriteEvent((int)EventIds.LogSpamEventId, ApplicationName, ServiceName, AgentName, BuildVersion, ProcessName, PartitionId, ReplicaId, CorrelationId, TransactionId, "Spam", category, tagId, threadId, message);


		/// <summary>
		/// Get the Tag id as a string
		/// </summary>
		/// <param name="tagId">tag id</param>
		/// <returns>the tag as string</returns>
		/// <owner alias="davidah"/>
		/// <remarks>
		/// In terms of the conversion from integer tag value to equivalent string reprsentation, the following scheme is used:
		/// 1. If the integer tag &lt;= 0x0000FFFF, treat the tag as special tag called numeric only tag.
		/// Hence the string representation is direct conversion i.e. tag id 6700 == 6700
		/// 2. Else, if it's an alphanumeric tag, there are 2 different schemes to pack those. viz. 4 letter and 5 letter representations.
		/// 2.1 four letter tags are converted by transforming each byte into it's equivalent ASCII. e.g. 0x61626364 => abcd
		/// 2.2 five letter tags are converted by transforming lower 30 bits of the integer value into the symbol space a-z,0-9.
		/// The conversion is done by treating each group of 6 bits as an index into the symbol space a,b,c,d, ... z, 0, 1, 2, ....9
		/// eg. 0x000101D0 = 00 000000 000000 010000 000111 010000 2 = aaqhq
		/// (from http://office/15/howto/reliability/Wiki/Assert%20and%20ULS%20Tagging.aspx)
		/// </remarks>
		public static string TagIdAsString(int tagId)
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
