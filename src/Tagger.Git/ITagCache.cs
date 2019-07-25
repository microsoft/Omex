// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Log.Tagger.Git
{
	public interface ITagCache
	{
		string GetOrAdd(string filePath, int lineNumber, Func<(string, int), string> factory);
	}
}