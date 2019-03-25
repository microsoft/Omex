// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.System.Data
{
	/// <summary>
	/// A delegate that represents handler to be called when the resource is updated
	/// </summary>
	/// <param name="details">Details</param>
	public delegate void ResourceUpdatedHandler(ResourceUpdatedEventArgs details);


	/// <summary>
	/// An interface for monitoring resource updates.
	/// </summary>
	public interface IResourceMonitor
	{
		/// <summary>
		/// Start monitoring the resources.
		/// </summary>
		/// <param name="resourcesToMonitor">The resources to be monitored.</param>
		/// <param name="callbackAction">Handler to call when any of the resources is uptdated</param>
		/// <returns><c>true</c> if the operation succeeded; <c>false</c> otherwise.</returns>
		bool TryStartMonitoring(IEnumerable<IResource> resourcesToMonitor, ResourceUpdatedHandler callbackAction);


		/// <summary>
		/// Stop monitoring the resources.
		/// </summary>
		void StopMonitoring(ResourceUpdatedHandler callbackAction);


		/// <summary>
		/// Gets a value indicating whether the monitor is enabled.
		/// </summary>
		bool IsEnabled { get; }
	}
}