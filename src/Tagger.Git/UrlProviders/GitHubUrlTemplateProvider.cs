// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Log.Tagger.Git
{
	internal class GitHubUrlTemplateProvider : IUrlTemplateProvider
	{
		public string PathSeparator => "/";

		public bool IsApplicable(string origin) => origin.Contains("github");

		public string CreateUrlTemplate(string origin) => origin + "/blob/{0}{1}#L{2}";
	}
}