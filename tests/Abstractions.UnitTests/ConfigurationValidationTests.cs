// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Omex.Extensions.Abstractions.UnitTests
{
	[TestClass]
	public class ConfigurationValidationTests
	{
		[TestMethod]
		public void GetNonNullableOrThrow_WhenValueNotNull()
		{
			// prepare mocks
			string sectionKeyInt = "SectionKeyInt";
			string sectionKeyString = "SectionKeyString";
			string configValueInt = "5";
			string configValueString = "asdf";

			Mock<IConfigurationSection> mockSectionInt = new Mock<IConfigurationSection>();
			Mock<IConfigurationSection> mockSectionString = new Mock<IConfigurationSection>();
			Mock<IConfiguration> mockConfig = new Mock<IConfiguration>();

			mockSectionInt.Setup(x => x.Value).Returns(configValueInt);
			mockSectionString.Setup(x => x.Value).Returns(configValueString);
			mockConfig.Setup(x => x.GetSection(It.Is<string>(k => k == sectionKeyInt))).Returns(mockSectionInt.Object);
			mockConfig.Setup(x => x.GetSection(It.Is<string>(k => k == sectionKeyString))).Returns(mockSectionString.Object);

			// assert
			int resultInt = ConfigurationValidation.GetNonNullableOrThrow<int>(mockConfig.Object, sectionKeyInt);
			string resultString = ConfigurationValidation.GetNonNullableOrThrow<string>(mockConfig.Object, sectionKeyString);
			Assert.AreEqual(resultInt, int.Parse(configValueInt));
			Assert.AreEqual(resultString, configValueString);
		}


		[TestMethod]
		public void GetNonNullableOrThrow_WhenValueNull()
		{
			// prepare mocks
			string sectionKey = "SectionKey";

			Mock<IConfigurationSection> mockSection = new Mock<IConfigurationSection>();
			Mock<IConfiguration> mockConfig = new Mock<IConfiguration>();

			mockSection.Setup(x => x.Value).Returns((string?)null);
			mockConfig.Setup(x => x.GetSection(It.Is<string>(k => k == sectionKey))).Returns(mockSection.Object);

			// assert
			Assert.ThrowsException<InvalidOperationException>(() => ConfigurationValidation.GetNonNullableOrThrow<string>(mockConfig.Object, sectionKey));
		}

		[TestMethod]
		public void GetNonNullableOrThrow_WhenWrongType()
		{
			// prepare mocks
			string sectionKey = "SectionKey";
			string sectionKeyString = "SectionKeyString";

			Mock<IConfigurationSection> mockSection = new Mock<IConfigurationSection>();
			Mock<IConfiguration> mockConfig = new Mock<IConfiguration>();

			mockSection.Setup(x => x.Value).Returns(sectionKeyString);
			mockConfig.Setup(x => x.GetSection(It.Is<string>(k => k == sectionKey))).Returns(mockSection.Object);

			// assert
			Assert.ThrowsException<InvalidOperationException>(() => ConfigurationValidation.GetNonNullableOrThrow<int>(mockConfig.Object, sectionKey));
		}
	}
}
