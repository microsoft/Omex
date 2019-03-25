// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Extensions
{
	/// <summary>
	/// Extension methods for the Assembly class
	/// </summary>
	public static class AssemblyExtensions
	{
		/// <summary>
		/// Loads an embedded resource and returns as a string
		/// </summary>
		/// <param name="assembly">The assembly to load from</param>
		/// <param name="resourceName">Name of the embedded resource</param>
		/// <returns>Resource as string, or null if not found</returns>
		public static string LoadEmbeddedResourceAsString(this Assembly assembly, string resourceName)
		{
			if (!Code.ValidateArgument(assembly, nameof(assembly), TaggingUtilities.ReserveTag(0x2385069f /* tag_97q05 */)) ||
				!Code.ValidateNotNullOrWhiteSpaceArgument(resourceName, nameof(resourceName), TaggingUtilities.ReserveTag(0x238506a0 /* tag_97q06 */)))
			{
				return null;
			}

			string[] resources = assembly.GetManifestResourceNames();
			if (!Array.Exists(resources, name => string.Equals(name, resourceName, StringComparison.Ordinal)))
			{
				// try resolving partial name

				string[] matches = Array.FindAll(resources, name => name.StartsWith(resourceName, StringComparison.Ordinal));

				if (matches.Length == 1)
				{
					resourceName = matches[0];
				}
				else
				{
					ULSLogging.LogTraceTag(0x23850386 /* tag_97qog */, Categories.Infrastructure, Levels.Error,
						"Embedded resource '{0}' was not found in '{1}'.", resourceName, assembly.FullName);

					return null;
				}
			}

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					ULSLogging.LogTraceTag(0x23850387 /* tag_97qoh */, Categories.Infrastructure, Levels.Error,
						"Embedded resource '{0}' could not be loaded from '{1}'.", resourceName, assembly.FullName);

					return null;
				}

				using (StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}