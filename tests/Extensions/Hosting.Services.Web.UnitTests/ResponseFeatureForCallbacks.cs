// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Hosting.Services.Web.UnitTests
{
	internal class ResponseFeatureForCallbacks : IHttpResponseFeature
	{
		public Stream Body { get; set; }

		public bool HasStarted { get; private set; }

		public IHeaderDictionary Headers { get; set; }

		public string ReasonPhrase { get; set; }

		public int StatusCode { get; set; }

		public ResponseFeatureForCallbacks()
		{
			Body = Stream.Null;
			HasStarted = false;
			Headers = new HeaderDictionary();
			ReasonPhrase = string.Empty;
			StatusCode = 0;
		}

		public void OnStarting(Func<object, Task> callback, object state)
		{
			m_onStartingCallback = callback;
			m_onStartingState = state;
		}

		public void OnCompleted(Func<object, Task> callback, object state)
		{
			m_onCompletedCallback = callback;
			m_onCompletedState = state;
		}

		public Task InvokeOnStarting()
		{
			HasStarted = true;
			return m_onStartingCallback != null && m_onStartingState != null
				? m_onStartingCallback(m_onStartingState)
				: Task.CompletedTask;
		}

		public Task InvokeOnCompleted()
		{
			return m_onCompletedCallback != null && m_onCompletedState != null
				? m_onCompletedCallback(m_onCompletedState)
				: Task.CompletedTask;
		}

		private Func<object, Task>? m_onStartingCallback;
		private object? m_onStartingState;
		private Func<object, Task>? m_onCompletedCallback;
		private object? m_onCompletedState;
	}
}
