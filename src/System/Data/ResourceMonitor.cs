// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Monads;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Data
{
	/// <summary>
	/// A class for monitoring resource updates.
	/// </summary>
	public class ResourceMonitor : IResourceMonitor, IDisposable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceMonitor"/> class.
		/// </summary>
		public ResourceMonitor()
			: this(new RetryPolicy(new LinearBackoffPolicy(), 1000, 5), TimeSpan.FromMinutes(5))
		{
			RetryPolicy = new RetryPolicy(new LinearBackoffPolicy(), 1000, 5);
			LoadDelay = TimeSpan.FromMinutes(5);
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceMonitor"/> class.
		/// </summary>
		/// <param name="retryPolicy">retry policy</param>
		/// <param name="loadDelay">load delay</param>
		protected ResourceMonitor(RetryPolicy retryPolicy, TimeSpan loadDelay)
		{
			RetryPolicy = retryPolicy ?? RetryPolicy.None;
			LoadDelay = loadDelay;

			m_monitoredResources = new ConcurrentDictionary<IResource, bool>();
			m_resourceDetails = new ConcurrentDictionary<IResource, IResourceDetails>();
			m_resourceMonitoringData = new ConcurrentDictionary<ResourceUpdatedHandler, ResourceMonitoringData>();
			m_exclusiveAction = new RunExclusiveAction();
		}


		/// <summary>
		/// Start monitoring the resources.
		/// </summary>
		/// <param name="resourcesToMonitor">The resources to be monitored.</param>
		/// <param name="callbackAction">Handler to call when any of the resources is uptdated</param>
		/// <returns><c>true</c> if the operation succeeded; <c>false</c> otherwise.</returns>
		public bool TryStartMonitoring(IEnumerable<IResource> resourcesToMonitor, ResourceUpdatedHandler callbackAction)
		{
			if (!Code.ValidateArgument(resourcesToMonitor, nameof(resourcesToMonitor), TaggingUtilities.ReserveTag(0x238506c2 /* tag_97q1c */)) ||
				!Code.ValidateArgument(callbackAction, nameof(callbackAction), TaggingUtilities.ReserveTag(0x238506c3 /* tag_97q1d */)))
			{
				return false;
			}

			IList<IResource> resources = resourcesToMonitor.ToList();
			ResourceMonitoringData monitoringData = new ResourceMonitoringData(callbackAction, resources);
			if (!TryPerformInitialLoad(resources, monitoringData))
			{
				return false;
			}

			if (!m_resourceMonitoringData.TryAdd(callbackAction, monitoringData))
			{
				return false;
			}

			foreach (IResource resource in resources)
			{
				if (!resource.IsStatic)
				{
					m_monitoredResources.TryAdd(resource, true);
				}
			}

			StartExclusiveAction();

			return true;
		}


		/// <summary>
		/// Stop monitoring the resources.
		/// </summary>
		public void StopMonitoring(ResourceUpdatedHandler callback)
		{
			if (!m_resourceMonitoringData.TryRemove(callback, out ResourceMonitoringData monitoringInstance))
			{
				ULSLogging.LogTraceTag(0x2385038e /* tag_97qoo */, Categories.ConfigurationDataSet, Levels.Verbose,
					"An attempt to remove a non-existing handler");
			}
		}


		/// <summary>
		/// Releases resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Releases resources.
		/// </summary>
		/// <param name="freeManagedObjects"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool freeManagedObjects)
		{
			if (freeManagedObjects)
			{
				Interlocked.Exchange(ref m_actionTimer, null)?.Dispose();
			}
		}


		/// <summary>
		/// Gets a value indicating whether the monitor is enabled.
		/// </summary>
		public bool IsEnabled => m_actionTimer != null;


		/// <summary>
		/// Perfroms initial load of the resources which are not yet known to the monitor
		/// </summary>
		/// <param name="resources">A list of resources</param>
		/// <param name="monitoringData">Monitoring data for this initial load</param>
		private bool TryPerformInitialLoad(IEnumerable<IResource> resources, ResourceMonitoringData monitoringData)
		{
			List<IResource> resourcesNotYetSeen = new List<IResource>();
			foreach (IResource resource in resources)
			{
				if (!m_resourceDetails.TryGetValue(resource, out IResourceDetails details))
				{
					// it is not terrible if we add here a resource that we have actually already loaded
					// but we would want to reduce the number of such occasions to minimum
					resourcesNotYetSeen.Add(resource);
				}
			}

			CheckResourcesForUpdates(resourcesNotYetSeen, monitoringData);
			foreach (IResource resource in resourcesNotYetSeen)
			{
				if (!m_resourceDetails.TryGetValue(resource, out IResourceDetails details))
				{
					return false;
				}
			}

			return true;
		}


		/// <summary>
		/// Check resources for updates
		/// </summary>
		/// <param name="resources">A list of resources to be checked</param>
		/// <param name="singleMonitoringData">Signle instance of ResourceMonitoringData to use if present (use class-wide dict instead)</param>
		private void CheckResourcesForUpdates(IEnumerable<IResource> resources, ResourceMonitoringData singleMonitoringData)
		{
			HashSet<IResource> updatedResources = new HashSet<IResource>();
			foreach (IResource resource in resources)
			{
				IResourceDetails newDetails = GetFileDetails(resource);
				if (newDetails == null)
				{
					ULSLogging.LogTraceTag(0x2385038f /* tag_97qop */, Categories.ConfigurationDataSet, Levels.Warning, "Failed to load resource details for '{0}'", resource.Name);
					continue;
				}

				bool updated;
				if (m_resourceDetails.TryGetValue(resource, out IResourceDetails currentDetails) && currentDetails != null)
				{
					updated = !string.Equals(currentDetails.SHA256Hash, newDetails.SHA256Hash, StringComparison.Ordinal);
				}
				else
				{
					// it is the first time we load this resource
					updated = true;
				}

				if (updated)
				{
					// No explicit synchronization was added here for the following reason:
					// the worst thing that can happed is two parallel threads consider the same resource
					// updated and both call the ResourceMonitoringData.CallHandlerIfNecessary
					// That method on its own ensures in-order execution of handlers
					m_resourceDetails.AddOrUpdate(resource, newDetails, (res, oldDetails) => newDetails);
					updatedResources.Add(resource);
				}
			}

			NotifyHandlers(updatedResources, singleMonitoringData);
		}


		/// <summary>
		/// Call all of the handlers who are initerested in any of the updated resources
		/// or just the specified handler if it should be called
		/// </summary>
		/// <param name="updatedResources">Resources that were updated since the last check</param>
		/// <param name="singleMonitoringData">Single handler to be called if not null</param>
		private void NotifyHandlers(HashSet<IResource> updatedResources, ResourceMonitoringData singleMonitoringData)
		{
			ICollection<ResourceMonitoringData> monitoringCollection = m_resourceMonitoringData.Values;
			if (singleMonitoringData != null)
			{
				monitoringCollection = new List<ResourceMonitoringData> { singleMonitoringData };
			}

			foreach (ResourceMonitoringData monitoringInstance in monitoringCollection)
			{
				monitoringInstance.CallHandlerIfNecessary(m_resourceDetails, updatedResources);
			}
		}


		/// <summary>
		/// Get the file details for the specified resource
		/// </summary>
		/// <param name="resource">A resource to load</param>
		/// <returns>File datails</returns>
		private IResourceDetails GetFileDetails(IResource resource)
		{
			RetryPolicy retryPolicy = RetryPolicy ?? RetryPolicy.None;
			RetryCounter retryCounter = new RetryCounter(retryPolicy);
			int iteration = 0;
			Tuple<ResourceReadStatus, IResourceDetails> finalStatus = retryCounter.Run(
				() =>
				{
					iteration++;
					ULSLogging.LogTraceTag(0x23850390 /* tag_97qoq */, Categories.ConfigurationDataSet, Levels.Verbose,
						"Attempting data set load for resource '{0}' in iteration '{1}' of '{2}'.",
						resource.Name, iteration, retryPolicy.RetryLimit);

					Tuple<ResourceReadStatus, IResourceDetails> status = resource.Read();
					return Tuple.Create(status.Item1 == ResourceReadStatus.Success, status);
				});

			if (finalStatus.Item1 != ResourceReadStatus.Success)
			{
				ULSLogging.LogTraceTag(0x23850391 /* tag_97qor */, Categories.ConfigurationDataSet, Levels.Warning,
					"Failed to read resource '{0}' as the file read status is '{1}'.",
					resource.Name, finalStatus);
				return null;
			}

			return finalStatus.Item2;
		}


		/// <summary>
		/// Start the exclusive action in a timer.
		/// </summary>
		private void StartExclusiveAction()
		{
			if (m_actionTimer == null && !m_monitoredResources.IsEmpty)
			{
				Interlocked.Exchange(ref m_actionTimer, new Timer((object state) =>
				{
					m_exclusiveAction.Do(() => CheckResourcesForUpdates(m_monitoredResources.Keys, null));
				}, null, TimeSpan.Zero, LoadDelay))?.Dispose();
			}
		}


		/// <summary>
		/// Gets or sets the retry policy for loading the data sets.
		/// </summary>
		private RetryPolicy RetryPolicy { get; }


		/// <summary>
		/// Gets or sets the delay between each load.
		/// </summary>
		private TimeSpan LoadDelay { get; }


		/// <summary>
		/// All the resources in monitoring
		/// </summary>
		private ConcurrentDictionary<IResource, bool> m_monitoredResources;


		/// <summary>
		/// Current resource details
		/// </summary>
		private ConcurrentDictionary<IResource, IResourceDetails> m_resourceDetails;


		/// <summary>
		/// A list of ResourceMonitoringData instances that contain
		/// information about callbacks and resources they are interested in
		/// </summary>
		private ConcurrentDictionary<ResourceUpdatedHandler, ResourceMonitoringData> m_resourceMonitoringData;


		/// <summary>
		/// Timer used to run file checking every interval
		/// </summary>
		private Timer m_actionTimer;


		/// <summary>
		/// Exclusive action to check ressources for updates.
		/// </summary>
		private readonly RunExclusiveAction m_exclusiveAction;
	}
}