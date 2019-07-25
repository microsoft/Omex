// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Omex.Log.Tagger.Git
{
	public class CompileTimeTagCache : ITagCache
	{
		private readonly ITagCache m_fallBackTagCache;
		private readonly IPreBuildTagDictionary m_dictionary;

		public CompileTimeTagCache(Assembly assembly)
		{
			m_fallBackTagCache = new InMemoryTagCache();
			Type interfaceType = typeof(IPreBuildTagDictionary);
			Type dictionaryType = assembly.GetTypes().SingleOrDefault(t => interfaceType.IsAssignableFrom(t));

			if (dictionaryType == null)
			{
				throw new ArgumentException("Failed to find type that implemetns IPreBuildTagDictionary");
			}

			m_dictionary = (IPreBuildTagDictionary)Activator.CreateInstance(dictionaryType);
		}

		public string GetOrAdd(string filePath, int lineNumber, Func<(string, int), string> factory) => 
			m_dictionary?.GetTag(filePath, lineNumber) 
			?? m_fallBackTagCache.GetOrAdd(filePath, lineNumber, factory);
	}
}