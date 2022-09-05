// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.UnitTests.FileGeneration
{
	public sealed class XmlFileGenerationUnitTests
	{

		[TestMethod]
		public void XmlFileGeneration_TestWriteToFile_Succeed()
		{
			XmlFileGeneration<SettingsXmlModel> generator = new();
			generator.GenerateFile(new SettingsXmlModel(), ".\\text.xml");
		}

		[TestMethod]
		public void XmlFileGeneration_TestWriteToFile_Fail()
		{
			XmlFileGeneration<SettingsXmlModel> generator = new();
			Assert.ThrowsException<ArgumentException>(() => generator.GenerateFile(new SettingsXmlModel(), string.Empty));
		}
	}
}
