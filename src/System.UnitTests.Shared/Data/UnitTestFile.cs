// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.Data.FileSystem;

namespace Microsoft.Omex.System.UnitTests.Shared.Data.FileSystem
{
	/// <summary>
	/// Unit Test File Implementation
	/// </summary>
	public abstract class UnitTestFile : IFile
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fileExists">Does file exist</param>
		/// <param name="canBeRead">Can file be read</param>
		protected UnitTestFile(bool fileExists, bool canBeRead)
		{
			FileUnlocked = true;
			FileExists = fileExists;
			CanBeRead = canBeRead;
			LengthInBytes = 10;
		}

		/// <summary>
		/// Checks if file exists
		/// </summary>
		/// <param name="path">path</param>
		/// <returns>true if file exists, false otherwise</returns>
		public bool Exists(string path) => FileExists;

		/// <summary>
		/// Get the last write time for the file.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		/// <returns>The last write time for the file.</returns>
		public DateTime GetLastWriteTime(string path) => LastKnownWriteTime ?? DateTime.UtcNow;

		/// <summary>
		/// Get the length of the file in bytes.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		/// <returns>The length of the file in bytes.</returns>
		public long GetLength(string path) => LengthInBytes;

		/// <summary>
		/// Makes file unreadable
		/// </summary>
		public void CorruptFile() => CanBeRead = false;

		/// <summary>
		/// Makes file readable
		/// </summary>
		public void FixCorruptFile() => CanBeRead = true;

		/// <summary>
		/// Is the file unlocked?
		/// </summary>
		public bool FileUnlocked { get; set; }

		/// <summary>
		/// Gets the last known write time for the file.
		/// </summary>
		public DateTime? LastKnownWriteTime { get; set; }

		/// <summary>
		/// Gets the length of the file in bytes.
		/// </summary>
		public long LengthInBytes { get; set; }

		/// <summary>
		/// Returns file contents as byte array
		/// </summary>
		/// <param name="path">path</param>
		/// <returns>file contents</returns>
		public abstract byte[] ReadAllBytes(string path);

		/// <summary>
		/// Writes given bytes to file.
		/// When file exists it will be replaced with new content.
		/// </summary>
		/// <param name="path">Path to file.</param>
		/// <param name="bytes">Bytes to write to file.</param>
		public abstract void WriteAllBytes(string path, byte[] bytes);

		/// <summary>
		/// Does file exist
		/// </summary>
		protected bool FileExists { get; set; }

		/// <summary>
		/// Can file be read
		/// </summary>
		protected bool CanBeRead { get; set; }
	}
}
