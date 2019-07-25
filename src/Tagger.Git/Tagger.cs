// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.Omex.Log.Tagger.Git
{
	public static class Tagger
	{
		private static ITagger s_tagger;

		private static ITagger GetTagger(string pathToClonedRepo) 
			=> s_tagger ?? (s_tagger = new GitTagger(
				pathToClonedRepo, 
				new InMemoryTagCache(), 
				new IUrlTemplateProvider[] { new AzureDevOpsUrlTemplateProvider(), new GitHubUrlTemplateProvider() })
			);

		public static string GetTagUrl([CallerFilePath]string filePath = null, [CallerLineNumber]int lineNumber = 0) 
			=> GetTagger(filePath).GetUrl(filePath, lineNumber);
	}
}