/***************************************************************************
	FileResource.cs

	Class representing a file in data deployment
***************************************************************************/

#region Using directives

using System;
using System.IO;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

#endregion

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
			File = Code.ExpectsArgument(file, nameof(file), TaggingUtilities.ReserveTag(0x238506c7 /* tag_97q1h */));
			Folder = Code.ExpectsNotNullOrWhiteSpaceArgument(folder, nameof(folder), TaggingUtilities.ReserveTag(0x238506c8 /* tag_97q1i */));
			Name = Code.ExpectsNotNullOrWhiteSpaceArgument(name, nameof(name), TaggingUtilities.ReserveTag(0x238506c9 /* tag_97q1j */));

			try
			{
				Location = Path.Combine(Folder, Name);
			}
			catch (ArgumentException exception)
			{
				ULSLogging.ReportExceptionTag(0x23850392 /* tag_97qos */, Categories.ConfigurationDataSet, exception,
					"Exception constructing FileResource for folder: '{0}', name: '{1}'", Folder, Name);

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
				ULSLogging.LogTraceTag(0x23850393 /* tag_97qot */, Categories.ConfigurationDataSet, Levels.Verbose,
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
				ULSLogging.ReportExceptionTag(0x23850394 /* tag_97qou */, Categories.ConfigurationDataSet, exception,
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
	}
}
