// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Globalization;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.TimedScopes;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.AspNetCore
{
	/// <summary>
	/// Logs timed scopes using a custom activity
	/// </summary>
	public class CustomActivityTimedScopeLogger : ITimedScopeLogger
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="eventSource">The event source for timed scopes</param>
		/// <param name="machineInformation">Machine information</param>
		public CustomActivityTimedScopeLogger(TimedScopeEventSource eventSource, IMachineInformation machineInformation)
		{
			m_eventSource = Code.ExpectsArgument(eventSource, nameof(eventSource), TaggingUtilities.ReserveTag(0x2375d3d6 /* tag_933pw */));
			m_machineInformation = Code.ExpectsArgument(machineInformation, nameof(machineInformation), TaggingUtilities.ReserveTag(0x2375d3d7 /* tag_933px */));
		}

		/// <summary>
		/// Logs the scope start
		/// </summary>
		/// <param name="scope">Scope to log</param>
		public void LogScopeStart(TimedScope scope)
		{
		}

		/// <summary>
		/// Logs the scope end
		/// </summary>
		/// <param name="scope">Scope to log</param>
		/// <param name="data">Correlation data</param>
		public void LogScopeEnd(TimedScope scope, CorrelationData data)
		{
			if (!Code.ValidateArgument(scope, nameof(scope), TaggingUtilities.ReserveTag(0x2375d3d8 /* tag_933py */)) ||
				!Code.ValidateArgument(data, nameof(data), TaggingUtilities.ReserveTag(0x2375d3d9 /* tag_933pz */)))
			{
				return;
			}

			if (scope.IsTransaction)
			{
				m_eventSource.LogEvent(Categories.TimingGeneral,
					name: scope.Name,
					subtype: scope.SubType ?? NullPlaceholder,
					metadata: scope.MetaData ?? NullPlaceholder,
					serviceName: ServiceName ?? NullPlaceholder,
					result: scope.Result,
					correlationId: data.VisibleId.ToString("D", CultureInfo.InvariantCulture),
					durationMs: scope.DurationInMilliseconds);
			}
			else
			{
				m_eventSource.LogEvent(Categories.TimingGeneral,
					name: scope.Name,
					subtype: scope.SubType ?? NullPlaceholder,
					metadata: scope.MetaData ?? NullPlaceholder,
					serviceName: ServiceName ?? NullPlaceholder,
					userHash: data.Data(TimedScopeDataKeys.InternalOnly.UserHash) ?? data.UserHash ?? NullPlaceholder,
					result: scope.Result,
					correlationId: data.VisibleId.ToString("D", CultureInfo.InvariantCulture),
					durationMs: scope.DurationInMilliseconds);
			}
		}

		/// <summary>
		/// The service name
		/// </summary>
		private string ServiceName => m_machineInformation?.ServiceName?.ToString();

		/// <summary>
		/// Used as a dimension value for null scope dimensions
		/// </summary>
		private const string NullPlaceholder = "null";

		/// <summary>
		/// The event source for timed scopes
		/// </summary>
		private readonly TimedScopeEventSource m_eventSource;

		/// <summary>
		/// Machine information
		/// </summary>
		private readonly IMachineInformation m_machineInformation;
	}
}
