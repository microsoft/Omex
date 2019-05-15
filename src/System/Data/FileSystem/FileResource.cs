// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Data.FileSystem
{
	/// <summary>
	/// Class representing a file in data deployment
	/// </summary>
	public class FileResource : IResource
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="file">File adaptor</param>
		/// <param name="folder">Folder</param>
		/// <param name="name">File Name including extension</param>
		public FileResource(IFile file, string folder, string name)
		{
			File = Code.ExpectsArgument(file, nameof(file), TaggingUtilities.ReserveTag(0x238208d2 /* tag_9669s */));
			Folder = Code.ExpectsNotNullOrWhiteSpaceArgument(folder, nameof(folder), TaggingUtilities.ReserveTag(0x238208d3 /* tag_9669t */));
			Name = Code.ExpectsNotNullOrWhiteSpaceArgument(name, nameof(name), TaggingUtilities.ReserveTag(0x238208d4 /* tag_9669u */));

			bool locationSet = false;
			try
			{
				if (!HasInvalidPathChars(Folder) && !HasInvalidPathChars(Name))
				{
					Location = Path.Combine(Folder, Name);
					locationSet = true;
				}
				else
				{
					ULSLogging.LogTraceTag(0x238208d5 /* tag_9669v */, Categories.ConfigurationDataSet, Levels.Error,
						"Failed to combine FileResource.Location for folder: '{0}', name: '{1}'", Folder, Name);
				}
			}
			catch (ArgumentException exception)
			{
				ULSLogging.ReportExceptionTag(0x238208d6 /* tag_9669w */, Categories.ConfigurationDataSet, exception,
					"Exception constructing FileResource for folder: '{0}', name: '{1}'", Folder, Name);
			}

			if (!locationSet)
			{
				Location = Name;
			}
		}


		/// <summary>
		/// Retrieves resource contents
		/// </summary>
		/// <param name="content">Resource content</param>
		/// <returns>Resource read status</returns>
		public ResourceReadStatus GetContent(out byte[] content)
		{
			content = null;
			if (!File.Exists(Location))
			{
				ULSLogging.LogTraceTag(0x238208d7 /* tag_9669x */, Categories.ConfigurationDataSet, Levels.Verbose,
					"FileResource does not exist: '{0}'", Location);
				return ResourceReadStatus.NotFound;
			}

			try
			{
				content = File.ReadAllBytes(Location);
				return ResourceReadStatus.Success;
			}
			catch (Exception exception)
			{
				ULSLogging.ReportExceptionTag(0x238208d8 /* tag_9669y */, Categories.ConfigurationDataSet, exception,
					"Exception reading file '{0}'", Location);

				content = null;
				return ResourceReadStatus.ReadFailed;
			}
		}


		/// <summary>
		/// Resource Name
		/// </summary>
		public string Name { get; }


		/// <summary>
		/// Resource location
		/// </summary>
		public string Location { get; }


		/// <summary>
		/// File folder
		/// </summary>
		public string Folder { get; }


		/// <summary>
		/// Gets the last write time to the file.
		/// </summary>
		public DateTime LastWriteTime => File.GetLastWriteTime(Location);


		/// <summary>
		/// Gets the length of the file in bytes.
		/// </summary>
		public long Length => File.GetLength(Location);


		/// <summary>
		/// Is a static resource
		/// </summary>
		public bool IsStatic => false;


		/// <summary>
		/// File adaptor
		/// </summary>
		protected IFile File { get; }


		/// <summary>
		/// Check if path contains invalid path chars
		/// Method created since Path.Combine in .netcore does not throw exception in case of illegal chars so check should be done manually
		/// </summary>
		private static bool HasInvalidPathChars(string path) => path.Any(s_InvalidPathCharsForFullFramework.Contains);


		private static readonly ISet<char> s_InvalidPathCharsForFullFramework = new HashSet<char>(Path.GetInvalidPathChars()) { '"', '<', '>' };
	}
}
