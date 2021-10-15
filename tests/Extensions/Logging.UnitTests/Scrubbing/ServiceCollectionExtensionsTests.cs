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
		public void AddConstantLogScrubbingRule_RegistersLogger()
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddConstantLogScrubbingRule("valueToReplace", "replacementValue");

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(1, logScrubbingRules.Length);
			Assert.IsTrue(logScrubbingRules.Any(logScrubbingRule => logScrubbingRule is ConstantLogScrubbingRule));
		}

		[TestMethod]
		public void AddRegexLogScrubbingRule_RegistersLogger()
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddRegexLogScrubbingRule("valueToReplace", "replacementValue");

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(1, logScrubbingRules.Length);
			Assert.IsTrue(logScrubbingRules.Any(logScrubbingRule => logScrubbingRule is RegexLogScrubbingRule));
		}

		[TestMethod]
		public void AddIPv4AddressLogScrubbingRule_RegistersLogger()
		{
			ILoggingBuilder builder = new MockLoggingBuilder().AddIPv4AddressLogScrubbingRule();

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(1, logScrubbingRules.Length);
			Assert.IsTrue(logScrubbingRules.Any(logScrubbingRule => logScrubbingRule is IPv4AddressLogScrubbingRule));
		}

		[TestMethod]
		public void AddMultipleLogScrubbingRules_RegistersLoggers()
		{
			ILoggingBuilder builder = new MockLoggingBuilder()
				.AddConstantLogScrubbingRule("valueToReplace", "replacementValue")
				.AddRegexLogScrubbingRule("valueToReplace", "replacementValue")
				.AddIPv4AddressLogScrubbingRule();

			ILogScrubbingRule[] logScrubbingRules = GetTypeRegistrations(builder.Services);
			Assert.AreEqual(3, logScrubbingRules.Length);
			Assert.IsTrue(logScrubbingRules.Any(logScrubbingRule => logScrubbingRule is ConstantLogScrubbingRule));
			Assert.IsTrue(logScrubbingRules.Any(logScrubbingRule => logScrubbingRule is RegexLogScrubbingRule));
			Assert.IsTrue(logScrubbingRules.Any(logScrubbingRule => logScrubbingRule is IPv4AddressLogScrubbingRule));
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
