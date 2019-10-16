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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b15e /* tag_961f4 */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b15f /* tag_961f5 */));
			Code.ExpectsArgument(document, nameof(document), TaggingUtilities.ReserveTag(0x2381b160 /* tag_961f6 */));

			Uri collectionUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(
				0,
				() => client.CreateDocumentAsync(collectionUri, document, requestOptions, disableIdGeneration))
					.ConfigureAwait(false);
		}


		/// <summary>
		/// Creates a document and its parent containers (database, collection) if they do not exist.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="document">Object to create.</param>
		/// <param name="partitionKeyField">Request options</param>
		/// <param name="requestOptions">Request options</param>
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b161 /* tag_961f7 */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId),0);
			Code.ExpectsArgument(document, nameof(document), TaggingUtilities.ReserveTag(0x2381b162 /* tag_961f8 */));

			Uri collectionUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0x2381b163 /* tag_961f9 */),
				async () =>
				{
					try
					{
						return await client.CreateDocumentAsync(collectionUri, document, requestOptions, disableIdGeneration).ConfigureAwait(false);
					}
					catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
					{
						// We get NotFound exception either when db and/or collection is not yet created. We create the missing component before retrying to create the document.
						await GetOrCreateDbAndCollectionAsync(dbId, collectionId, partitionKeyField).ConfigureAwait(false);

						return await client.CreateDocumentAsync(collectionUri, document, requestOptions, disableIdGeneration).ConfigureAwait(false);
					}
				}).ConfigureAwait(false);
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b180 /* tag_961ga */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b181 /* tag_961gb */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), TaggingUtilities.ReserveTag(0x2381b182 /* tag_961gc */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), TaggingUtilities.ReserveTag(0x2381b183 /* tag_961gd */));

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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b184 /* tag_961ge */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b185 /* tag_961gf */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), TaggingUtilities.ReserveTag(0x2381b186 /* tag_961gg */));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0x2381b187 /* tag_961gh */),
				async () =>
				{
					try
					{
						return await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(dbId, collectionId, docId), requestOptions)
						.ConfigureAwait(false);
					}
					catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
					{
						return default(ResourceResponse<Document>);
					}
				}).ConfigureAwait(false);
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b188 /* tag_961gi */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b189 /* tag_961gj */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), TaggingUtilities.ReserveTag(0x2381b18a /* tag_961gk */));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0x2381b18b /* tag_961gl */),
				async () =>
				{
					try
					{
						return await client.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(dbId, collectionId, docId), requestOptions)
							.ConfigureAwait(false);
					}
					catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
					{
						return default(T);
					}
				}).ConfigureAwait(false);
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b18c /* tag_961gm */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b18d /* tag_961gn */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), TaggingUtilities.ReserveTag(0x2381b18e /* tag_961go */));
			Code.ExpectsArgument(converter, nameof(converter), TaggingUtilities.ReserveTag(0x2381b18f /* tag_961gp */));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			ResourceResponse<Document> response = await DocumentDbAdapter.ExecuteAndLogAsync(
				0, () => client.ReadDocumentAsync(
					UriFactory.CreateDocumentUri(dbId, collectionId, docId), requestOptions)).ConfigureAwait(false);

			return converter(response.Resource);
		}


		/// <summary>
		/// Gets all documents.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <param name="queryFunc">Optional delegate to update the generated query before calling document db</param>
		/// <returns>List of objects of specified type returned from specified database and collection.</returns>
		public async Task<IReadOnlyList<T>> GetAllDocumentsAsync<T>(
			string dbId,
			string collectionId,
			FeedOptions feedOptions = null,
			Func<IQueryable<T>, IQueryable<T>> queryFunc = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b190 /* tag_961gq */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b191 /* tag_961gr */));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			IQueryable<T> queryable = client.CreateDocumentQuery<T>(
				UriFactory.CreateDocumentCollectionUri(dbId, collectionId), feedOptions);

			if (queryFunc != null)
			{
				queryable = queryFunc(queryable);
			}

			using (IDocumentQuery<T> query = queryable.AsDocumentQuery())
			{
				return await QueryDocumentsAsync(query, feedOptions).ConfigureAwait(false);
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b192 /* tag_961gs */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b193 /* tag_961gt */));

			Uri colUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

			feedOptions = feedOptions ?? new FeedOptions();
			feedOptions.RequestContinuation = continuationToken;

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);
			try
			{
				using (IDocumentQuery<T> query =
					client.CreateDocumentQuery<T>(colUri, feedOptions).AsDocumentQuery())
				{
					FeedResponse<T> page = await DocumentDbAdapter.ExecuteAndLogAsync(
						0, () => query.ExecuteNextAsync<T>()).ConfigureAwait(false);

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
		/// <param name="queryFunc">Optional delegate to update the generated query before calling document db</param>
		/// <returns>List of objects of specified type returned from specified database and collection.</returns>
		public Task<IReadOnlyList<T>> GetAllDocumentsFromPartitionAsync<T>(
			string dbId, string collectionId, string partitionKey, FeedOptions feedOptions = null,
			Func<IQueryable<T>, IQueryable<T>> queryFunc = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), TaggingUtilities.ReserveTag(0x2381b194 /* tag_961gu */));

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
			Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), TaggingUtilities.ReserveTag(0x2381b195 /* tag_961gv */));

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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b196 /* tag_961gw */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b197 /* tag_961gx */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(sqlQuery, nameof(sqlQuery), TaggingUtilities.ReserveTag(0x2381b198 /* tag_961gy */));

			Uri colUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			using (IDocumentQuery<T> query =
				client.CreateDocumentQuery<T>(colUri, new SqlQuerySpec(sqlQuery), feedOptions).AsDocumentQuery())
			{
				return await QueryDocumentsAsync(query, feedOptions).ConfigureAwait(false);
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
			Code.ExpectsArgument(query, nameof(query), TaggingUtilities.ReserveTag(0x2381b199 /* tag_961gz */));
			List<T> data = new List<T>();

			try
			{
				while (query.HasMoreResults)
				{
					foreach (T t in await DocumentDbAdapter.ExecuteAndLogAsync(
						0, () => query.ExecuteNextAsync<T>()).ConfigureAwait(false))
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
			where T:class
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b19a /* tag_961g0 */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b19b /* tag_961g1 */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), TaggingUtilities.ReserveTag(0x2381b19c /* tag_961g2 */));
			Code.ExpectsArgument(entity, nameof(entity), TaggingUtilities.ReserveTag(0x2381b19d /* tag_961g3 */));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0x2381b19e /* tag_961g4 */),
				() => client.ReplaceDocumentAsync(
					UriFactory.CreateDocumentUri(dbId, collectionId, docId), entity, requestOptions))
						.ConfigureAwait(false);
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
			where T:class
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b19f /* tag_961g5 */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b1a0 /* tag_961g6 */));
			Code.ExpectsArgument(entity, nameof(entity), TaggingUtilities.ReserveTag(0x2381b1a1 /* tag_961g7 */));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0x2381b1a2 /* tag_961g8 */),
				() => client.UpsertDocumentAsync(
					UriFactory.CreateDocumentCollectionUri(dbId, collectionId), entity, requestOptions, disableIdGeneration))
						.ConfigureAwait(false);
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b1a3 /* tag_961g9 */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b1c0 /* tag_961ha */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), TaggingUtilities.ReserveTag(0x2381b1c1 /* tag_961hb */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(partitionKey, nameof(partitionKey), TaggingUtilities.ReserveTag(0x2381b1c2 /* tag_961hc */));

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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b1c3 /* tag_961hd */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0x2381b1c4 /* tag_961he */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(docId, nameof(docId), TaggingUtilities.ReserveTag(0x2381b1c5 /* tag_961hf */));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0x2381b1c6 /* tag_961hg */),
				() => client.DeleteDocumentAsync(
					UriFactory.CreateDocumentUri(dbId, collectionId, docId), requestOptions))
						.ConfigureAwait(false);
		}
	}
}