// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.System.Data.FileSystem
{
	/// <summary>
	/// Interface abstracting System.IO.File methods
	/// </summary>
	public interface IFile
	{
		/// <summary>
		/// Checks if file exists
		/// </summary>
		/// <param name="path">path</param>
		/// <returns>true if file exists, false otherwise</returns>
		bool Exists(string path);


		/// <summary>
		/// Returns file contents as byte array
		/// </summary>
		/// <param name="path">path</param>
		/// <returns>file contents</returns>
		byte[] ReadAllBytes(string path);


		/// <summary>
		/// Writes given content to file.
		/// When file exists it will be replaced with new content.
		/// </summary>
		/// <param name="path">Path to file.</param>
		/// <param name="value">Content to write to file.</param>
		void WriteAllBytes(string path, byte[] value);


		/// <summary>
		/// Get the last write time for the file.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		/// <returns>The last write time for the file.</returns>
		DateTime GetLastWriteTime(string path);


		/// <summary>
		/// Get the length of the file in bytes.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		/// <returns>The length of the file in bytes.</returns>
		long GetLength(string path);
	}
}