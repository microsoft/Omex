// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Log.Tagger.Git
{
	public interface IUrlTemplateProvider
	{
		bool IsApplicable(string origin);

		string CreateUrlTemplate(string origin);

		string PathSeparator { get; }
	}
}