// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Extension methods for <see cref="IConfiguration"/>.
/// </summary>
internal static class ConfigurationExtensions
{
	/// <summary>
	/// Gets the settings related to the specified type.
	/// </summary>
	/// <typeparam name="TSettings">The settings type.</typeparam>
	/// <param name="configuration">The configuration from which to retrieve the settings.</param>
	/// <returns>The retrieved or constructed settings.</returns>
	public static TSettings GetOrCreate<TSettings>(this IConfiguration configuration)
		where TSettings : new() =>
		configuration.Get<TSettings>() ?? new();
}
