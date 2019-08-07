// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.UnitTests.Shared.Configuration;

namespace Microsoft.Omex.System.UnitTests.Shared.Data
{
	/// <summary>
	/// Unit test resource watcher
	/// </summary>
	public class UnitTestResourceMonitor : IResourceMonitor
	{
		/// <summary>
		/// constructor
		/// </summary>
		public UnitTestResourceMonitor() => m_enablesSuccessfully = true;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="enablesSuccessfully">should start successfully</param>
		public UnitTestResourceMonitor(bool enablesSuccessfully) => m_enablesSuccessfully = enablesSuccessfully;


		/// <summary>
		/// Start monitoring data deployment resources
		/// </summary>
		/// <param name="monitoredResources">Resources to be monitored</param>
		/// <param name="callbackAction">Action to be called when the resources are updated</param>
		/// <returns><c>true</c> if the operation succeeded; <c>false</c> otherwise.</returns>
		public bool TryStartMonitoring(IEnumerable<IResource> monitoredResources, ResourceUpdatedHandler callbackAction)
		{
			IsEnabled = m_enablesSuccessfully;

			IDictionary<string, IResourceDetails> resources =
				new Dictionary<string, IResourceDetails>(StringComparer.OrdinalIgnoreCase);
			foreach (IResource monitoredResource in monitoredResources)
			{
				ResourceReadStatus status = monitoredResource.GetContent(out byte[] contents);
				if (status == ResourceReadStatus.Success)
				{
					resources.Add(monitoredResource.Name, EmbeddedResources.CreateResourceDetails(contents));
				}
				else
				{
					return false;
				}
			}

			m_resourceUpdatedHandler += callbackAction;
			ThrowResourceUpdated(new ResourceUpdatedEventArgs(resources, true));
			return IsEnabled;
		}


		/// <summary>
		/// Stops monitoring
		/// </summary>
		public void StopMonitoring(ResourceUpdatedHandler callback)
		{
			IsEnabled = false;
			IsDisposed = true;
		}


		/// <summary>
		/// Throws ResourceUpdated event
		/// </summary>
		/// <param name="arguments">The event arguments.</param>
		public void ThrowResourceUpdated(ResourceUpdatedEventArgs arguments) => m_resourceUpdatedHandler?.Invoke(arguments);


		/// <summary>
		/// Is resource monitoring enabled
		/// </summary>
		public bool IsEnabled { get; private set; }


		/// <summary>
		/// Is disposed
		/// </summary>
		public bool IsDisposed { get; private set; }


		/// <summary>
		/// Should be enabled successfully
		/// </summary>
		private readonly bool m_enablesSuccessfully;


		/// <summary>
		/// A handler to be called when the resource is updated
		/// </summary>
		private ResourceUpdatedHandler m_resourceUpdatedHandler;
	}
}