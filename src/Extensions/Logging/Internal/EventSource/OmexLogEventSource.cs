// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.Tracing;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.Omex.Extensions.Logging.Internal;

namespace Microsoft.Omex.Extensions.Logging
{
	// Renamed from Microsoft-OMEX-Logs to avoid conflict with sources in other libraries
	[EventSource(Name = "Microsoft-OMEX-Logs-Ext")] //TODO: new event source should be registered GitHub Issue #187
	internal sealed class OmexLogEventSource : BaseEventSource
	{

		public static OmexLogEventSource Instance { get; } = new OmexLogEventSource();

		private OmexLogEventSource() { }
	}
}
