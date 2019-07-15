// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
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
		/// Creates a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="document">Object to create.</param>
		/// <param name="requestOptions">Request options</param>
		/// <param name="disableIdGeneration">Disables automatic id generation</param>
		/// <returns>Created document</returns>
		public async Task<ResourceResponse<Document>> CreateDocumentAsync(
			string dbId, string collectionId, object document, RequestOptions requestOptions = null, bool disableIdGeneration = true)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsArgument(document, nameof(document), 0);

			Uri collectionUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

			IDocumentClient client = await GetDocumentClientAsync();
			return await DocumentDbAdapter.ExecuteAndLogAsync(
				0,
				() => client.CreateDocumentAsync(collectionUri, document, requestOptions, disableIdGeneration));
		}


		/// <summary>
		/// Creates a document and its parent containers (database, collection) if they do not exist.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="document">Object to create.</param>
		/// <param name="requestOptions">Request options</param>
		/// <param name="partitionKeyField">Request options</param>
		/// <param name="disableIdGeneration">Disables automatic id generation</param>
		/// <returns>Created document</returns>
		public async Task<ResourceResponse<Document>> CreateDocumentAndContainersAsync(
			string dbId,
			string collectionId,
			object document,
			string partitionKeyField,
			RequestOptions requestOptions = null,
			bool disableIdGeneration = false)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId),0);
			Code.ExpectsArgument(document, nameof(document), 0);

			Uri collectionUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

			IDocumentClient client = await GetDocumentClientAsync();

			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				async () =>
				{
					try
					{
						return await client.CreateDocumentAsync(collectionUri, document, requestOptions, disableIdGeneration);
					}
					catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
					{
						await GetOrCreateDbAndCollectionAsync(dbId, collectionId, partitionKeyField);
						return await client.CreateDocumentAsync(collectionUri, document, requestOptions, disableIdGeneration);
					}
				});
		}


		/// <summary>
		/// Gets a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Document id.</param>
		/// <param name="partitionKey">Partition key for the collection.</param>
		/// <returns>Created document</returns>
		public Task<ResourceResponse<Document>> GetDocumentAsync(
			string dbId, string collectionId, string docId, string partitionKey)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId),0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), 0);

			RequestOptions requestOptions = new RequestOptions { PartitionKey = new PartitionKey(partitionKey) };

			return GetDocumentAsync(dbId, collectionId, docId, requestOptions);
		}


		/// <summary>
		/// Gets a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Document id.</param>
		/// <param name="requestOptions">Request options.</param>
		/// <returns>Created document</returns>
		public async Task<ResourceResponse<Document>> GetDocumentAsync(
			string dbId, string collectionId, string docId, RequestOptions requestOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), 0);

			IDocumentClient client = await GetDocumentClientAsync();

			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				async () =>
				{
					try
					{
						return await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(dbId, collectionId, docId), requestOptions);
					}
					catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
					{
						return default(ResourceResponse<Document>);
					}
				});
		}


		/// <summary>
		/// Gets a document and converts to a POCO type.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Document id.</param>
		/// <param name="requestOptions">Request options.</param>
		/// <returns>POCO object deserialized by default default json deserialization settings by document db sdk.</returns>
		public async Task<T> GetDocumentAsync<T>(
			string dbId, string collectionId, string docId, RequestOptions requestOptions = null) where T : class
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), 0);

			IDocumentClient client = await GetDocumentClientAsync();


			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				async () =>
				{
					try
					{
						return await client.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(dbId, collectionId, docId), requestOptions);
					}
					catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
					{
						return default(T);
					}
				});
		}


		/// <summary>
		/// Gets a document and converts to a POCO using specified converter.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Document id.</param>
		/// <param name="requestOptions">Request options.</param>
		/// <param name="converter">Delegate to convert a document to a POCO object.</param>
		/// <returns>POCO object converted from the retrieved Document.</returns>
		public async Task<T> GetDocumentAsync<T>(
			string dbId, string collectionId, string docId, Func<Document, T> converter, RequestOptions requestOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), 0);
			Code.ExpectsArgument(converter, nameof(converter), 0);

			IDocumentClient client = await GetDocumentClientAsync();

			ResourceResponse<Document> response = await DocumentDbAdapter.ExecuteAndLogAsync(
				0, () => client.ReadDocumentAsync(
					UriFactory.CreateDocumentUri(dbId, collectionId, docId), requestOptions));

			return converter(response.Resource);
		}


		/// <summary>
		/// Gets all documents.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <param name="queryFunc">Optional query</param>
		/// <returns>List of objects of specified type returned from specified database and collection.</returns>
		public async Task<IReadOnlyList<T>> GetAllDocumentsAsync<T>(
			string dbId,
			string collectionId,
			FeedOptions feedOptions = null,
			Func<IQueryable<T>, IQueryable<T>> queryFunc = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);

			IDocumentClient client = await GetDocumentClientAsync();

			IQueryable<T> queryable = client.CreateDocumentQuery<T>(
				UriFactory.CreateDocumentCollectionUri(dbId, collectionId), feedOptions);

			if (queryFunc != null)
			{
				queryable = queryFunc(queryable);
			}

			using (IDocumentQuery<T> query = queryable.AsDocumentQuery())
			{
				return await QueryDocumentsAsync(query, feedOptions);
			}
		}


		/// <summary>
		/// Queries all documents in current continuation and returns continuation token for paged requests.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="continuationToken">Continuation token that can be used  to request next page.
		///  Should be passed as null in the first call.</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <returns>List of objects of specified type returned from specified database and collection.</returns>
		public async Task<Tuple<string, IEnumerable<T>>> GetAllDocumentsWithPagingAsync<T>(
			string dbId,
			string collectionId,
			string continuationToken,
			FeedOptions feedOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);

			Uri colUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

			feedOptions = feedOptions ?? new FeedOptions();
			feedOptions.RequestContinuation = continuationToken;

			IDocumentClient client = await GetDocumentClientAsync();
			try
			{
				using (IDocumentQuery<T> query =
					client.CreateDocumentQuery<T>(colUri, feedOptions).AsDocumentQuery())
				{
					FeedResponse<T> page = await DocumentDbAdapter.ExecuteAndLogAsync(
						0, () => query.ExecuteNextAsync<T>());

					return new Tuple<string, IEnumerable<T>>(page.ResponseContinuation, page);
				}
			}
			catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
			{
				return new Tuple<string, IEnumerable<T>>(continuationToken, Enumerable.Empty<T>());
			}
		}


		/// <summary>
		/// Gets all documents from specified partition.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="partitionKey">Partition key.</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <param name="queryFunc">Optional linq query</param>
		/// <returns>List of objects of specified type returned from specified database and collection.</returns>
		public Task<IReadOnlyList<T>> GetAllDocumentsFromPartitionAsync<T>(
			string dbId, string collectionId, string partitionKey, FeedOptions feedOptions = null,
			Func<IQueryable<T>, IQueryable<T>> queryFunc = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), 0);

			feedOptions = feedOptions ?? new FeedOptions();
			feedOptions.PartitionKey = new PartitionKey(partitionKey);

			return GetAllDocumentsAsync<T>(dbId, collectionId, feedOptions, queryFunc);
		}


		/// <summary>
		/// Queries all documents in current continuation from the specified partition and
		/// returns continuation token for paged requests.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="partitionKey">Partition key.</param>
		/// <param name="continuationToken">Continuation token.</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <returns>List of objects of specified type returned from specified database and collection.</returns>
		public Task<Tuple<string, IEnumerable<T>>> GetAllDocumentsFromPartitionWithPagingAsync<T>(
			string dbId, string collectionId, string partitionKey, string continuationToken, FeedOptions feedOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), 0);

			feedOptions = feedOptions ?? new FeedOptions();
			feedOptions.PartitionKey = new PartitionKey(partitionKey);

			return GetAllDocumentsWithPagingAsync<T>(dbId, collectionId, continuationToken, feedOptions);
		}


		/// <summary>
		/// Queries the collection using the specified sql query.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="sqlQuery">Sql query string</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <returns>List of objects of specified type returned from sql query.</returns>

		public async Task<IReadOnlyList<T>> QueryDocumentsWithSqlAsync<T>(
			string dbId, string collectionId, string sqlQuery, FeedOptions feedOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(sqlQuery, nameof(sqlQuery), 0);

			Uri colUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);
			List<T> data = new List<T>();

			IDocumentClient client = await GetDocumentClientAsync();

			using (IDocumentQuery<T> query =
				client.CreateDocumentQuery<T>(colUri, feedOptions).AsDocumentQuery())
			{
				return await QueryDocumentsAsync(query, feedOptions);
			}
		}


		/// <summary>
		/// Queries the collection using the specified document query.
		/// </summary>
		/// <param name="query">Document query.</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <returns>List of objects of specified type returned from document query.</returns>
		public async Task<IReadOnlyList<T>> QueryDocumentsAsync<T>(
			IDocumentQuery<T> query, FeedOptions feedOptions = null)
		{
			Code.ExpectsArgument(query, nameof(query), 0);
			List<T> data = new List<T>();

			try
			{
				while (query.HasMoreResults)
				{
					foreach (T t in await DocumentDbAdapter.ExecuteAndLogAsync(
						0, () => query.ExecuteNextAsync<T>()))
					{
						data.Add(t);
					}
				}
			}
			catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
			{
				return new List<T>();
			}

			return data;
		}


		/// <summary>
		/// Replaces a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Document id</param>
		/// <param name="entity">Entity to replace in document db collection.</param>
		/// <param name="requestOptions">Request options</param>
		/// <returns>Replaced document.</returns>
		public async Task<ResourceResponse<Document>> ReplaceDocumentAsync<T>(
			string dbId, string collectionId, string docId, T entity, RequestOptions requestOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), 0);
			Code.ExpectsArgument(entity, nameof(entity), 0);

			IDocumentClient client = await GetDocumentClientAsync();

			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				() => client.ReplaceDocumentAsync(
					UriFactory.CreateDocumentUri(dbId, collectionId, docId), entity, requestOptions));
		}


		/// <summary>
		/// Upserts a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="entity">Entity to replace in document db collection.</param>
		/// <param name="requestOptions">Request options</param>
		/// <param name="disableIdGeneration">Disables automatic id generation</param>
		/// <returns>Upserted document.</returns>
		public async Task<ResourceResponse<Document>> UpsertDocumentAsync<T>(
			string dbId, string collectionId, T entity, RequestOptions requestOptions = null, bool disableIdGeneration = true)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsArgument(entity, nameof(entity), 0);

			IDocumentClient client = await GetDocumentClientAsync();

			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				() => client.UpsertDocumentAsync(
					UriFactory.CreateDocumentCollectionUri(dbId, collectionId), entity, requestOptions, disableIdGeneration));
		}


		/// <summary>
		/// Deletes a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Document id</param>
		/// <param name="partitionKey">Partition key of the document to delete.</param>
		/// <returns>Deleted document.</returns>
		public Task<ResourceResponse<Document>> DeleteDocumentAsync(
			string dbId, string collectionId, string docId, string partitionKey)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), 0);

			RequestOptions requestOptions = new RequestOptions { PartitionKey = new PartitionKey(partitionKey) };
			return DeleteDocumentAsync(dbId, collectionId, docId, requestOptions);
		}


		/// <summary>
		/// Deletes a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Document id</param>
		/// <param name="requestOptions">Request options</param>
		/// <returns>Deleted document.</returns>
		public async Task<ResourceResponse<Document>> DeleteDocumentAsync(
			string dbId, string collectionId, string docId, RequestOptions requestOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), 0);

			IDocumentClient client = await GetDocumentClientAsync();

			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				() => client.DeleteDocumentAsync(
					UriFactory.CreateDocumentUri(dbId, collectionId, docId), requestOptions));
		}
	}
}