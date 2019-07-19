// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Microsoft.Omex.DocumentDb
{
	/// <summary>
	/// Document client factory interface.
	/// </summary>
	public interface IDocumentClientFactory
	{
		/// <summary>
		/// Gets document client.
		/// </summary>
		/// <param name="config">Document Db settings config, containing information like region, environment, access type etc.</param>
		/// <returns>The IDocumentClient interface.</returns>
		Task<IDocumentClient> GetDocumentClientAsync(DocumentDbSettingsConfig config = null);
	}
}