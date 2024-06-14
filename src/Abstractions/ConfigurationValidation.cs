// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Class provides common validation methods for Microsoft.Extensions.IConfiguration
	/// </summary>
	public static class ConfigurationValidation
	{
		/// <summary>
		/// Omex workaround around IConfigurationSection.Get() returning null variables.
		/// </summary>
		/// <typeparam name="T">Type needed from the configuration section</typeparam>
		/// <param name="configuration">IConfiguration object</param>
		/// <param name="sectionName">Name of configuration section to get the object from</param>
		/// <returns>Non-nullable object of type T</returns>
		/// <exception cref="ArgumentNullException">If the type T is not found in the given section of the configuration</exception>
		public static T GetNonNullableOrThrow<T>(IConfiguration configuration, string sectionName)
		{
			return configuration.GetSection(sectionName).Get<T>()
				?? throw new InvalidOperationException($"{nameof(T)} missing in the {sectionName} section of the configuration");
		}
	}
}
