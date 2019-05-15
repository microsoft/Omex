// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Omex.System.Extensions;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Data.Resources
{
	/// <summary>
	/// Class representing an embedded resource
	/// </summary>
	public class EmbeddedResource : IResource
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type">type to locate assembly</param>
		/// <param name="resourceName">resource name in assembly</param>
		public EmbeddedResource(Type type, string resourceName)
			: this(Code.ExpectsArgument(type, nameof(type), TaggingUtilities.ReserveTag(0)).Assembly, resourceName)
		{
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="assembly">assembly containing embedded resource</param>
		/// <param name="resourceName">resource name in assembly</param>
		public EmbeddedResource(Assembly assembly, string resourceName)
		{
			Assembly = Code.ExpectsArgument(assembly, nameof(assembly), TaggingUtilities.ReserveTag(0));
			Name = Code.ExpectsNotNullOrWhiteSpaceArgument(resourceName, nameof(resourceName), TaggingUtilities.ReserveTag(0));

			ResourceContent = new Lazy<Tuple<ResourceReadStatus, byte[]>>(() =>
			{
				ResourceReadStatus status = ResourceReadStatus.NotFound;
				byte[] content = null;
				try
				{
					string resourceContent = Assembly.LoadEmbeddedResourceAsString(Name);
					if (resourceContent != null)
					{
						content = Encoding.UTF8.GetBytes(resourceContent);
						status = ResourceReadStatus.Success;
					}
				}
				catch (Exception ex)
				{
					ULSLogging.ReportExceptionTag(0, Categories.ConfigurationDataSet, ex,
						"Unable to read resource {resourceName}", resourceName);
					status = ResourceReadStatus.ReadFailed;
				}

				return Tuple.Create(status, content);
			}, LazyThreadSafetyMode.PublicationOnly);
		}


		/// <summary>
		/// Resource name
		/// </summary>
		public string Name { get; }


		/// <summary>
		/// Resource location
		/// </summary>
		public string Location => Assembly.FullName;


		/// <summary>
		/// Gets the last write time to the file.
		/// </summary>
		public DateTime LastWriteTime => DateTime.MinValue;


		/// <summary>
		/// Gets the length of the file in bytes.
		/// </summary>
		public long Length => ResourceContent.Value.Item2?.Length ?? 0;


		/// <summary>
		/// Retrieves resource contents
		/// </summary>
		/// <param name="content">Resource content</param>
		/// <returns>Resource read status</returns>
		public ResourceReadStatus GetContent(out byte[] content)
		{
			content = ResourceContent.Value.Item2;
			return ResourceContent.Value.Item1;
		}


		/// <summary>
		/// Is a static resource
		/// </summary>
		public bool IsStatic => true;


		private Lazy<Tuple<ResourceReadStatus, byte[]>> ResourceContent { get; }


		private Assembly Assembly { get; }
	}
}