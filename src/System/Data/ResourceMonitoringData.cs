// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Data
{
	/// <summary>
	/// A container class for a single resource monitoring instance
	/// which specifies the list of interesting resources, an update
	/// handler and whether the update handler has been already called
	/// at least once
	/// </summary>
	internal class ResourceMonitoringData
	{
		/// <summary>
		/// A constructor
		/// </summary>
		/// <param name="handler">A handler to be called when resources are updated</param>
		/// <param name="resources">A set of resources to be monitored</param>
		public ResourceMonitoringData(ResourceUpdatedHandler handler, IList<IResource> resources)
		{
			OnSomeOfTrackedResourcesUpdated = Code.ExpectsArgument(handler, nameof(handler), TaggingUtilities.ReserveTag(0x238506a2 /* tag_97q08 */));
			m_trackedResources = Code.ExpectsArgument(resources, nameof(resources), TaggingUtilities.ReserveTag(0x238506a3 /* tag_97q09 */));

			m_wasCalledAtLeastOnce = false;
		}


		/// <summary>
		/// Checks whether the conditions to call a handler are met and calls it if necessary
		/// </summary>
		/// <param name="allKnownDetails">Details of all resources that were successfully loaded at some stage</param>
		/// <param name="updatedResources">Resources that were upated since the last check</param>
		public void CallHandlerIfNecessary(IDictionary<IResource, IResourceDetails> allKnownDetails,
			ICollection<IResource> updatedResources)
		{
			if (!Code.ValidateArgument(allKnownDetails, nameof(allKnownDetails), TaggingUtilities.ReserveTag(0x238506c0 /* tag_97q1a */)) ||
				!Code.ValidateArgument(updatedResources, nameof(updatedResources), TaggingUtilities.ReserveTag(0x238506c1 /* tag_97q1b */)))
			{
				return;
			}

			if (HandlerShouldBeCalled(allKnownDetails.Keys, updatedResources) && OnSomeOfTrackedResourcesUpdated != null)
			{
				OnSomeOfTrackedResourcesUpdated(new ResourceUpdatedEventArgs(CreateResourceSetDetails(allKnownDetails), !m_wasCalledAtLeastOnce));
				m_wasCalledAtLeastOnce = true;
			}
		}


		/// <summary>
		/// Create a dictionary of resources details in the format in which handlers expect it
		/// </summary>
		/// <param name="allKnownDetails">Resource details</param>
		/// <returns>Mapping of resource names to details</returns>
		private IDictionary<string, IResourceDetails> CreateResourceSetDetails(IDictionary<IResource, IResourceDetails> allKnownDetails)
		{
			IDictionary<string, IResourceDetails> result = new Dictionary<string, IResourceDetails>();
			foreach (IResource resource in m_trackedResources)
			{
				if (!allKnownDetails.ContainsKey(resource))
				{
					ULSLogging.LogTraceTag(0x2385038d /* tag_97qon */, Categories.ConfigurationDataSet, Levels.Error,
						"Resource details for '{0}' are not present although it should've been loaded at this stage", resource.Name);
					continue;
				}

				result[resource.Name] = allKnownDetails[resource];
			}

			return result;
		}


		/// <summary>
		/// Indicates whether a handler should be called
		/// </summary>
		/// <param name="availableResources">All of the resources for which the data has been loaded at least once</param>
		/// <param name="updatedResources">Updated resources</param>
		/// <returns>True if handler should be called, false otherwise</returns>
		private bool HandlerShouldBeCalled(ICollection<IResource> availableResources, ICollection<IResource> updatedResources)
		{
			if (m_wasCalledAtLeastOnce)
			{
				return m_trackedResources.Any(resource => updatedResources.Contains(resource));
			}

			return m_trackedResources.All(resource => availableResources.Contains(resource));
		}


		/// <summary>
		/// A sequence of all the resources current handler is interested in
		/// </summary>
		private readonly IList<IResource> m_trackedResources;


		/// <summary>
		/// A handler to be called when the resources are modified
		/// </summary>
		private event ResourceUpdatedHandler OnSomeOfTrackedResourcesUpdated;


		/// <summary>
		/// Indicates whether this handler was called at least once
		/// </summary>
		private bool m_wasCalledAtLeastOnce;
	}
}