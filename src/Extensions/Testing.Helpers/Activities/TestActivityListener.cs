// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Testing.Helpers.Activities
{
	/// <summary>
	/// Helpers to test ActivitySource
	/// </summary>
	public class TestActivityListener : IDisposable
	{
		private readonly ActivityListener m_listener;

		/// <summary>
		/// Creates ActivitySource that would for activity creation for specific ActivitySource
		/// </summary>
		public TestActivityListener(string? sourceName = null)
		{
			Activities = new List<Activity>();
			m_listener = new ActivityListener();
			m_listener.ShouldListenTo += source => sourceName == null || source.Name.Equals(sourceName, StringComparison.Ordinal);
			m_listener.Sample += (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded;
			m_listener.SampleUsingParentId += (ref ActivityCreationOptions<string> options) => ActivitySamplingResult.AllDataAndRecorded;
			m_listener.ActivityStarted += activity => Activities.Add(activity);
			ActivitySource.AddActivityListener(m_listener);
		}

		/// <summary>
		/// Created activities
		/// </summary>
		public List<Activity> Activities { get; }

		/// <inheritdoc/>
		public void Dispose() => m_listener.Dispose();
	}
}
