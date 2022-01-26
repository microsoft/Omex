// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Omex.CodeGenerators.SettingsGen.Comparing;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.UnitTests.Comparing
{
	/// <summary>
	/// Unit tests for XmlComparison class
	/// </summary>
	[TestClass]
	public sealed class XmlComparisonUnitTests
	{
		[TestMethod]
		public void AreExistingSettingsEqual_TestFalse_NoSourceText()
		{
			XmlComparison<SettingsXmlModel> xmlComparison = new XmlComparison<SettingsXmlModel>();
			AdditionalText additionalText = new MockAdditionalText(string.Empty);
			Assert.IsFalse(xmlComparison.AreExistingSettingsEqual(new SettingsXmlModel(), additionalText), "Empty text should be false");
		}

		[TestMethod]
		public void AreExistingSettingsEqual_TestEqual_Empty()
		{
			XmlComparison<SettingsXmlModel> xmlComparison = new XmlComparison<SettingsXmlModel>();
			AdditionalText additionalText = new MockAdditionalText(string.Format(SettingElement, string.Empty));
			Assert.IsTrue(xmlComparison.AreExistingSettingsEqual(new SettingsXmlModel(), additionalText), "Empty settings should be equal");
		}

		[TestMethod]
		public void AreExistingSettingsEqual_TestEqual_WithParms()
		{
			XmlComparison<SettingsXmlModel> xmlComparison = new XmlComparison<SettingsXmlModel>();
			SettingsXmlModel settingsXmlModel = new();
			settingsXmlModel.Sections.Add(SectionModel);

			AdditionalText additionalText = new MockAdditionalText(string.Format(SettingElement, SectionWithParamXml));
			Assert.IsTrue(xmlComparison.AreExistingSettingsEqual(settingsXmlModel, additionalText), "Empty settings should be equal");
		}

		[TestMethod]
		public void AreExistingSettingsEqual_TestNotEqual()
		{
			XmlComparison<SettingsXmlModel> xmlComparison = new XmlComparison<SettingsXmlModel>();
			SettingsXmlModel settingsXmlModel = new();
			settingsXmlModel.Sections.Add(SectionModel);
			settingsXmlModel.Sections.Add(new SectionModel
			{
				Name = "Hello"
			});

			AdditionalText additionalText = new MockAdditionalText(string.Format(SettingElement, SectionWithParamXml));
			Assert.IsFalse(xmlComparison.AreExistingSettingsEqual(settingsXmlModel, additionalText), "Settings should not be equal");
		}

		[TestMethod]
		public void AreExistingSettingsEqual_TestInvalidXml()
		{
			string invalidXml = "hello";
			XmlComparison<SettingsXmlModel> xmlComparison = new XmlComparison<SettingsXmlModel>();
			SettingsXmlModel settingsXmlModel = new();
			settingsXmlModel.Sections.Add(SectionModel);
			settingsXmlModel.Sections.Add(new SectionModel
			{
				Name = "Hello"
			});

			AdditionalText additionalText = new MockAdditionalText(invalidXml);
			Assert.IsFalse(xmlComparison.AreExistingSettingsEqual(settingsXmlModel, additionalText));
		}

		public readonly SectionModel SectionModel = new SectionModel
		{
			Name = "NotExample",
			Parameters = new List<ParameterModel>
			{
				new ParameterModel
				{
					Name = "Setting1",
					Value = "0"
				},
				new ParameterModel
				{
					Name = "Setting2",
					Value = "false"
				}
			}

		};

		public string SettingElement => "<Settings xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"" +
			" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/2011/01/fabric\">{0}</Settings>";

		/// <summary>
		/// Section with parameters xml string
		/// </summary>
		public const string SectionWithParamXml = "  <Section Name=\"NotExample\">" +
			"\r\n    <Parameter Name=\"Setting1\" Value=\"0\" />\r\n    <Parameter Name=\"Setting2\" Value=\"false\" />" +
			"</Section>";
		
		/// <summary>
		/// MockAdditional text
		/// </summary>
		public class MockAdditionalText : AdditionalText
		{
			public string Text { get; private set; }

			public override string Path => string.Empty;

			public MockAdditionalText(string text)
			{
				Text = text;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="cancellationToken"></param>
			/// <returns></returns>
			public override SourceText? GetText(CancellationToken cancellationToken = default)
			{
				return string.IsNullOrEmpty(Text) ? null : SourceText.From(Text);
			}
		}
	}
}
