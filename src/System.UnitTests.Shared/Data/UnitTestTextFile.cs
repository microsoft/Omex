// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Text;

namespace Microsoft.Omex.System.UnitTests.Shared.Data.FileSystem
{
	/// <summary>
	/// A unit test text file wrapper.
	/// </summary>
	public class UnitTestTextFile : UnitTestFile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UnitTestTextFile"/> class.
		/// </summary>
		/// <param name="fileExists">Does the file exist?</param>
		/// <param name="canBeRead">Can the file be read?</param>
		/// <param name="contents">The contents.</param>
		public UnitTestTextFile(bool fileExists, bool canBeRead, string contents)
			: base(fileExists, canBeRead) => Contents = contents;


		/// <summary>
		/// Returns the file contents as byte array.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>The file contents.</returns>
		public override byte[] ReadAllBytes(string path)
		{
			if (!CanBeRead)
			{
				throw new IOException();
			}

			return Encoding.UTF8.GetBytes(Contents ?? string.Empty);
		}


		/// <summary>
		/// Writes given bytes to file.
		/// When file exists it will be replaced with new content.
		/// </summary>
		/// <param name="path">Path to file.</param>
		/// <param name="bytes">Bytes to write to file.</param>
		public override void WriteAllBytes(string path, byte[] bytes) => Contents = Encoding.UTF8.GetString(bytes);


		/// <summary>
		/// The contents.
		/// </summary>
		private string Contents { get; set; }
	}
}