// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;

namespace Microsoft.Omex.Log.Tagger.Git
{
	public class InMemoryTagCache : ITagCache
	{
		private readonly ConcurrentDictionary<(string, int), string> m_cache = new ConcurrentDictionary<(string, int), string>();

		public string GetOrAdd(string filePath, int lineNumber, Func<(string, int), string> factory) => m_cache.GetOrAdd((filePath, lineNumber), factory);
	}
}