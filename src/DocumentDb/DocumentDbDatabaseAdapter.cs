// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
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
		/// Get a Database by id, or create a new one if one with the id provided doesn't exist.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="requestOptions">Request Options</param>
		/// <returns>The matched, or created, Database object</returns>
		public Task<ResourceResponse<Database>> GetOrCreateDatabaseAsync(
			string dbId, RequestOptions requestOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);

			return GetOrCreateDatabaseAsync(new Database { Id = dbId }, requestOptions);
		}


		/// <summary>
		/// Gets or creates the database.
		/// </summary>
		/// <param name="database">Database to create or get.</param>
		/// <param name="requestOptions">Request options.</param>
		/// <returns>Database with specified database id.</returns>
		public async Task<ResourceResponse<Database>> GetOrCreateDatabaseAsync(
			Database database, RequestOptions requestOptions = null)
		{
			Code.ExpectsArgument(database, nameof(database), 0);

			IDocumentClient client = await GetDocumentClientAsync();
			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				() => client.CreateDatabaseIfNotExistsAsync(database, requestOptions));
		}


		/// <summary>
		/// Gets all databases in the document db account.
		/// </summary>
		/// <param name="feedOptions">Feed options</param>
		/// <returns>All databases in the account.</returns>
		public async Task<IReadOnlyList<Database>> GetAllDatabasesAsync(FeedOptions feedOptions = null)
		{
			IDocumentClient client = await GetDocumentClientAsync();

			using (IDocumentQuery<Database> query = client.CreateDatabaseQuery().AsDocumentQuery())
			{
				return await QueryDocumentsAsync(query, feedOptions);
			}
		}


		/// <summary>
		/// Gets the database with specified id.
		/// </summary>
		/// <param name="dbId">Database id.</param>
		/// <param name="feedOptions">Request options</param>
		/// <returns>The Database.</returns>
		public async Task<Database> GetDatabaseAsync(string dbId, FeedOptions feedOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);

			IDocumentClient client = await GetDocumentClientAsync();

			using (IDocumentQuery<Database> query =
				client.CreateDatabaseQuery().Where(db => db.Id == dbId).AsDocumentQuery())
			{
				IReadOnlyList<Database> dbs = await QueryDocumentsAsync(query, feedOptions);
				return dbs.FirstOrDefault();
			}
		}


		/// <summary>
		/// Deletes the database.
		/// </summary>
		/// <param name="dbId">Id of the databsae to delete.</param>
		/// <param name="requestOptions">Request options</param>
		/// <returns>Database with specified database id.</returns>
		public async Task<ResourceResponse<Database>> DeleteDatabaseAsync(
			string dbId, RequestOptions requestOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId),0);

			IDocumentClient client = await GetDocumentClientAsync();
			return await DocumentDbAdapter.ExecuteAndLogAsync(
				0x23854602 /* tag_97uyc */, () => client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(dbId), requestOptions));
		}
	}
}