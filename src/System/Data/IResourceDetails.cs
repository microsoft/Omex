// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.System.Data
{
	/// <summary>
	/// The details of a loaded file resource.
	/// </summary>
	public interface IResourceDetails
	{
		/// <summary>
		/// Gets the last write time for the file.
		/// </summary>
		DateTime LastWrite { get; }


		/// <summary>
		/// Gets the length of the file in bytes.
		/// </summary>
		long Length { get; }


		/// <summary>
		/// Gets the file contents.
		/// </summary>
		byte[] Contents { get; }


		/// <summary>
		/// Gets the SHA256 hash of the file contents.
		/// </summary>
		string SHA256Hash { get; }
	}
}