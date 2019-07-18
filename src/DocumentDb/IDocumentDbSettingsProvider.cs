// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.Omex.DocumentDb
{
	/// <summary>
	/// Document db settings provider.
	/// </summary>
	public interface IDocumentDbSettingsProvider
	{
		/// <summary>
		/// Gets document db settings.
		/// </summary>
		/// <param name="config">Document Db settings config, containing information like region, environment, access type etc.</param>
		/// <returns>Document db settings</returns>
		Task<DocumentDbSettings> GetSettingsAsync(DocumentDbSettingsConfig config = null);
	}
}