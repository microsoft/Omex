// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.TimedScopes;

namespace Microsoft.Omex.System.UnitTests.Shared.TimedScopes
{
	/// <summary>
	/// Represents TimedScope event for easier validating TimedScopes in UTs
	/// </summary>
	public class TimedScopeLogEvent
	{
		/// <summary>
		/// Scope name
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Scope subtype
		/// </summary>
		public string SubType { get; }

		/// <summary>
		/// Scope metadata
		/// </summary>
		public string MetaData { get; }

		/// <summary>
		/// User hash
		/// </summary>
		public string UserHash { get; }

		/// <summary>
		/// Duration
		/// </summary>
		public TimeSpan Duration { get; }

		/// <summary>
		/// Scope result
		/// </summary>
		public TimedScopeResult Result { get; }

		/// <summary>
		/// Failure Description (strongly typed)
		/// </summary>
		public Enum FailureDescriptionEnum { get; }

		/// <summary>
		/// Failure Description
		/// </summary>
		public string FailureDescription => FailureDescriptionEnum?.ToString();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Scope name</param>
		/// <param name="subtype">Subtype</param>
		/// <param name="metadata">Metadata</param>
		/// <param name="result">Scope result</param>
		/// <param name="failureDescription">Failure description</param>
		/// <param name="userHash">User hash</param>
		/// <param name="duration">Duration</param>
		public TimedScopeLogEvent(string name, string subtype, string metadata,
			TimedScopeResult result, Enum failureDescription, string userHash,
			TimeSpan duration)
		{
			Name = name;
			SubType = subtype;
			Result = result;
			MetaData = metadata;
			FailureDescriptionEnum = failureDescription;
			UserHash = userHash;
			Duration = duration;
		}
	}
}
