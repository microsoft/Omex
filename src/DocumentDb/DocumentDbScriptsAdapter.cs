// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
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
		/// Gets the stored procedure, or creates a new one if one with the specified storedProcedure.id doesn't exist.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="storedProcedure">The stored procedure to get or create.</param>
		/// <param name="deleteStoredProcedure">Indicator to delete the stored procedure before creating it.</param>
		/// <returns>The matched, or created, StoredProcedure object</returns>
		public async Task<StoredProcedure> GetOrCreateStoredProcedureAsync(
			string dbId,
			string collectionId,
			StoredProcedure storedProcedure,
			bool deleteStoredProcedure = false)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsArgument(storedProcedure, nameof(storedProcedure), TaggingUtilities.ReserveTag(0));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			StoredProcedure sproc = null;
			var colUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0),
				async () =>
				{
					try
					{
						if (deleteStoredProcedure)
						{
							await DeleteStoredProcedureAsync(dbId, collectionId, storedProcedure.Id).ConfigureAwait(false);
						}

						sproc = client.CreateStoredProcedureQuery(colUri)
							.Where(p => p.Id == storedProcedure.Id).AsEnumerable().FirstOrDefault();

						if (sproc != null)
						{
							return sproc;
						}

						sproc = await client.CreateStoredProcedureAsync(colUri, storedProcedure).ConfigureAwait(false);
						return sproc;
					}
					catch (DocumentClientException ex)
					{
						if (ex.StatusCode == HttpStatusCode.Conflict)
						{
							sproc = client.CreateStoredProcedureQuery(colUri)
								.Where(p => p.Id == storedProcedure.Id).AsEnumerable().FirstOrDefault();

							return sproc;
						}

						throw;
					}
				}).ConfigureAwait(false);
		}


		/// <summary>
		/// Gets the trigger, or creates a new one if one with the specified trigger.id doesn't exist.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="trigger">The trigger to get or create.</param>
		/// <param name="deleteTrigger">Indicator to delete the trigger before creating it.</param>
		/// <returns>The matched, or created, Trigger object</returns>
		public async Task<Trigger> GetOrCreateTriggerAsync(
			string dbId,
			string collectionId,
			Trigger trigger,
			bool deleteTrigger = false)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsArgument(trigger, nameof(trigger), TaggingUtilities.ReserveTag(0));

			IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

			Uri colUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);
			return await DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0),
				async () =>
				{
					try
					{
						if (deleteTrigger)
						{
							await DeleteTriggerAsync(dbId, collectionId, trigger.Id).ConfigureAwait(false);
						}

						Trigger tr = client.CreateTriggerQuery(colUri).Where(t => t.Id == trigger.Id).AsEnumerable().FirstOrDefault();

						if (tr != null)
						{
							return tr;
						}

						return await client.CreateTriggerAsync(colUri, trigger).ConfigureAwait(false);
						
					}
					catch (DocumentClientException ex)
					{
						if (ex.StatusCode != HttpStatusCode.Conflict) throw;

						return client.CreateTriggerQuery(colUri)
							.Where(p => p.Id == trigger.Id).AsEnumerable().FirstOrDefault();
					}
				});
		}


		/// <summary>
		/// Deletes the stored procedure.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="storedProcedureId">The stored procedure to get or create.</param>
		/// <returns>True if operation is successful, false otherwise</returns>
		public Task DeleteStoredProcedureAsync(string dbId, string collectionId, string storedProcedureId)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(storedProcedureId, nameof(storedProcedureId), TaggingUtilities.ReserveTag(0));

			return DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0),
				async () =>
				{
					try
					{
						IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

						await client.DeleteStoredProcedureAsync(
							UriFactory.CreateStoredProcedureUri(dbId, collectionId, storedProcedureId))
							.ConfigureAwait(false);
					}
					catch (DocumentClientException clientEx) when (clientEx.StatusCode == HttpStatusCode.NotFound)
					{
					}
				});
		}


		/// <summary>
		/// Deletes the trigger.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="triggerId">The trigger to get or create.</param>
		/// <returns>True if operation is successful, false otherwise</returns>
		public Task DeleteTriggerAsync(string dbId, string collectionId, string triggerId)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(triggerId, nameof(triggerId), TaggingUtilities.ReserveTag(0));

			return DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0),
				async () =>
				{
					IDocumentClient client = await GetDocumentClientAsync().ConfigureAwait(false);

					try
					{
						await client.DeleteTriggerAsync(UriFactory.CreateTriggerUri(dbId, collectionId, triggerId))
							.ConfigureAwait(false);
					}
					catch (DocumentClientException clientEx) when (clientEx.StatusCode == HttpStatusCode.NotFound)
					{
					}
				});
		}


		/// <summary>
		/// Registers the provided stored procedures and triggers.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="scriptOptions">The script options object holding references to stored procedures and triggers to register.</param>
		public Task RegisterScriptsAsync(string dbId, string collectionId, ScriptOptions scriptOptions)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), TaggingUtilities.ReserveTag(0));
			Code.ExpectsArgument(scriptOptions, nameof(scriptOptions), TaggingUtilities.ReserveTag(0));

			return DocumentDbAdapter.ExecuteAndLogAsync(TaggingUtilities.ReserveTag(0), () =>
				 {
					 IEnumerable<Task> tasks = Enumerable.Empty<Task>();
					 tasks = tasks.Union(scriptOptions?.Triggers?.Select(t => GetOrCreateTriggerAsync(dbId, collectionId, t, scriptOptions.ResetScripts)) ??
						 Enumerable.Empty<Task>());

					 tasks = tasks.Union(scriptOptions?.StoredProcedures?.Select(s => GetOrCreateStoredProcedureAsync(dbId, collectionId, s, scriptOptions.ResetScripts)) ??
						 Enumerable.Empty<Task>());

					 return Task.WhenAll(tasks);
				 });
		}
	}
}