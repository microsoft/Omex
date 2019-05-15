// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#region Using directives

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

#endregion

namespace Microsoft.Omex.System.Configuration.DataSets
{
	/// <summary>
	/// Configuration DataSet base class
	/// </summary>
	public abstract class ConfigurationDataSet<T> : IConfigurationDataSet where T : ConfigurationDataSetLogging, new()
	{
		/// <summary>
		/// Is DataSet healthy
		/// </summary>
		public bool IsHealthy => LastReload > DateTime.MinValue && (Errors == null || Errors.Count == 0);


		/// <summary>
		/// Last Reload date and time
		/// </summary>
		public DateTime LastReload { get; protected set; }


		/// <summary>
		/// Gets the details.
		/// </summary>
		public IList<ConfigurationDataSetLoadDetails> LoadDetails { get; private set; }


		/// <summary>
		/// Loading errors
		/// </summary>
		public IList<string> Errors => ULSLogging.Errors;


		/// <summary>
		/// Loads the data set
		/// </summary>
		/// <param name="resources">List of resources to load from</param>
		/// <returns>true if load was successful, false otherwise</returns>
		public virtual bool Load(IDictionary<string, IResourceDetails> resources)
		{
			if (!Code.Validate((resources?.Count ?? 0) > 0, TaggingUtilities.ReserveTag(0),
				"{0} should containt at least one resource, unable to load '{1}'", nameof(resources), GetType().Name))
			{
				//Log error into ConfigurationDataSet error list to preserve old behaviour for backward compatibility
				ULSLogging.LogCodeErrorTag(0, Categories.ConfigurationDataSet, false,
					true, "{0} should contaminant at least one resource, unable to load '{1}'", nameof(resources), GetType().Name);

				return false;
			}

			LoadDetails = new List<ConfigurationDataSetLoadDetails>(resources.Count);
			return true;
		}


		/// <summary>
		/// Loads resource
		/// </summary>
		/// <param name="resources">List of resources</param>
		/// <param name="resourceName">Resource to load</param>
		/// <param name="resourceContent">Loaded resource content</param>
		/// <returns><c>true</c> if the operation succeeded; <c>false</c> otherwise.</returns>
		protected bool LoadResource(IDictionary<string, IResourceDetails> resources, string resourceName, out byte[] resourceContent)
		{
			resourceContent = null;
			if (!Code.ValidateArgument(resources, nameof(resources), TaggingUtilities.ReserveTag(0)) ||
				!Code.ValidateNotNullOrWhiteSpaceArgument(resourceName, nameof(resourceName), TaggingUtilities.ReserveTag(0)))
			{
				return false;
			}

			if (!resources.TryGetValue(resourceName, out IResourceDetails resource))
			{
				ULSLogging.LogCodeErrorTag(0, Categories.ConfigurationDataSet, false, true,
					"Failed to find resource '{0}' for data set '{1}' in the set of loaded resources.", resourceName, GetType().Name);
				return false;
			}

			LoadDetails.Add(new ConfigurationDataSetLoadDetails(resourceName, resource.LastWrite, resource.Length, resource.SHA256Hash));
			resourceContent = resource.Contents;
			return true;
		}


		/// <summary>
		/// Own implementation of logging
		/// </summary>
		protected T ULSLogging { get; } = new T();
	}


	/// <summary>
	/// Configuration DataSet base class
	/// </summary>
	public abstract class ConfigurationDataSet : ConfigurationDataSet<ConfigurationDataSetLogging>
	{
	}
}