// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Data.FileSystem;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.UnitTests.Shared.Data.FileSystem;

#endregion

namespace Microsoft.Omex.System.UnitTests.Shared.Configuration
{
	/// <summary>
	/// Embedded resources handling
	/// </summary>
	public static class EmbeddedResources
	{
		/// <summary>
		/// Loads FileResource from embedded resources
		/// </summary>
		/// <param name="resourceName">Resource name</param>
		/// <param name="type">Type to determine assembly</param>
		/// <returns>Loaded resource</returns>
		public static FileResource GetEmbeddedResource(string resourceName, Type type)
		{
			return new FileResource(new UnitTestTextFile(true, true, GetEmbeddedResourceAsString(resourceName, type)), "TestFolder", resourceName);
		}


		/// <summary>
		/// Loads embedded resource
		/// </summary>
		/// <param name="resourceName">Resource name</param>
		/// <param name="type">Type to determine assembly</param>
		/// <returns>Resource contents as string</returns>
		public static string GetEmbeddedResourceAsString(string resourceName, Type type)
		{
			using (Stream stream = type.Assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					ULSLogging.LogTraceTag(0, Categories.Infrastructure,
						Levels.Error, "Cannot find embedded resource '{0}' in assembly '{1}'.",
						resourceName, type.Assembly);
					return null;
				}

				using (StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}


		/// <summary>
		/// Loads embedded resource as a byte array.
		/// </summary>
		/// <param name="resourceName">The resource name.</param>
		/// <param name="type">The type from which to determine the assembly.</param>
		/// <returns>The resource contents as a byte array.</returns>
		public static byte[] GetEmbeddedResourceAsByteArray(string resourceName, Type type)
		{
			List<byte> result = new List<byte>();
			using (Stream stream = type.Assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					ULSLogging.LogTraceTag(0, Categories.Infrastructure,
						Levels.Error, "Cannot find embedded resource '{0}' in type '{1}' parent assembly '{2}'.",
						resourceName, type, type.Assembly);
					return null;
				}

				int character = stream.ReadByte();
				while (character != -1)
				{
					result.Add((byte)character);
					character = stream.ReadByte();
				}
			}

			return result.ToArray();
		}


		/// <summary>
		/// Loads an embedded resource as a FileResourceDetails object.
		/// </summary>
		/// <param name="resourceName">The resource name.</param>
		/// <param name="type">A type, used to determine the containing assembly.</param>
		/// <returns>The resource details.</returns>
		public static IResourceDetails GetEmbeddedResourceAsResourceDetails(string resourceName, Type type)
		{
			return CreateResourceDetails(GetEmbeddedResourceAsByteArray(resourceName, type));
		}


		/// <summary>
		/// Creates the blob resource details.
		/// </summary>
		/// <param name="contents">The file contents.</param>
		/// <returns>The resource details.</returns>
		public static IResourceDetails CreateResourceDetails(string contents)
		{
			return CreateResourceDetails(Encoding.UTF8.GetBytes(contents));
		}


		/// <summary>
		/// Creates the file resource details.
		/// </summary>
		/// <param name="contents">The file contents.</param>
		/// <returns>The resource details.</returns>
		public static IResourceDetails CreateResourceDetails(byte[] contents)
		{
			return new ResourceDetails(DateTime.UtcNow, 1, contents);
		}
	}
}