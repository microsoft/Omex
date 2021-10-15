// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
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
		public void AddRegexLogScrubbingRule_RegistersLogger()
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddRegexLogScrubbingRule("valueToReplace", "replacementValue");

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(1, logScrubbingRules.Length);
			Assert.IsTrue(logScrubbingRules[0] is RegexLogScrubbingRule);
		}

		[TestMethod]
		public void AddIPv4AddressLogScrubbingRule_RegistersLogger()
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddIPv4AddressLogScrubbingRule();

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(1, logScrubbingRules.Length);
			Assert.IsTrue(logScrubbingRules[0] is RegexLogScrubbingRule);
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
			Assert.IsTrue(logScrubbingRules[0] is RegexLogScrubbingRule);
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
			Assert.IsTrue(logScrubbingRules[0] is RegexLogScrubbingRule);
			Assert.AreEqual("replacementValue", logScrubbingRules[0].Scrub("valueToReplace"));
			Assert.IsTrue(logScrubbingRules[1] is RegexLogScrubbingRule);
			Assert.AreEqual("[IPv4 ADDRESS]", logScrubbingRules[1].Scrub("0.0.0.0"));
			Assert.IsTrue(logScrubbingRules[2] is RegexLogScrubbingRule);
			Assert.AreEqual("[IPv6 ADDRESS]", logScrubbingRules[2].Scrub("1000::A01:1:AA10"));
		}

		private static ILogScrubbingRule[] GetTypeRegistrations(IServiceCollection collection)
		{
			IEnumerable<ILogScrubbingRule> objects = collection
				.AddOmexLogging()
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
	}
}
