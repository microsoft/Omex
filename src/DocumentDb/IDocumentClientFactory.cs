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
        /// Gets document db client.
        /// </summary>
        /// <returns>Document db client</returns>
        Task<IDocumentClient> GetDocumentClientAsync();
    }
}
