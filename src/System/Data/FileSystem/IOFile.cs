// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;

namespace Microsoft.Omex.System.Data.FileSystem
{
	/// <summary>
	/// Adapter for System.IO.File
	/// </summary>
	public class IOFile : IFile
	{
		/// <summary>
		/// Calls File.Exists
		/// </summary>
		/// <param name="path">path</param>
		/// <returns>value returned by File.Exists method</returns>
		public bool Exists(string path) => File.Exists(path);

		/// <summary>
		/// Calls File.ReadAllBytes
		/// </summary>
		/// <param name="path">path</param>
		/// <returns>value returned by File.ReadAllBytes method</returns>
		public byte[] ReadAllBytes(string path)
		{
			byte[] bytes;
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					bytes = memoryStream.ToArray();
				}
			}

			return bytes;
		}

		/// <summary>
		/// Writes given content to file.
		/// When file exists it will be replaced with new content.
		/// </summary>
		/// <param name="path">Path to file.</param>
		/// <param name="content">Content to write to file.</param>
		public void WriteAllBytes(string path, byte[] content)
		{
			using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				fileStream.Write(content, 0, content.Length);
			}
		}

		/// <summary>
		/// Get the last write time for the file.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		/// <returns>The last write time for the file.</returns>
		public DateTime GetLastWriteTime(string path) => File.GetLastWriteTimeUtc(path);

		/// <summary>
		/// Get the length of the file in bytes.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		/// <returns>The length of the file in bytes.</returns>
		public long GetLength(string path) => new FileInfo(path).Length;
	}
}
