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
		/// <returns>Document db client</returns>
		Task<DocumentDbSettings> GetSettingsAsync();
	}
}