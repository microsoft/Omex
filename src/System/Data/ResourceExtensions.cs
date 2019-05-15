// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Data
{
	/// <summary>
	/// Extension methods for resource classes.
	/// </summary>
	public static class ResourceExtensions
	{
		/// <summary>
		/// Read the content of a resource
		/// </summary>
		/// <param name="resource">resource</param>
		/// <returns>read status and resource details</returns>
		public static Tuple<ResourceReadStatus, IResourceDetails> Read(this IResource resource)
		{
			if (!Code.ValidateArgument(resource, nameof(resource), TaggingUtilities.ReserveTag(0)))
			{
				return Tuple.Create<ResourceReadStatus, IResourceDetails>(ResourceReadStatus.ReadFailed, null);
			}

			ResourceReadStatus status = resource.GetContent(out byte[] content);

			return Tuple.Create(
				status,
				(IResourceDetails)new ResourceDetails(resource.LastWriteTime, content?.LongLength ?? 0, content ?? new byte[] { }));
		}
	}
}