// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
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
		/// Get a Database by id, or create a new one if one with the id provided doesn't exist.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="requestOptions">Request Options</param>
		/// <returns>The matched, or created, Database object</returns>
		public Task<ResourceResponse<Database>> GetOrCreateDatabaseAsync(
			string dbId, RequestOptions requestOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b1c7 /* tag_961hh */));

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
			Code.ExpectsArgument(database, nameof(database), TaggingUtilities.ReserveTag(0x2381b1c8 /* tag_961hi */));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0x2381b1c9 /* tag_961hj */),
				() => client.CreateDatabaseIfNotExistsAsync(database, requestOptions)).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets all databases in the document db account.
		/// </summary>
		/// <param name="feedOptions">Feed options</param>
		/// <returns>All databases in the account.</returns>
		public async Task<IReadOnlyList<Database>> GetAllDatabasesAsync(FeedOptions feedOptions = null)
		{
			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			using (IDocumentQuery<Database> query = client.CreateDatabaseQuery().AsDocumentQuery())
			{
				return await QueryDocumentsAsync(query, feedOptions).ConfigureAwait(false);
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0x2381b1ca /* tag_961hk */));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			using (IDocumentQuery<Database> query =
				client.CreateDatabaseQuery().Where(db => db.Id == dbId).AsDocumentQuery())
			{
				IReadOnlyList<Database> dbs = await QueryDocumentsAsync(query, feedOptions).ConfigureAwait(false);

				return dbs.FirstOrDefault();
			}
		}

		/// <summary>
		/// Deletes the database.
		/// </summary>
		/// <param name="dbId">Id of the database to delete.</param>
		/// <param name="requestOptions">Request options</param>
		/// <returns>Database with specified database id.</returns>
		public async Task<ResourceResponse<Database>> DeleteDatabaseAsync(
			string dbId, RequestOptions requestOptions = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId),0);

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			return await DocumentDbAdapter.ExecuteAndLogAsync(
				0, () => client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(dbId), requestOptions))
					.ConfigureAwait(false);
		}
	}
}
