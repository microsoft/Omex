﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Configuration
{
	/// <summary>
	/// A collection of extension methods to safely get the configuration values.
	/// </summary>
	public static class IOptionsExtensions
	{
		/// <summary>
		/// Returns the <seealso cref="IOptionsMonitor{TOptions}.CurrentValue"/> from the given options.
		/// If the options value does not pass the validation set while configuring the configuration at startup,
		/// it will return null instead of throwing <seealso cref="OptionsValidationException"/>.
		/// </summary>
		/// <typeparam name="TOption">The type that parses the configuration.</typeparam>
		/// <param name="options">The <seealso cref="IOptionsMonitor{TOptions}"/> instance.</param>
		/// <param name="logger">
		/// An <seealso cref="ILogger"/> instance that, if passed in input, will be used
		/// to log the details of the validation exception if thrown.
		/// </param>
		/// <returns>The configuration value if the validation passes, <c>null</c> otherwise.</returns>
		public static TOption? SafeGetCurrentValue<TOption>(this IOptionsMonitor<TOption> options, ILogger? logger = default)
			where TOption : class
		{
			try
			{
				return options.CurrentValue;
			}
			catch (OptionsValidationException ex)
			{
				logger?.LogError(Tag.ReserveTag(0), ex, "The configuration validation threw an exception: '{ExceptionMessage}'.", ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Returns the <seealso cref="IOptions{TOptions}.Value"/> from the given options.
		/// If the options value does not pass the validation set while configuring the configuration at startup,
		/// it will return null instead of throwing <seealso cref="OptionsValidationException"/>.
		/// </summary>
		/// <typeparam name="TOption">The type that parses the configuration.</typeparam>
		/// <param name="options">The <seealso cref="IOptions{TOptions}"/> instance.</param>
		/// <param name="logger">
		/// An <seealso cref="ILogger"/> instance that, if passed in input, will be used
		/// to log the details of the validation exception if thrown.
		/// </param>
		/// <returns>The configuration value if the validation passes, <c>null</c> otherwise.</returns>
		public static TOption? SafeGetValue<TOption>(this IOptions<TOption> options, ILogger? logger = default)
			where TOption : class
		{
			try
			{
				return options.Value;
			}
			catch (OptionsValidationException ex)
			{
				logger?.LogError(Tag.ReserveTag(0), ex, "The configuration validation threw an exception: '{ExceptionMessage}'.", ex.Message);
				return null;
			}
		}
	}
}
