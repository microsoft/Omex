// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Log.Tagger.Git
{
	public interface IPreBuildTagDictionary
	{
		string GetTag(string localPath, int index);
	}
}