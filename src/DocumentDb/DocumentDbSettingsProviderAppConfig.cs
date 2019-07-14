// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Configuration;
using System.Threading.Tasks;

namespace Microsoft.Omex.DocumentDb
{
	/// <summary>
	/// Document db settings provider that reads settings from app config file.
	/// </summary>
	public class DocumentDbSettingsProviderAppConfig : IDocumentDbSettingsProvider
	{
		/// <summary>
		/// Get document db settings from app config.
		/// </summary>
		/// <returns>The document db settings</returns>
		public Task<DocumentDbSettings> GetSettingsAsync()
		{
			string endpoint = ConfigurationManager.AppSettings["DocumentDbEndpoint"];
			string key = ConfigurationManager.AppSettings["DocumentDbKey"];

			return Task.FromResult(new DocumentDbSettings(endpoint, key));
		}
	}
}