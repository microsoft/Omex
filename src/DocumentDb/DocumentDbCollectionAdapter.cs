// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Omex.System.Logging;
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), TaggingUtilities.ReserveTag(0));

			Database database = await GetOrCreateDatabaseAsync(dbId).ConfigureAwait(false);

			return await GetOrCreateCollectionAsync(dbId, collectionId, partitionKey, reservedRUs).ConfigureAwait(false);
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), TaggingUtilities.ReserveTag(0));

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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsArgument(documentCollection, nameof(documentCollection), TaggingUtilities.ReserveTag(0));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0),
				() => client.CreateDocumentCollectionIfNotExistsAsync(
					UriFactory.CreateDatabaseUri(dbId),
					documentCollection,
					requestOptions)).ConfigureAwait(false);
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0),
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
				}).ConfigureAwait(false);
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			using (IDocumentQuery<DocumentCollection> query =
				client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(dbId), feedOptions).AsDocumentQuery())
			{
				return await QueryDocumentsAsync(query).ConfigureAwait(false);
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0),
				() => client.DeleteDocumentCollectionAsync(
					UriFactory.CreateDocumentCollectionUri(dbId, collectionId), requestOptions)).ConfigureAwait(false);
		}
	}
}