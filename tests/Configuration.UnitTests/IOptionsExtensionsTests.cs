// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Configuration.UnitTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Configuration.UnitTests
{
	[TestClass]
	public class IOptionsExtensionsTests
	{
		private static OptionsValidationException GetOptionsValidationException()
		{
			List<ValidationResult> validationResults = new();
			ConfigurationExample configuration = new()
			{
				ValidatedConfiguration = "",
				ValidatedInteger = -1
			};

			ValidationContext context = new(configuration, null, null);
			Validator.TryValidateObject(configuration, context, validationResults);

			return new OptionsValidationException(
				Options.DefaultName,
				typeof(ConfigurationExample),
				validationResults.Select(e => e.ErrorMessage));
		}

		[TestMethod]
		public void IOptions_SafeCurrentValue_ReturnsNullWhenInstanceInvalid()
		{
			OptionsValidationException exception = GetOptionsValidationException();
			Mock<IOptions<ConfigurationExample>> mock = new();
			mock.Setup(m => m.Value).Throws(exception);

			ConfigurationExample? configuration = mock.Object.SafeCurrentValue();

			Assert.IsNull(configuration);
		}

		[TestMethod]
		public void IOptions_SafeCurrentValue_ReturnsInstanceWhenValid()
		{
			ConfigurationExample configuration = new()
			{
				ValidatedConfiguration = "not-empty",
				ValidatedInteger = 5
			};
			Mock<IOptions<ConfigurationExample>> mock = new();
			mock.Setup(m => m.Value).Returns(configuration);

			ConfigurationExample? gotConfiguration = mock.Object.SafeCurrentValue();

			Assert.AreEqual(configuration.ValidatedConfiguration, gotConfiguration?.ValidatedConfiguration);
			Assert.AreEqual(configuration.UnvalidatedConfiguration, gotConfiguration?.UnvalidatedConfiguration);
			Assert.AreEqual(configuration.ValidatedInteger, gotConfiguration?.ValidatedInteger);
		}

		[TestMethod]
		public void IOptionsMonitor_SafeCurrentValue_ReturnsNullWhenInstanceInvalid()
		{
			OptionsValidationException exception = GetOptionsValidationException();
			Mock<IOptionsMonitor<ConfigurationExample>> mock = new();
			mock.Setup(m => m.CurrentValue).Throws(exception);

			ConfigurationExample? configuration = mock.Object.SafeCurrentValue();

			Assert.IsNull(configuration);
		}

		[TestMethod]
		public void IOptionsMonitor_SafeCurrentValue_ReturnsInstanceWhenValid()
		{
			ConfigurationExample configuration = new()
			{
				ValidatedConfiguration = "not-empty",
				ValidatedInteger = 5
			};
			Mock<IOptionsMonitor<ConfigurationExample>> mock = new();
			mock.Setup(m => m.CurrentValue).Returns(configuration);

			ConfigurationExample? gotConfiguration = mock.Object.SafeCurrentValue();

			Assert.AreEqual(configuration.ValidatedConfiguration, gotConfiguration?.ValidatedConfiguration);
			Assert.AreEqual(configuration.UnvalidatedConfiguration, gotConfiguration?.UnvalidatedConfiguration);
			Assert.AreEqual(configuration.ValidatedInteger, gotConfiguration?.ValidatedInteger);
		}

		[TestMethod]
		public void IOptions_SafeCurrentValue_WithLogger_ReturnsNullWhenInstanceInvalid()
		{
			LogCollectorLogger<IOptionsExtensionsTests> logger = new();
			OptionsValidationException exception = GetOptionsValidationException();
			Mock<IOptions<ConfigurationExample>> mock = new();
			mock.Setup(m => m.Value).Throws(exception);

			ConfigurationExample? configuration = mock.Object.SafeCurrentValue(logger);

			Assert.IsNull(configuration);

			Assert.AreEqual(1, logger.GetLogMessages().Count());
			Assert.AreEqual(1, logger.GetLogMessages(LogLevel.Error).Count());
			Assert.IsTrue(logger.GetLogMessages(LogLevel.Error).First()?.Contains(exception.Message));
		}

		[TestMethod]
		public void IOptions_SafeCurrentValue_WithLogger_ReturnsInstanceWhenValid()
		{
			LogCollectorLogger<IOptionsExtensionsTests> logger = new();
			ConfigurationExample configuration = new()
			{
				ValidatedConfiguration = "not-empty",
				ValidatedInteger = 5
			};
			Mock<IOptions<ConfigurationExample>> mock = new();
			mock.Setup(m => m.Value).Returns(configuration);

			ConfigurationExample? gotConfiguration = mock.Object.SafeCurrentValue(logger);

			Assert.AreEqual(configuration.ValidatedConfiguration, gotConfiguration?.ValidatedConfiguration);
			Assert.AreEqual(configuration.UnvalidatedConfiguration, gotConfiguration?.UnvalidatedConfiguration);
			Assert.AreEqual(configuration.ValidatedInteger, gotConfiguration?.ValidatedInteger);

			Assert.AreEqual(0, logger.GetLogMessages().Count());
		}

		[TestMethod]
		public void IOptionsMonitor_SafeCurrentValue_WithLogger_ReturnsNullWhenInstanceInvalid()
		{
			LogCollectorLogger<IOptionsExtensionsTests> logger = new();
			OptionsValidationException exception = GetOptionsValidationException();
			Mock<IOptionsMonitor<ConfigurationExample>> mock = new();
			mock.Setup(m => m.CurrentValue).Throws(exception);

			ConfigurationExample? configuration = mock.Object.SafeCurrentValue(logger);

			Assert.IsNull(configuration);

			Assert.AreEqual(1, logger.GetLogMessages().Count());
			Assert.AreEqual(1, logger.GetLogMessages(LogLevel.Error).Count());
			Assert.IsTrue(logger.GetLogMessages(LogLevel.Error).First()?.Contains(exception.Message));
		}

		[TestMethod]
		public void IOptionsMonitor_SafeCurrentValue_WithLogger_ReturnsInstanceWhenValid()
		{
			LogCollectorLogger<IOptionsExtensionsTests> logger = new();
			ConfigurationExample configuration = new()
			{
				ValidatedConfiguration = "not-empty",
				ValidatedInteger = 5
			};
			Mock<IOptionsMonitor<ConfigurationExample>> mock = new();
			mock.Setup(m => m.CurrentValue).Returns(configuration);

			ConfigurationExample? gotConfiguration = mock.Object.SafeCurrentValue(logger);

			Assert.AreEqual(configuration.ValidatedConfiguration, gotConfiguration?.ValidatedConfiguration);
			Assert.AreEqual(configuration.UnvalidatedConfiguration, gotConfiguration?.UnvalidatedConfiguration);
			Assert.AreEqual(configuration.ValidatedInteger, gotConfiguration?.ValidatedInteger);

			Assert.AreEqual(0, logger.GetLogMessages().Count());
		}
	}
}
