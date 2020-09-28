// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Services.Remoting
{
	internal class MockObserver : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object?>>, IDisposable
	{
		private readonly string m_listenterName;

		private IDisposable? m_unsubscribe;

		public Dictionary<string, object?> Events { get; } = new Dictionary<string, object?>();

		public MockObserver(string listenterName)
		{
			m_listenterName = listenterName;
		}

		public void OnCompleted() { }

		public void OnError(Exception error) { }

		public void OnNext(DiagnosticListener listener)
		{
			if (string.Equals(listener.Name, m_listenterName, StringComparison.Ordinal))
			{
				listener.Subscribe(this);
			}
		}

		public void OnNext(KeyValuePair<string, object?> pair)
		{
			Events.Add(pair.Key, pair.Value);
		}

		public void AssertException(Exception exception)
		{
			KeyValuePair<string, object?> eventInfo = Events.Single(e => e.Key.EndsWith("Exception"));
			Assert.IsInstanceOfType(eventInfo.Value, typeof(Exception));
			Assert.AreEqual(exception, eventInfo.Value);
		}

		public static (DiagnosticListener, MockObserver) CreateListener([CallerMemberName] string name = "")
		{
			MockObserver mockObserver = new MockObserver(name);
			mockObserver.m_unsubscribe = DiagnosticListener.AllListeners.Subscribe(mockObserver);
			DiagnosticListener listener = new DiagnosticListener(name);
			return (listener, mockObserver);
		}

		public void Dispose() => m_unsubscribe?.Dispose();
	}
}
