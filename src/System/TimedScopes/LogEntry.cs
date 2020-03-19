// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Data Contract for a single ULS Log Entry
	/// </summary>
	[DataContract]
	[Serializable]
	public sealed class LogEntry
	{
		/// <summary>
		/// Logging category name
		/// </summary>
		[DataMember]
		public string CategoryName { get; set; }

		/// <summary>
		/// CorrelationId this entry is part of
		/// </summary>
		[DataMember]
		public Guid CorrelationId { get; set; }

		/// <summary>
		/// ULS Message
		/// </summary>
		[DataMember]
		public string Message { get; set; }

		/// <summary>
		/// ULS TagId in numeric format
		/// </summary>
		[DataMember]
		public uint NumericTagId { get; set; }

		/// <summary>
		/// Unique Sequence Number for this event in the context of the host service type
		/// </summary>
		[DataMember]
		public long SequenceNumber { get; set; }

		/// <summary>
		/// Server Timestamp (Stopwatch.GetTimestamp) when event occurred
		/// </summary>
		[DataMember]
		public long ServerTimestamp { get; set; }

		/// <summary>
		/// Server UTC Time when event occurred
		/// </summary>
		[DataMember]
		public DateTime ServerTimeUtc { get; set; }

		/// <summary>
		/// ULS TagId in string format
		/// </summary>
		[DataMember]
		public string TagId { get; set; }

		/// <summary>
		/// ULS trace level
		/// </summary>
		[DataMember]
		public string TraceLevel { get; set; }
	}
}
