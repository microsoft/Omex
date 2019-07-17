// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Microsoft.Omex.DocumentDb
{
	/// <summary>
	/// DocumentDb adapter interface.
	/// Abstracts core document db functionality and provides useful wrappers.
	/// </summary>
	public interface IDocumentDbAdapter
	{
		#region common

		/// <summary>
		/// Gets IDocumentClient. The underlying tak is lazy initialized.
		/// Once the task runs to completion, it will return the same
		/// </summary>
		/// <returns>The IDocumentClient interface.</returns>
		Task<IDocumentClient> GetDocumentClientAsync();

		#endregion common

		#region database

		/// <summary>
		/// Get a Database by id, or create a new one if one with the id provided doesn't exist.
		/// </summary>
		/// <param name="dbId">The id of the Database to get or create.</param>
		/// <param name="requestOptions">Request options.</param>
		/// <returns>The matched, or created, Database object</returns>
		Task<ResourceResponse<Database>> GetOrCreateDatabaseAsync(
			string dbId, RequestOptions requestOptions = null);


		/// <summary>
		/// Gets or creates the database.
		/// </summary>
		/// <param name="database">Database to create or get.</param>
		/// <param name="requestOptions">Request options.</param>
		/// <returns>Database with specified database id.</returns>
		Task<ResourceResponse<Database>> GetOrCreateDatabaseAsync(
			Database database, RequestOptions requestOptions = null);


		/// <summary>
		/// Gets all databases in the document db account.
		/// </summary>
		/// <param name="feedOptions">Feed options</param>
		/// <returns>All databases in the account.</returns>
		Task<IReadOnlyList<Database>> GetAllDatabasesAsync(FeedOptions feedOptions = null);


		/// <summary>
		/// Gets the database with specified id.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="feedOptions">Request options</param>
		/// <returns>The Database.</returns>
		Task<Database> GetDatabaseAsync(string dbId, FeedOptions feedOptions = null);


		/// <summary>
		/// Deletes the database.
		/// </summary>
		/// <param name="dbId">Id of the database to delete.</param>
		/// <param name="requestOptions">Request options</param>
		/// <returns>Database with specified database id.</returns>
		Task<ResourceResponse<Database>> DeleteDatabaseAsync(
			string dbId, RequestOptions requestOptions = null);

		#endregion database

		#region collection

		/// <summary>
		/// Gets the database and collection, otherwise creates the collection and the database if db also does not exist.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="partitionKey">Partition key of the document collection</param>
		/// <param name="reservedRUs">Reserved request units for the collection.</param>
		/// <returns>DocumentCollection with specified id.</returns>
		Task<ResourceResponse<DocumentCollection>> GetOrCreateDbAndCollectionAsync(
			string dbId,
			string collectionId,
			string partitionKey,
			int reservedRUs = 1000);


		/// <summary>
		/// Gets the DocumentCollection, otherwise creates it including the database.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="partitionKey">Partition key of the document collection</param>
		/// <param name="reservedRUs">Reserved request units for the collection.</param>
		/// <returns>DocumentCollection with specified id.</returns>
		Task<ResourceResponse<DocumentCollection>> GetOrCreateCollectionAsync(
			string dbId,
			string collectionId,
			string partitionKey,
			int reservedRUs = 1000);


		/// <summary>
		/// Creates the DocumentCollection if it does not exit otherwise returns the existing one.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="documentCollection">Document collection.</param>
		/// <param name="requestOptions">Request options.</param>
		/// <returns>Created document collection.</returns>
		Task<ResourceResponse<DocumentCollection>> GetOrCreateCollectionAsync(
			string dbId,
			DocumentCollection documentCollection,
			RequestOptions requestOptions);


		/// <summary>
		/// Gets the document collection.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="requestOptions">Request options</param>
		/// <returns>DocumentCollection with specified id.</returns>
		Task<ResourceResponse<DocumentCollection>> GetCollectionAsync(
			string dbId, string collectionId, RequestOptions requestOptions);


		/// <summary>
		/// Gets the all document collections in the database.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="feedOptions">Feed options</param>
		/// <returns>All document collections in the database.</returns>
		Task<IReadOnlyList<DocumentCollection>> GetAllCollectionsAsync(
			string dbId, FeedOptions feedOptions = null);


		/// <summary>
		/// Deletes the document collection.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="requestOptions">Request options</param>
		/// <returns>Deleted DocumentCollection.</returns>
		Task<ResourceResponse<DocumentCollection>> DeleteCollectionAsync(
			string dbId, string collectionId, RequestOptions requestOptions = null);

		#endregion collection

		#region document

		/// <summary>
		/// Creates a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="document">Object to create.</param>
		/// <param name="requestOptions">Request options</param>
		/// <param name="disableIdGeneration">Disables automatic id generation</param>
		/// <returns>Created document</returns>
		Task<ResourceResponse<Document>> CreateDocumentAsync(
			string dbId, string collectionId, object document, RequestOptions requestOptions = null, bool disableIdGeneration = true);


		/// <summary>
		/// Creates a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="document">Object to create.</param>
		/// <param name="requestOptions">Request options</param>
		/// <param name="partitionKeyField">PartitionId field, needed to create the partitioned collection
		///  if it does not exist.</param>
		/// <param name="disableIdGeneration">Disables automatic id generation</param>
		/// <returns>Created document</returns>
		Task<ResourceResponse<Document>> CreateDocumentAndContainersAsync(
			string dbId,
			string collectionId,
			object document,
			string partitionKeyField,
			RequestOptions requestOptions = null,
			bool disableIdGeneration = false);


		/// <summary>
		/// Gets a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Collection id.</param>
		/// <param name="partitionKey">Partition key for the collection.</param>
		/// <returns>Created document</returns>
		Task<ResourceResponse<Document>> GetDocumentAsync(
			string dbId, string collectionId, string docId, string partitionKey);


		/// <summary>
		/// Gets a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Collection id.</param>
		/// <param name="requestOptions">Request options.</param>
		/// <returns>Created document</returns>
		Task<ResourceResponse<Document>> GetDocumentAsync(
			string dbId, string collectionId, string docId, RequestOptions requestOptions = null);


		/// <summary>
		/// Gets a document and converts to a POCO using specified converter.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Collection id.</param>
		/// <param name="requestOptions">Request options.</param>
		/// <returns>POCO object deserialized by default default json deserialization settings by document db sdk.</returns>
		Task<T> GetDocumentAsync<T>(string dbId, string collectionId, string docId, RequestOptions requestOptions = null)
			where T : class;


		/// <summary>
		/// Gets a document and converts to a POCO using specified converter.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Collection id.</param>
		/// <param name="requestOptions">Request options.</param>
		/// <param name="converter">Delegate to convert a document to a POCO object.</param>
		/// <returns>POCO object converted from the retrieved Document.</returns>
		Task<T> GetDocumentAsync<T>(
			string dbId,
			string collectionId,
			string docId,
			Func<Document, T> converter,
			RequestOptions requestOptions = null);


		/// <summary>
		/// Gets all documents.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <param name="queryFunc">Optional delegate to update the generated query before calling document db</param>
		/// <returns>List of objects of specified type returned from specified database and collection.</returns>
		Task<IReadOnlyList<T>> GetAllDocumentsAsync<T>(
			string dbId,
			string collectionId,
			FeedOptions feedOptions = null,
			Func<IQueryable<T>, IQueryable<T>> queryFunc = null);


		/// <summary>
		/// Queries all documents and returns continuation token for paged requests.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="continuationToken">Continuation token that can be used  to request next page.
		///  Should be passed as null in the first call.</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <returns>List of objects of specified type returned from specified database and collection.</returns>
		Task<Tuple<string, IEnumerable<T>>> GetAllDocumentsWithPagingAsync<T>(
			string dbId,
			string collectionId,
			string continuationToken,
			FeedOptions feedOptions = null);


		/// <summary>
		/// Gets all documents from specified partition.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="partitionKey">Partition key.</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <param name="queryFunc">Optional delegate to update the generated query before calling document db</param>
		/// <returns>List of objects of specified type returned from specified database and collection.</returns>
		Task<IReadOnlyList<T>> GetAllDocumentsFromPartitionAsync<T>(
			string dbId,
			string collectionId,
			string partitionKey,
			FeedOptions feedOptions = null,
			Func<IQueryable<T>, IQueryable<T>> queryFunc = null);


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
		Task<Tuple<string, IEnumerable<T>>> GetAllDocumentsFromPartitionWithPagingAsync<T>(
			string dbId,
			string collectionId,
			string partitionKey,
			string continuationToken,
			FeedOptions feedOptions = null);


		/// <summary>
		/// Queries the collection using the specified sql query.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="sqlQuery">Sql query string</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <returns>List of objects of specified type returned from sql query.</returns>
		Task<IReadOnlyList<T>> QueryDocumentsWithSqlAsync<T>(
			string dbId,
			string collectionId,
			string sqlQuery,
			FeedOptions feedOptions = null);


		/// <summary>
		/// Queries the collection using the specified document query.
		/// </summary>
		/// <param name="query">Document query.</param>
		/// <param name="feedOptions">Feed options.</param>
		/// <returns>List of objects of specified type returned from document query.</returns>
		Task<IReadOnlyList<T>> QueryDocumentsAsync<T>(
			IDocumentQuery<T> query, FeedOptions feedOptions = null);


		/// <summary>
		/// Replaces a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Document id</param>
		/// <param name="entity">Entity to replace in document db collection.</param>
		/// <param name="requestOptions">Request options</param>
		/// <returns>Replaced document.</returns>
		Task<ResourceResponse<Document>> ReplaceDocumentAsync<T>(
			string dbId, string collectionId, string docId, T entity, RequestOptions requestOptions = null);


		/// <summary>
		/// Upserts a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="entity">Entity to replace in document db collection.</param>
		/// <param name="requestOptions">Request options</param>
		/// <param name="disableIdGeneration">Disables automatic id generation</param>
		/// <returns>Upserted document.</returns>
		Task<ResourceResponse<Document>> UpsertDocumentAsync<T>(
			string dbId, string collectionId, T entity, RequestOptions requestOptions = null, bool disableIdGeneration = true);


		/// <summary>
		/// Deletes a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Document id</param>
		/// <param name="partitionKey">Partition key of the document to delete.</param>
		/// <returns>Deleted document.</returns>
		Task<ResourceResponse<Document>> DeleteDocumentAsync(
			string dbId, string collectionId, string docId, string partitionKey);


		/// <summary>
		/// Deletes a document.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="collectionId">Collection id.</param>
		/// <param name="docId">Document id</param>
		/// <param name="requestOptions">Request options</param>
		/// <returns>Deleted document.</returns>
		Task<ResourceResponse<Document>> DeleteDocumentAsync(
			string dbId, string collectionId, string docId, RequestOptions requestOptions = null);

		#endregion document

		#region scripts

		/// <summary>
		/// Gets the stored procedure, or creates a new one if one with the specified storedProcedure.id doesn't exist.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="storedProcedure">The stored procedure to get or create.</param>
		/// <param name="deleteStoredProcedure">Indicator to delete the stored procedure before creating it.</param>
		/// <returns>The matched, or created, StoredProcedure object</returns>
		Task<StoredProcedure> GetOrCreateStoredProcedureAsync(
			string dbId,
			string collectionId,
			StoredProcedure storedProcedure,
			bool deleteStoredProcedure = false);


		/// <summary>
		/// Gets the trigger, or creates a new one if one with the specified trigger.id doesn't exist.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="trigger">The trigger to get or create.</param>
		/// <param name="deleteTrigger">Indicator to delete the trigger before creating it.</param>
		/// <returns>The matched, or created, Trigger object</returns>
		Task<Trigger> GetOrCreateTriggerAsync(
			string dbId,
			string collectionId,
			Trigger trigger,
			bool deleteTrigger = false);


		/// <summary>
		/// Deletes the stored procedure.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="storedProcedureId">The stored procedure to get or create.</param>
		/// <returns>True is operation is successful, false otherwise</returns>
		Task DeleteStoredProcedureAsync(string dbId, string collectionId, string storedProcedureId);


		/// <summary>
		/// Deletes the trigger.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="triggerId">The stored procedure to get or create.</param>
		/// <returns>True is operation is successful, false otherwise</returns>
		Task DeleteTriggerAsync(string dbId, string collectionId, string triggerId);


		/// <summary>
		/// Registers the provided stored procedures and triggers.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="scriptOptions">The script options object holding references to stored procedures and triggers to register.</param>
		Task RegisterScriptsAsync(string dbId, string collectionId, ScriptOptions scriptOptions);

		#endregion scripts
	}
}