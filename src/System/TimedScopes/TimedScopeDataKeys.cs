// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Base class to store the value of the keys used when writing to the timedscope ULS logs.
	/// </summary>
	public static class TimedScopeDataKeys
	{
		/// <summary>
		/// Keys used for internal telemetry framework purposes - should not be set/modified by feature code
		/// </summary>
		public static class InternalOnly
		{
			/// <summary>
			/// Name of the Host Agent
			/// </summary>
			public const string AgentName = "AgentName";

			/// <summary>
			/// Depth of call within proxying infrastructure, Depth 0 is original caller
			/// </summary>
			public const string CallDepth = "CallDepth";

			/// <summary>
			/// Number of Cpu cycles used
			/// </summary>
			public const string CpuCycles = "CpuCycles";

			/// <summary>
			/// The key value used when writing the Duration value to the ULS log.
			/// </summary>
			public const string Duration = "Duration";

			/// <summary>
			/// The key value used when writing http request count
			/// </summary>
			public const string HttpRequestCount = "HttpRequestCount";

			/// <summary>
			/// The key value used when writing the unique instance Id for a Timed Scope
			/// </summary>
			public const string InstanceId = "InstanceId";

			/// <summary>
			/// The key value used when writing the timescope IsSuccessful value to the ULS log.
			/// </summary>
			public const string IsSuccessful = "IsSuccessful";

			/// <summary>
			/// Is this a root scope in a call stack
			/// </summary>
			public const string IsRoot = "IsRoot";

			/// <summary>
			/// Number of milliseconds thread spent in kernel mode
			/// </summary>
			public const string KernelModeDuration = "KernelModeDuration";

			/// <summary>
			/// The key value used when writing the timed scope result to the ULS log.
			/// </summary>
			public const string ScopeResult = "ScopeResult";

			/// <summary>
			/// Number of milliseconds thread spent in user mode
			/// </summary>
			public const string UserModeDuration = "UserModeDuration";

			/// <summary>
			/// The key value used when writing the service call count (WCF calls)
			/// </summary>
			public const string ServiceCallCount = "ServiceCallCount";

			/// <summary>
			/// The key value used when writing the timescope failure description to the ULS log.
			/// </summary>
			public const string FailureDescription = "FailureDescription";

			/// <summary>
			/// Identity of the host machine cluster
			/// </summary>
			public const string MachineCluster = "MachineCluster";

			/// <summary>
			/// Identity of the host machine
			/// </summary>
			public const string MachineId = "MachineId";

			/// <summary>
			/// Identity of the host machine role
			/// </summary>
			public const string MachineRole = "MachineRole";

			/// <summary>
			/// ULS event level from Replay
			/// </summary>
			public const string ReplayLevel = "Level";

			/// <summary>
			/// ULS message body from Replay
			/// </summary>
			public const string ReplayMessage = "Message";

			/// <summary>
			/// ULS Tag Id from Replay
			/// </summary>
			public const string ReplayTagId = "Tag";

			/// <summary>
			/// Result/Status code used explictly by Analytics for tracking scope outcome reason
			/// </summary>
			public const string ReasonCodeForAnalytics = "ReasonCodeForAnalytics";

			/// <summary>
			/// The sequence number of the scope within its correlation
			/// </summary>
			public const string SequenceNumber = "SequenceNumber";

			/// <summary>
			/// The key value used when writing the ScopeName value to the ULS log.
			/// </summary>
			public const string ScopeName = "ScopeName";

			/// <summary>
			/// The key value used when writing the StatusCode value to the ULS log.
			/// </summary>
			public const string StatusCode = "StatusCode";

			/// <summary>
			/// A Hash that uniquely identifies the user
			/// </summary>
			public const string UserHash = "UserHash";
		}

		/// <summary>
		/// Key that can be used to store source category associated with an Object, e.g. office client type
		/// for a specified app query
		/// </summary>
		public const string Category = "Category";

		/// <summary>
		/// Key that can be used to store additional meta data about the scope.
		/// </summary>
		public const string ObjectMetaData = "ObjectMetaData";

		/// <summary>
		/// Key that can be used to store a constant string representing a Scope being used in a specific
		/// scenario context - e.g. Purchase of a Trial vs PrePaid offer
		/// </summary>
		public const string SubType = "SubType";
	}
}
