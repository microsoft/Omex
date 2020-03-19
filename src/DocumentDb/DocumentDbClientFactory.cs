﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.DocumentDb
{
	/// <summary>
	/// Document db client factory class.
	/// </summary>
	public class DocumentClientFactory : IDocumentClientFactory
	{
		/// <summary>
		/// Document db settings provider.
		/// </summary>
		private readonly IDocumentDbSettingsProvider m_DocumentDbSettingsProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="documentDbSettingsProvider">Document db settings provider.</param>
		public DocumentClientFactory(IDocumentDbSettingsProvider documentDbSettingsProvider)
		{
			Code.ExpectsArgument(documentDbSettingsProvider, nameof(documentDbSettingsProvider), TaggingUtilities.ReserveTag(0x2381b1db /* tag_961h1 */));

			m_DocumentDbSettingsProvider = documentDbSettingsProvider;
		}

		/// <summary>
		/// Gets document client.
		/// </summary>
		/// <param name="config">Document Db settings config, containing information like region, environment, access type etc.</param>
		/// <returns>The IDocumentClient interface.</returns>
		public async Task<IDocumentClient> GetDocumentClientAsync(DocumentDbSettingsConfig config = null)
		{
			DocumentDbSettings settings = await DocumentDbAdapter.ExecuteAndLogAsync(
				0, () => m_DocumentDbSettingsProvider.GetSettingsAsync(config)).ConfigureAwait(false);

			DocumentClient client = new DocumentClient(settings.Endpoint, settings.Key);

			await DocumentDbAdapter.ExecuteAndLogAsync(
			0,
			async () =>
				{
					await client.OpenAsync().ConfigureAwait(false);

					return true;
				}).ConfigureAwait(false);

			return client;
		}
	}
}
