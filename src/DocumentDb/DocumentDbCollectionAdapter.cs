/*****************************************************************************************************
	DocumentDbCollectionAdapter.cs

	Copyright (c) Microsoft Corporation

	Documentdb adapter for Collections.
******************************************************************************************************/

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.DocumentDb
{
    /// <summary>
    /// Document db adapter class.
    /// </summary>
    public partial class DocumentDbAdapter : IDocumentDbAdapter
    {
        /// <summary>
        /// Gets the collection if it exists, otherwise creates the collection.
        /// If the database does not exist, the method also creates it before creating the collection.
        /// </summary>
        /// <param name="dbId">Database id.</param>
        /// <param name="collectionId">Collection id.</param>
        /// <param name="partitionKey">Partition key of the document collection</param>
        /// <param name="reservedRUs">Reserved request units for the collection.</param>
        /// <returns>DocumentCollection with specified id.</returns>
        public async Task<ResourceResponse<DocumentCollection>> GetOrCreateDbAndCollectionAsync(
            string dbId, string collectionId, string partitionKey, int reservedRUs = 1000)
        {
            Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0x23854603 /* tag_97uyd */);
            Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0x23854604 /* tag_97uye */);
            Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), 0x23854605 /* tag_97uyf */);

            Database database = await GetOrCreateDatabaseAsync(dbId);
            return await GetOrCreateCollectionAsync(dbId, collectionId, partitionKey, reservedRUs);
        }


        /// <summary>
        /// Gets the DocumentCollection, otherwise creates it including the database.
        /// </summary>
        /// <param name="dbId">Database id.</param>
        /// <param name="collectionId">Collection id.</param>
        /// <param name="partitionKey">Partition key of the document collection</param>
        /// <param name="reservedRUs">Reserved request units for the collection.</param>
        /// <returns>DocumentCollection with specified id.</returns>
        public Task<ResourceResponse<DocumentCollection>> GetOrCreateCollectionAsync(
            string dbId, string collectionId, string partitionKey, int reservedRUs = 1000)
        {
            Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0x23854606 /* tag_97uyg */);
            Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0x23854607 /* tag_97uyh */);
            Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), 0x23854608 /* tag_97uyi */);

            PartitionKeyDefinition partitionKeyDefinition = new PartitionKeyDefinition();
            partitionKeyDefinition.Paths.Add(partitionKey);

            DocumentCollection documentCollection =
                new DocumentCollection
                {
                    Id = collectionId,
                    PartitionKey = partitionKeyDefinition
                };

            RequestOptions requestOptions = new RequestOptions { OfferThroughput = reservedRUs };

            return GetOrCreateCollectionAsync(dbId, documentCollection, requestOptions);
        }


        /// <summary>
        /// Creates the DocumentCollection if it does not exit otherwise returns the existing one.
        /// </summary>
        /// <param name="dbId">Database id.</param>
        /// <param name="documentCollection">Document collection.</param>
        /// <param name="requestOptions">Request options.</param>
        /// <returns>Created document collection.</returns>
        public async Task<ResourceResponse<DocumentCollection>> GetOrCreateCollectionAsync(
            string dbId, DocumentCollection documentCollection, RequestOptions requestOptions = null)
        {
            Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0x23854609 /* tag_97uyj */);
            Code.ExpectsArgument(documentCollection, nameof(documentCollection), 0x2385460a /* tag_97uyk */);

            IDocumentClient client = await GetDocumentClientAsync();

            return await DocumentDbAdapter.ExecuteAndLogAsync(0x2385460b /* tag_97uyl */,
                () => client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(dbId),
                    documentCollection,
                    requestOptions));
        }


        /// <summary>
        /// Gets the document collection.
        /// </summary>
        /// <param name="dbId">Database id.</param>
        /// <param name="collectionId">Collection id.</param>
        /// <param name="requestOptions">Request options</param>
        /// <returns>DocumentCollection with specified id.</returns>
        public async Task<ResourceResponse<DocumentCollection>> GetCollectionAsync(
            string dbId, string collectionId, RequestOptions requestOptions = null)
        {
            Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0x2385460c /* tag_97uym */);
            Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0x2385460d /* tag_97uyn */);

            IDocumentClient client = await GetDocumentClientAsync();

            return await DocumentDbAdapter.ExecuteAndLogAsync(0x2385460e /* tag_97uyo */,
                async () =>
                {
                    try
                    {
                        return await client.ReadDocumentCollectionAsync(
                            UriFactory.CreateDocumentCollectionUri(dbId, collectionId),
                            requestOptions);
                    }
                    catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
                    {
                        return default(ResourceResponse<DocumentCollection>);
                    }
                });
        }


        /// <summary>
        /// Gets the all document collections in the database.
        /// </summary>
        /// <param name="dbId">Database id.</param>
        /// <param name="feedOptions">Feed options</param>
        /// <returns>All document collections in the database.</returns>
        public async Task<IReadOnlyList<DocumentCollection>> GetAllCollectionsAsync(
            string dbId, FeedOptions feedOptions = null)
        {
            Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0x2385460f /* tag_97uyp */);

            IDocumentClient client = await GetDocumentClientAsync();

            using (IDocumentQuery<DocumentCollection> query =
                client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(dbId), feedOptions).AsDocumentQuery())
            {
                return await QueryDocumentsAsync(query);
            }
        }


        /// <summary>
        /// Deletes the document collection.
        /// </summary>
        /// <param name="dbId">Database id.</param>
        /// <param name="collectionId">Collection id.</param>
        /// <param name="requestOptions">Request options</param>
        /// <returns>Deleted DocumentCollection.</returns>
        public async Task<ResourceResponse<DocumentCollection>> DeleteCollectionAsync(
            string dbId, string collectionId, RequestOptions requestOptions = null)
        {
            Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0x23854610 /* tag_97uyq */);
            Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0x23854611 /* tag_97uyr */);

            IDocumentClient client = await GetDocumentClientAsync();

            return await DocumentDbAdapter.ExecuteAndLogAsync(0x23854612 /* tag_97uys */,
                () => client.DeleteDocumentCollectionAsync(
                    UriFactory.CreateDocumentCollectionUri(dbId, collectionId), requestOptions));
        }
    }
}
