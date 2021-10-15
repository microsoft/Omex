// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging.Scrubbing
{
	/// <summary>
	/// Extension methods for the <see cref="IServiceCollection"/> class related to log scrubbing.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds a constant log scrubbing rule.
		/// </summary>
		/// <param name="builder">The extension method argument.</param>
		/// <param name="valueToReplace">The value to be replaced.</param>
		/// <param name="replacementValue">The value with which to replace the matching text.</param>
		public static ILoggingBuilder AddConstantLogScrubbingRule(this ILoggingBuilder builder, string valueToReplace, string replacementValue)
		{
			builder.Services.AddSingleton<ILogScrubbingRule>(_ => new ConstantLogScrubbingRule(valueToReplace, replacementValue));
			return builder;
		}

		/// <summary>
		/// Adds a regular expression log scrubbing rule.
		/// </summary>
		/// <param name="builder">The extension method argument.</param>
		/// <param name="regexToReplace">The regular expression specifying the strings to replace.</param>
		/// <param name="replacementValue">The value with which to replace the matching text.</param>
		public static ILoggingBuilder AddRegexLogScrubbingRule(this ILoggingBuilder builder, string regexToReplace, string replacementValue)
		{
			builder.Services.AddSingleton<ILogScrubbingRule>(_ => new RegexLogScrubbingRule(regexToReplace, replacementValue));
			return builder;
		}

		/// <summary>
		/// Adds an IPv4 address log scrubbing rule.
		/// </summary>
		/// <param name="builder">The extension method argument.</param>
		public static ILoggingBuilder AddIPv4AddressLogScrubbingRule(this ILoggingBuilder builder)
		{
			builder.Services.AddSingleton<ILogScrubbingRule, IPv4AddressLogScrubbingRule>();
			return builder;
		}
	}
}
