// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Logging.Scrubbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests.Scrubbing
{
	[TestClass]
	public class ServiceCollectionTests
	{
		[TestMethod]
		public void WithNoRules_ReturnsEmptyList()
		{
			ILoggingBuilder builder = new MockLoggingBuilder();

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(0, logScrubbingRules.Length);
		}

		[TestMethod]
		public void AddRegexLogScrubbingRule_RegistersLogger()
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddRegexLogScrubbingRule("valueToReplace", "replacementValue");

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(1, logScrubbingRules.Length);
			Assert.IsInstanceOfType(logScrubbingRules[0], typeof(RegexLogScrubbingRule));
		}

		[TestMethod]
		public void AddIPv4AddressLogScrubbingRule_RegistersLogger()
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddIPv4AddressLogScrubbingRule();

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(1, logScrubbingRules.Length);
			Assert.IsInstanceOfType(logScrubbingRules[0], typeof(RegexLogScrubbingRule));
		}

		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "input")]
		[DataRow("1.2.3", "1.2.3")]
		[DataRow("1.2345.6.7", "1.2345.6.7")]
		[DataRow("1.2.3456.7", "1.2.3456.7")]
		[DataRow("0.0.0.0", "[IPv4 ADDRESS]")]
		[DataRow("100.100.100.100", "[IPv4 ADDRESS]")]
		[DataRow("0.0.0.0 100.100.100.100", "[IPv4 ADDRESS] [IPv4 ADDRESS]")]
		public void AddIPv4AddressLogScrubbingRule_Scrubs(string input, string expected)
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddIPv4AddressLogScrubbingRule();

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(expected, logScrubbingRules[0].Scrub(input));
		}

		[TestMethod]
		public void AddIPv6AddressLogScrubbingRule_RegistersLogger()
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddIPv6AddressLogScrubbingRule();

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(1, logScrubbingRules.Length);
			Assert.IsInstanceOfType(logScrubbingRules[0], typeof(RegexLogScrubbingRule));
		}

		[DataTestMethod]
		[DataRow("", "")]
		[DataRow(" ", " ")]
		[DataRow("input", "input")]
		[DataRow("1000::A01:1:AA10", "[IPv6 ADDRESS]")]
		[DataRow("1000::a01:1:aa10", "[IPv6 ADDRESS]")]
		[DataRow("1000::A01:100.0.0.0", "[IPv6 ADDRESS]")]
		[DataRow("1000::a01:100.0.0.0", "[IPv6 ADDRESS]")]
		[DataRow("1000:0:0:0:0:A01:1:AA10", "[IPv6 ADDRESS]")]
		[DataRow("1000:0:0:0:0:a01:1:aa10", "[IPv6 ADDRESS]")]
		[DataRow("1000:0:0:0:0:A01:100.0.0.0", "[IPv6 ADDRESS]")]
		[DataRow("1000:0:0:0:0:a01:100.0.0.0", "[IPv6 ADDRESS]")]
		[DataRow("1000:0:0:0:0:a01:100.0.0.0 input", "[IPv6 ADDRESS] input")]
		public void AddIPv6AddressLogScrubbingRule_Scrubs(string input, string expected)
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddIPv6AddressLogScrubbingRule();

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(expected, logScrubbingRules[0].Scrub(input));
		}

		[TestMethod]
		public void AddMultipleLogScrubbingRules_RegistersLoggersInCorrectOrder()
		{
			ILoggingBuilder builder = new MockLoggingBuilder()
				.AddRegexLogScrubbingRule("valueToReplace", "replacementValue")
				.AddIPv4AddressLogScrubbingRule()
				.AddIPv6AddressLogScrubbingRule();

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(3, logScrubbingRules.Length);
			Assert.IsInstanceOfType(logScrubbingRules[0], typeof(RegexLogScrubbingRule));
			Assert.AreEqual("replacementValue", logScrubbingRules[0].Scrub("valueToReplace"));
			Assert.IsInstanceOfType(logScrubbingRules[1], typeof(RegexLogScrubbingRule));
			Assert.AreEqual("[IPv4 ADDRESS]", logScrubbingRules[1].Scrub("0.0.0.0"));
			Assert.IsInstanceOfType(logScrubbingRules[2], typeof(RegexLogScrubbingRule));
			Assert.AreEqual("[IPv6 ADDRESS]", logScrubbingRules[2].Scrub("1000::A01:1:AA10"));
		}

		[TestMethod]
		[DataRow("/api/path?q1=v1&q2=v2&q3=v3", "/api/path?q1=REDACTED&q2=REDACTED&q3=v3")]
		[DataRow("/api/path?q1=v1&q2=v2", "/api/path?q1=REDACTED&q2=REDACTED")]
		[DataRow("/api/path?q3=v3", "/api/path?q3=v3")]
		public void AddRegexLogScrubbingRule_WithMatchEvaluator_Scrubs(string input, string expected)
		{
			MatchEvaluator matchEvaluator = new((match) => match.Groups[1].Value + "=REDACTED" + match.Groups[3].Value);

			ILoggingBuilder builder2 = new MockLoggingBuilder()
				.AddRegexLogScrubbingRule(Regex, matchEvaluator);

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder2.Services);

			Assert.AreEqual(expected, logScrubbingRules[0].Scrub(input));
		}

		[TestMethod]
		[DataRow("/api/path?q1=v1&q2=v2&q3=v3", "/api/path?REDACTED&REDACTED&q3=v3")]
		[DataRow("/api/path?q1=v1&q2=v2", "/api/path?REDACTED&REDACTED&")]
		[DataRow("/api/path?q3=v3", "/api/path?q3=v3")]
		public void AddRegexLogScrubbingRule_Scrubs(string input, string expected)
		{
			ILoggingBuilder builder2 = new MockLoggingBuilder()
				.AddRegexLogScrubbingRule(Regex,"REDACTED&");

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder2.Services);

			Assert.AreEqual(expected, logScrubbingRules[0].Scrub(input));
		}

		private static ILogScrubbingRule[] GetTypeRegistrations(IServiceCollection collection)
		{
			IEnumerable<ILogScrubbingRule> objects = collection
				.AddOmexLogging(null!)
				.BuildServiceProvider(new ServiceProviderOptions
				{
					ValidateOnBuild = true,
					ValidateScopes = true
				}).GetServices<ILogScrubbingRule>();

			Assert.IsNotNull(objects);
			return objects.ToArray();
		}

		private class MockLoggingBuilder : ILoggingBuilder
		{
			public IServiceCollection Services { get; } = new ServiceCollection();
		}

		private const string Regex = "(q1|q2)=(.+?)(&|$)";
	}
}
