// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Log.Tagger.Git
{
	internal class AzureDevOpsUrlTemplateProvider : IUrlTemplateProvider
	{
		public string PathSeparator => "%2F";

		public bool IsApplicable(string origin) => origin.Contains("visualstudio.com") || origin.Contains("dev.azure.com");

		public string CreateUrlTemplate(string origin) => origin + "?path={1}&version=GC{0}&line={2}";
	}
}