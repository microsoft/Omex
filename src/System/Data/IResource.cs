// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.System.Data
{
	/// <summary>
	/// Data deployment resource
	/// </summary>
	public interface IResource
	{
		/// <summary>
		/// Retrieves resource contents
		/// </summary>
		/// <param name="content">Resource content</param>
		/// <returns>Resource read status</returns>
		ResourceReadStatus GetContent(out byte[] content);


		/// <summary>
		/// Resource name
		/// </summary>
		string Name { get; }


		/// <summary>
		/// Resource location
		/// </summary>
		string Location { get; }


		/// <summary>
		/// Gets the last write time to the file.
		/// </summary>
		DateTime LastWriteTime { get; }


		/// <summary>
		/// Gets the length of the file in bytes.
		/// </summary>
		long Length { get; }


		/// <summary>
		/// Is a static resource
		/// </summary>
		bool IsStatic { get; }
	}
}