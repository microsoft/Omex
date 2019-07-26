// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Omex.System.Caching;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Monads;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Configuration.DataSets
{
	/// <summary>
	/// Loads a dataset from configuration files
	/// </summary>
	/// <typeparam name="T">DataSet type</typeparam>
	public abstract class ConfigurationDataSetLoader<T> : ConfigurationDataSetLoader, IConfigurationDataSetLoader<T>
		where T : class, IConfigurationDataSet
	{
		/// <summary>
		/// The override instance of the configuration data set.
		/// </summary>
		private T DataSetOverride { get; }


		/// <summary>
		/// Factory used to create new data sets
		/// </summary>
		private IDataSetFactory<T> DataSetFactory { get; }


		/// <summary>
		/// The exclusive action to run.
		/// </summary>
		private readonly RunExclusiveAction m_action = new RunExclusiveAction();


		/// <summary>
		/// The exclusive action to run.
		/// </summary>
		private static readonly RunExclusiveAction s_action = new RunExclusiveAction();


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="cache">The cache object.</param>
		/// <param name="resourceMonitor">Resource Watcher instance</param>
		/// <param name="dataSetFactory">The data set factory.</param>
		/// <param name="dataSetOverride">The data set override.</param>
		protected ConfigurationDataSetLoader(ICache cache, IResourceMonitor resourceMonitor, IDataSetFactory<T> dataSetFactory, T dataSetOverride = null)
		{
			Cache = Code.ExpectsArgument(cache, nameof(cache), TaggingUtilities.ReserveTag(0x238208d9 /* tag_9669z */));
			m_resourceMonitor = Code.ExpectsArgument(resourceMonitor, nameof(resourceMonitor), TaggingUtilities.ReserveTag(0x23821000 /* tag_967aa */));
			DataSetFactory = Code.ExpectsArgument(dataSetFactory, nameof(dataSetFactory), TaggingUtilities.ReserveTag(0));

			DataSetOverride = dataSetOverride;
		}


		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="freeManagedObjects">Should we only free unmanaged code</param>
		protected virtual void Dispose(bool freeManagedObjects)
		{
			if (freeManagedObjects)
			{
				Interlocked.Exchange(ref m_resourceMonitor, null)?.StopMonitoring(OnResourceUpdated);
			}
		}


		/// <summary>
		/// Loads DataSet and starts resources monitoring
		/// </summary>
		/// <param name="resources">resources</param>
		protected virtual void Initialize(IEnumerable<IResource> resources)
		{
			Resources = Code.ExpectsArgument(resources, nameof(resources), TaggingUtilities.ReserveTag(0x23821001 /* tag_967ab */));

			if (!m_resourceMonitor.TryStartMonitoring(Resources, OnResourceUpdated))
			{
				ULSLogging.LogTraceTag(0x23821002 /* tag_967ac */, Categories.ConfigurationDataSet, Levels.Error,
					"Failed to start resources monitoring for {0}", typeof(T).Name);
			}
			else
			{
				ULSLogging.LogTraceTag(0x23821003 /* tag_967ad */, Categories.ConfigurationDataSet, Levels.Verbose,
					"Successfully started resource monitoring for {0}", typeof(T).Name);
			}
		}


		/// <summary>
		/// Called when the data set is loaded.
		/// </summary>
		/// <param name="fileDetails">The file details.</param>
		protected abstract void OnLoad(IList<ConfigurationDataSetLoadDetails> fileDetails);


		/// <summary>
		/// Called when the data set is reloaded.
		/// </summary>
		/// <param name="oldFileDetails">The previous file details.</param>
		/// <param name="newFileDetails">The new file details.</param>
		protected abstract void OnReload(IList<ConfigurationDataSetLoadDetails> oldFileDetails,
			IList<ConfigurationDataSetLoadDetails> newFileDetails);


		/// <summary>
		/// Format the message for indicating that the data set has been loaded.
		/// </summary>
		/// <param name="fileDetails">The file details.</param>
		/// <returns>The formatted message.</returns>
		protected string FormatOnLoadMessage(IList<ConfigurationDataSetLoadDetails> fileDetails)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"Loaded the '{0}'. File details - '{1}'.",
				typeof(T).Name,
				fileDetails == null ? "null" : string.Join(";", fileDetails));
		}


		/// <summary>
		/// Format the message for indicating that the data set has been reloaded.
		/// </summary>
		/// <param name="oldFileDetails">The previous file details.</param>
		/// <param name="newFileDetails">The new file details.</param>
		/// <returns>The formatted message.</returns>
		protected string FormatOnReloadMessage(IList<ConfigurationDataSetLoadDetails> oldFileDetails,
			IList<ConfigurationDataSetLoadDetails> newFileDetails)
		{
			return string.Format(CultureInfo.InvariantCulture,
				"Reloaded the '{0}'. Old file details - '{1}'. New file details - '{2}'.",
				typeof(T).Name,
				oldFileDetails == null ? "null" : string.Join(";", oldFileDetails),
				newFileDetails == null ? "null" : string.Join(";", newFileDetails));
		}


		/// <summary>
		/// Attempts to load a new dataset and replaces the old one if load was successful
		/// </summary>
		/// <param name="arguments">The arguments for the update event.</param>
		protected virtual void UpdateLoadedDataSet(ResourceUpdatedEventArgs arguments)
		{
			RunExclusiveAction action = AllowMultipleThreadsLoadingDataSet ? m_action : s_action;
			action.Do(() =>
				{
					if (arguments.IsInitialLoad)
					{
						ULSLogging.LogTraceTag(0x23821004 /* tag_967ae */, Categories.ConfigurationDataSet, Levels.Verbose,
							"Adding data set type '{0}' to cache.", typeof(T).Name);

						if (Cache.GetOrAdd(typeof(IConfigurationDataSetLoader<T>),
							() => CreateCachedConfigurationDataSet(new CachedConfigurationDataSet<T>(DataSetOverride, DataSetFactory), arguments),
							out bool wasAdded) is CachedConfigurationDataSet<T> result && wasAdded)
						{
							OnLoad(result.LoadDetails);
						}
					}
					else
					{
						ULSLogging.LogTraceTag(0x23850399 /* tag_97qoz */, Categories.ConfigurationDataSet, Levels.Verbose,
							"Updating data set type '{0}' in cache.", typeof(T).Name);
						CachedConfigurationDataSet<T> dataSets = DataSets;
						IList<ConfigurationDataSetLoadDetails> loadDetails = dataSets.LoadDetails;

						if (Cache.AddOrUpdate(typeof(IConfigurationDataSetLoader<T>),
							() => CreateCachedConfigurationDataSet(dataSets, arguments),
							out bool wasUpdated) is CachedConfigurationDataSet<T> result && wasUpdated)
						{
							OnReload(loadDetails, result.LoadDetails);
						}
					}
				});
		}


		/// <summary>
		/// Creates the cached configuration data set.
		/// </summary>
		/// <param name="initialValue">The initial data set value.</param>
		/// <param name="arguments">The arguments for the update event.</param>
		/// <returns>The cached configuration data set.</returns>
		private CachedConfigurationDataSet<T> CreateCachedConfigurationDataSet(CachedConfigurationDataSet<T> initialValue,
			ResourceUpdatedEventArgs arguments)
		{
			initialValue.UpdateLoadedDataSet(arguments);
			return initialValue;
		}


		/// <summary>
		/// Updates DataSet on resources change event
		/// </summary>
		/// <param name="arguments">event arguments</param>
		/// <remarks>Since this may be called on a background thread without a current correlation,
		/// start a new correlation if one does not already exist</remarks>
		protected virtual void OnResourceUpdated(ResourceUpdatedEventArgs arguments)
		{
			ULSLogging.LogTraceTag(0x23821005 /* tag_967af */, Categories.ConfigurationDataSet, Levels.Verbose,
				"'{0}' loader encountered an event for resources '{1}'.",
				typeof(T).Name, string.Join(";", arguments.Details.Select(d => d.Key)));

			UpdateLoadedDataSet(arguments);
			DataSetLoaded?.Invoke(this, EventArgs.Empty);
		}


		/// <summary>
		/// DataSet resources
		/// </summary>
		public IEnumerable<IResource> Resources { get; private set; }


		/// <summary>
		/// The event raised on dataset load.
		/// </summary>
		public event EventHandler DataSetLoaded;


		/// <summary>
		/// Loaded DataSet instance
		/// </summary>
		public T LoadedDataSet => DataSets?.LoadedDataSet;


		/// <summary>
		/// DataSet instance corresponding to last unsuccessful loading
		/// </summary>
		public T FailedToLoadDataSet => DataSets?.FailedToLoadDataSet;


		/// <summary>
		/// The internal data set representation
		/// </summary>
		private CachedConfigurationDataSet<T> DataSets
		{
			get
			{
				CachedConfigurationDataSet<T> dataSets = Cache.Get(typeof(IConfigurationDataSetLoader<T>)) as CachedConfigurationDataSet<T>;
				if (dataSets == null)
				{
					ULSLogging.LogTraceTag(0x23821006 /* tag_967ag */, Categories.Common, Levels.Warning,
						"The set of data sets returned from the cache is null.");
				}

				return dataSets;
			}
		}


		/// <summary>
		/// Gets or sets the cache object.
		/// </summary>
		private ICache Cache { get; }


		/// <summary>
		/// Resource watcher instance
		/// </summary>
		private IResourceMonitor m_resourceMonitor;


		/// <summary>
		/// A cached configuration data set, that ensures all relevant information for a data set is stored in a cache line.
		/// </summary>
		/// <typeparam name="TDataSet">The data set type.</typeparam>
		private sealed class CachedConfigurationDataSet<TDataSet>
			where TDataSet : class, IConfigurationDataSet
		{
			/// <summary>
			/// The default dataset for creating a data set instance.
			/// </summary>
			private readonly TDataSet m_dataSetDefault;


			/// <summary>
			/// Factory used to create a new data set.
			/// </summary>
			private readonly IDataSetFactory<TDataSet> m_dataSetFactory;


			/// <summary>
			/// Initializes a new instance of the <see cref="CachedConfigurationDataSet{TDataSet}" /> class.
			/// </summary>
			/// <param name="dataSetDefault">The data set default.</param>
			/// <param name="dataSetFactory">Data set factory.</param>
			public CachedConfigurationDataSet(TDataSet dataSetDefault, IDataSetFactory<TDataSet> dataSetFactory)
			{
				m_dataSetDefault = dataSetDefault;
				m_dataSetFactory = Code.ExpectsArgument(dataSetFactory, nameof(dataSetFactory), TaggingUtilities.ReserveTag(0));
			}


			/// <summary>
			/// Attempts to load a new dataset and replaces the old one if load was successful
			/// </summary>
			/// <param name="arguments">The arguments for the update event.</param>
			public void UpdateLoadedDataSet(ResourceUpdatedEventArgs arguments)
			{
				if (!Code.ValidateArgument(arguments, nameof(arguments), TaggingUtilities.ReserveTag(0x23821007 /* tag_967ah */)))
				{
					return;
				}

				TDataSet dataSet = LoadDataSet(arguments);
				if (dataSet != null && dataSet.IsHealthy)
				{
					ULSLogging.LogTraceTag(0x23821008 /* tag_967ai */, Categories.ConfigurationDataSet, Levels.Verbose,
						"Successfully loaded {0} with status 'Healthy'", typeof(TDataSet).Name);
				}
				else
				{
					ULSLogging.LogTraceTag(0x23821009 /* tag_967aj */, Categories.ConfigurationDataSet, Levels.Error,
						"Loaded {0} with status '{1}'", typeof(TDataSet).Name,
						dataSet != null ? dataSet.IsHealthy ? "Healthy" : "Not healthy" : "DataSet is null");
				}

				if (LoadedDataSet == null)
				{
					LoadedDataSet = dataSet;
				}
				else if ((!LoadedDataSet.IsHealthy && dataSet != null) || (dataSet != null && dataSet.IsHealthy))
				{
					ULSLogging.LogTraceTag(0x2382100a /* tag_967ak */, Categories.ConfigurationDataSet, Levels.Verbose,
						"Replacing old {0} loaded at {1} with data loaded with status '{2}'", typeof(TDataSet).Name, LoadedDataSet.LastReload,
						dataSet.IsHealthy ? "Healthy" : "Not healthy");
					LoadedDataSet = dataSet;
				}
				else
				{
					ULSLogging.LogTraceTag(0x2382100b /* tag_967al */, Categories.ConfigurationDataSet, Levels.Error,
						"Not replacing old {0} with data loaded with status '{1}'", typeof(TDataSet).Name,
						dataSet != null ? dataSet.IsHealthy ? "Healthy" : "Not healthy" : "DataSet is null");
					FailedToLoadDataSet = dataSet;
				}

				LoadDetails = dataSet?.LoadDetails;
			}


			/// <summary>
			/// Loads DataSet
			/// </summary>
			/// <param name="arguments">The arguments for the update event.</param>
			/// <returns>Loaded DataSet</returns>
			private TDataSet LoadDataSet(ResourceUpdatedEventArgs arguments)
			{
				ULSLogging.LogTraceTag(0x2382100c /* tag_967am */, Categories.ConfigurationDataSet, Levels.Verbose,
					"Loading data set for '{0}'.", typeof(TDataSet).Name);

				TDataSet dataSet = m_dataSetDefault ?? m_dataSetFactory.Create();

				if (dataSet == null)
				{
					ULSLogging.LogTraceTag(0, Categories.ConfigurationDataSet, Levels.Error,
						"Unable to create data set of type '{0}'", typeof(TDataSet).Name);

					return null;
				}

				try
				{
					dataSet.Load(arguments.Details);
				}
				catch (Exception exception)
				{
					ULSLogging.ReportExceptionTag(0x2382100d /* tag_967an */, Categories.ConfigurationDataSet, exception,
						"Exception encountered while loading '{0}'", typeof(TDataSet).Name);
				}

				return dataSet;
			}


			/// <summary>
			/// The loaded data set instance.
			/// </summary>
			public TDataSet LoadedDataSet { get; private set; }


			/// <summary>
			/// The data set instance corresponding to the last unsuccessful loading.
			/// </summary>
			public TDataSet FailedToLoadDataSet { get; private set; }


			/// <summary>
			/// The last data set load details.
			/// </summary>
			public IList<ConfigurationDataSetLoadDetails> LoadDetails { get; private set; }
		}
	}


	/// <summary>
	/// Non-generic base class for common behaviour
	/// </summary>
	public abstract class ConfigurationDataSetLoader
	{
		/// <summary>
		/// Allow multiple threads to load data sets
		/// </summary>
		public static bool AllowMultipleThreadsLoadingDataSet { get; set; }
	}
}
