// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsArgument(storedProcedure, nameof(storedProcedure), 0);

			IDocumentClient client = await GetDocumentClientAsync();

			StoredProcedure sproc = null;
			var colUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);

			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				async () =>
				{
					try
					{
						if (deleteStoredProcedure)
						{
							await TryDeleteStoredProcedureAsync(dbId, collectionId, storedProcedure.Id);
						}

						sproc = client.CreateStoredProcedureQuery(colUri)
							.Where(p => p.Id == storedProcedure.Id).AsEnumerable().FirstOrDefault();

						if (sproc != null)
						{
							return sproc;
						}

						sproc = await client.CreateStoredProcedureAsync(colUri, storedProcedure);
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
				});
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
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsArgument(trigger, nameof(trigger), 0);

			IDocumentClient client = await GetDocumentClientAsync();

			Uri colUri = UriFactory.CreateDocumentCollectionUri(dbId, collectionId);
			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				async () =>
				{
					try
					{
						if (deleteTrigger)
						{
							await TryDeleteTriggerAsync(dbId, collectionId, trigger.Id);
						}

						Trigger tr = client.CreateTriggerQuery(colUri).Where(t => t.Id == trigger.Id).AsEnumerable().FirstOrDefault();

						if (tr != null)
						{
							return tr;
						}

						return await client.CreateTriggerAsync(colUri, trigger);
						
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
		/// <returns>True is operation is successful, false otherwise</returns>
		public async Task<bool> TryDeleteStoredProcedureAsync(string dbId, string collectionId, string storedProcedureId)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(storedProcedureId, nameof(storedProcedureId), 0);

			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				async () =>
				{
					bool success = true;
					try
					{
						IDocumentClient client = await GetDocumentClientAsync();

						await client.DeleteStoredProcedureAsync(
							UriFactory.CreateStoredProcedureUri(dbId, collectionId, storedProcedureId));
					}
					catch (DocumentClientException clientEx)
					{
						if (clientEx.StatusCode != HttpStatusCode.NotFound)
						{
							success = false;
						}
					}
					catch (Exception)
					{
						success = false;
					}

					return success;
				});
		}


		/// <summary>
		/// Deletes the trigger.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="triggerId">The stored procedure to get or create.</param>
		/// <returns>True is operation is successful, false otherwise</returns>
		public async Task<bool> TryDeleteTriggerAsync(string dbId, string collectionId, string triggerId)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(triggerId, nameof(triggerId), 0);

			return await DocumentDbAdapter.ExecuteAndLogAsync(0,
				async () =>
				{
					IDocumentClient client = await GetDocumentClientAsync();

					bool success = true;
					try
					{
						await client.DeleteTriggerAsync(UriFactory.CreateTriggerUri(dbId, collectionId, triggerId));
					}
					catch (DocumentClientException clientEx)
					{
						if (clientEx.StatusCode != HttpStatusCode.NotFound)
						{
							success = false;
						}
					}
					catch (Exception)
					{
						success = false;
					}

					return success;
				});
		}


		/// <summary>
		/// Registers the provided stored procedures and triggers.
		/// </summary>
		/// <param name="dbId">The id of the Database to search for, or create.</param>
		/// <param name="collectionId">The id of the document db collection.</param>
		/// <param name="scriptOptions">The script options object holding references to stored procedures and triggers to register.</param>
		public async Task RegisterScriptsAsync(string dbId, string collectionId, ScriptOptions scriptOptions)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(dbId, nameof(dbId), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(collectionId, nameof(collectionId), 0);
			Code.ExpectsArgument(scriptOptions, nameof(scriptOptions), 0);

			await DocumentDbAdapter.ExecuteAndLogAsync(0, () =>
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