// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Microsoft.Omex.Extensions.Testing.Helpers
{
	/// <summary>
	/// Test event listener for validating event sources
	/// </summary>
	public class TestEventListener : EventListener
	{
		/// <summary>
		/// Events reserved
		/// </summary>
		public List<EventWrittenEventArgs> EventsInformation { get; } = new List<EventWrittenEventArgs>();

		/// <inheritdoc/>
		protected override void OnEventWritten(EventWrittenEventArgs eventData) => EventsInformation.Add(eventData);
	}
}
