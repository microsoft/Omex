// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;
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
		/// Adds a regular expression log scrubbing rule.
		/// </summary>
		/// <param name="builder">The extension method argument.</param>
		/// <param name="regexToReplace">The regular expression specifying the strings to replace.</param>
		/// <param name="matchEvaluator">Custom logic for regex replace.</param>
		/// <remarks>
		/// Please be advised that using MatchEvaluator for Regex replace is a
		/// more expensive operation compared to simple string replace of the entire regex. After running
		/// unit test profiler locally, we can see there is roughly a 31% increase when using the evaluator. 
		/// </remarks>
		public static ILoggingBuilder AddRegexLogScrubbingRule(this ILoggingBuilder builder, string regexToReplace, MatchEvaluator matchEvaluator)
		{
			builder.Services.AddSingleton<ILogScrubbingRule>(_ => new RegexLogScrubbingRule(regexToReplace, matchEvaluator));
			return builder;
		}

		/// <summary>
		/// Adds an IPv4 address log scrubbing rule, which replaces IPv4 addresses with "[IPv4 ADDRESS]".
		/// </summary>
		/// <param name="builder">The extension method argument.</param>
		public static ILoggingBuilder AddIPv4AddressLogScrubbingRule(this ILoggingBuilder builder)
		{
			builder.Services.AddSingleton<ILogScrubbingRule>(
				_ => new RegexLogScrubbingRule(
					"(\\d{1,3}\\.){3}\\d{1,3}",
					"[IPv4 ADDRESS]"));
			return builder;
		}

		/// <summary>
		/// Adds an IPv6 address log scrubbing rule, which replaces IPv6 addresses with "[IPv6 ADDRESS]".
		/// </summary>
		/// <param name="builder">The extension method argument.</param>
		public static ILoggingBuilder AddIPv6AddressLogScrubbingRule(this ILoggingBuilder builder)
		{
			builder.Services.AddSingleton<ILogScrubbingRule>(
				_ => new RegexLogScrubbingRule(
					"(?:(?:(?:[a-f0-9]{1,4}:){6}|(?=(?:[a-f0-9]{0,4}:){0,6}(?:[0-9]{1,3}\\.){3}[0-9]{1,3}(?![:.\\w]))(([a-f0-9]{1,4}:){0,5}|:)((:[a-f0-9]{1,4}){1,5}:|:)|::(?:[a-f0-9]{1,4}:){5})(?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|(?:[a-f0-9]{1,4}:){7}[a-f0-9]{1,4}|(?=(?:[a-f0-9]{0,4}:){0,7}[a-f0-9]{0,4}(?![:.\\w]))(([a-f0-9]{1,4}:){1,7}|:)((:[a-f0-9]{1,4}){1,7}|:)|(?:[a-f0-9]{1,4}:){7}:|:(:[a-f0-9]{1,4}){7})(?![:.\\w])",
					"[IPv6 ADDRESS]"));
			return builder;
		}
	}
}
