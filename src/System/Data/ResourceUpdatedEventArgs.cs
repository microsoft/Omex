// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Data
{
	/// <summary>
	/// Event arguments defining a data deployment resource change.
	/// </summary>
	public class ResourceUpdatedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceUpdatedEventArgs"/> class.
		/// </summary>
		/// <param name="details">The details of the updated resource.</param>
		/// <param name="isInitialLoad">A value indicating whether this is the initial load.</param>
		public ResourceUpdatedEventArgs(IDictionary<string, IResourceDetails> details, bool isInitialLoad)
		{
			Details = Code.ExpectsArgument(details, nameof(details), TaggingUtilities.ReserveTag(0x238506ca /* tag_97q1k */));

			IsInitialLoad = isInitialLoad;
		}


		/// <summary>
		/// Gets the details of the updated resource.
		/// </summary>
		public IDictionary<string, IResourceDetails> Details { get; }


		/// <summary>
		/// Gets a value indicating whether this is the initial load.
		/// </summary>
		public bool IsInitialLoad { get; }
	}
}