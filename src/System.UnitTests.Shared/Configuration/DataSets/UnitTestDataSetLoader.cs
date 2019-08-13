// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Caching;
using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Data;

namespace Microsoft.Omex.System.UnitTests.Shared.Configuration.DataSets
{
	/// <summary>
	/// Unit Test ConfigurationDataSetLoader
	/// </summary>
	/// <typeparam name="T">Configuration DataSet type</typeparam>
	public class UnitTestDataSetLoader<T> : IConfigurationDataSetLoader<T> where T : class, IConfigurationDataSet, new()
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UnitTestDataSetLoader{T}"/> class.
		/// </summary>
		protected UnitTestDataSetLoader()
		{
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="cache">The cache object.</param>
		/// <param name="resourceMonitor">Resource Monitor instance</param>
		/// <param name="dataSetOverride">The data set override.</param>
		public UnitTestDataSetLoader(ICache cache, IResourceMonitor resourceMonitor, T dataSetOverride = null)
		{
			UnitTestLoader = new UnitTestConfigurationDataSetLoader<T>(cache, resourceMonitor, dataSetOverride, UnitTestOnResourceUpdated);
			UnitTestLoader.DataSetLoaded += OnDataSetLoadedByInternalLoader;
		}


		/// <summary>
		/// Loads DataSet and starts resources monitoring
		/// </summary>
		/// <param name="resources">resources</param>
		public void Initialize(IEnumerable<IResource> resources)
		{
			Resources = resources;
			UnitTestLoader.Initialize(Resources);
		}


		/// <summary>
		/// DataSet resources
		/// </summary>
		public IEnumerable<IResource> Resources { get; protected set; }


		/// <summary>
		/// The event raised on dataset load.
		/// </summary>
		public event EventHandler DataSetLoaded;


		/// <summary>
		/// Loaded DataSet instance
		/// </summary>
		public T LoadedDataSet => m_dataSetOverride != null ? m_dataSetOverride as T : UnitTestLoader.LoadedDataSet;


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
		}


		/// <summary>
		/// DataSet instance corresponding to last unsuccessful loading
		/// </summary>
		public T FailedToLoadDataSet => UnitTestLoader.FailedToLoadDataSet;


		/// <summary>
		/// DataSet override
		/// </summary>
		private IConfigurationDataSet m_dataSetOverride;


		/// <summary>
		/// Underlying ConfigurationDataSetLoader
		/// </summary>
		protected UnitTestConfigurationDataSetLoader<T> UnitTestLoader { get; set; }


		/// <summary>
		/// Override loaded dataSet
		/// </summary>
		/// <param name="dataSet">DataSet override</param>
		public void OverrideLoadedDataSet(IConfigurationDataSet dataSet)
		{
			m_dataSetOverride = dataSet;
			DataSetLoaded?.Invoke(this, EventArgs.Empty);
		}


		/// <summary>
		/// Loads from contents of embedded resources
		/// </summary>
		/// <param name="embeddedResourceNames">List of embedded resources</param>
		/// <param name="typeOverride">type use to override default assembly with resources</param>
		public void InitializeFromEmbeddedResources(IEnumerable<string> embeddedResourceNames, Type typeOverride = null)
		{
			List<IResource> resources = new List<IResource>();
			foreach (string resourceName in embeddedResourceNames)
			{
				resources.Add(EmbeddedResources.GetEmbeddedResource(resourceName, typeOverride ?? typeof(UnitTestConfigurationDataSet)));
			}

			Initialize(resources);
		}


		/// <summary>
		/// Handles ResourceUpdated event
		/// </summary>
		/// <param name="e">event args</param>
		private void UnitTestOnResourceUpdated(ResourceUpdatedEventArgs e) => ResourceUpdatedCalled = true;


		/// <summary>
		/// Indicates whether InternalLoader fired DataSetLoaded event
		/// </summary>
		public bool InternalLoaderDataSetLoadedEventFired { get; set; }


		/// <summary>
		/// A subscription to events fired by internal DataSetLoader
		/// </summary>
		/// <param name="sender">Event Sender</param>
		/// <param name="args">Event arguments</param>
		private void OnDataSetLoadedByInternalLoader(object sender, EventArgs args) => InternalLoaderDataSetLoadedEventFired = true;


		/// <summary>
		/// Has ResourceUpdated been called
		/// </summary>
		public bool ResourceUpdatedCalled { get; set; }


		/// <summary>
		/// Unit Test implementation of ConfigurationDataSetLoader
		/// </summary>
		/// <typeparam name="TConfigurationDataSet">Configuration DataSet type</typeparam>
		protected class UnitTestConfigurationDataSetLoader<TConfigurationDataSet> : ConfigurationDataSetLoader<TConfigurationDataSet>
			where TConfigurationDataSet : class, IConfigurationDataSet, new()
		{
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="cache">The cache object.</param>
			/// <param name="resourceMonitor">Resource Monitor instance</param>
			/// <param name="dataSetOverride">The data set override.</param>
			/// <param name="resourceUpdatedHandler">Handler to be called when the resource is updates</param>
			public UnitTestConfigurationDataSetLoader(ICache cache, IResourceMonitor resourceMonitor, TConfigurationDataSet dataSetOverride = null, ResourceUpdatedHandler resourceUpdatedHandler = null)
				: base(cache, resourceMonitor, dataSetOverride) => m_resourceUpdatedHandler = resourceUpdatedHandler;


			/// <summary>
			/// Loads DataSet and starts resources monitoring
			/// </summary>
			/// <param name="resources">resources</param>
			public new void Initialize(IEnumerable<IResource> resources) => base.Initialize(resources);


			/// <summary>
			/// Handler called when resource monitor notices that the resource has changed
			/// </summary>
			/// <param name="arguments">ResourceUpdated event arguments</param>
			protected override void OnResourceUpdated(ResourceUpdatedEventArgs arguments)
			{
				UpdateLoadedDataSet(arguments);
				m_resourceUpdatedHandler?.Invoke(arguments);
			}


			/// <summary>
			/// Called when the data set is loaded.
			/// </summary>
			/// <param name="fileDetails">The file details.</param>
			protected override void OnLoad(IList<ConfigurationDataSetLoadDetails> fileDetails)
			{
			}


			/// <summary>
			/// Called when the data set is reloaded.
			/// </summary>
			/// <param name="oldFileDetails">The previous file details.</param>
			/// <param name="newFileDetails">The new file details.</param>
			protected override void OnReload(IList<ConfigurationDataSetLoadDetails> oldFileDetails,
				IList<ConfigurationDataSetLoadDetails> newFileDetails)
			{
			}


			/// <summary>
			/// A handler to be called when the resource is updated
			/// </summary>
			private readonly ResourceUpdatedHandler m_resourceUpdatedHandler;
		}
	}
}