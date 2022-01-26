// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes;
using Microsoft.Omex.CodeGenerators.SettingsGen.Parser;
using Microsoft.Omex.CodeGenerators.SettingsGen.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.UnitTests.Parser
{
    public sealed class SettingsFromAttributeParserUnitTests
    {
		private Mock<IContextWrapper> m_contextWrapper = new Mock<IContextWrapper>();

		[TestMethod]
		public void Constructor_TestEmptySet()
		{
			Assert.ThrowsException<ArgumentException>(() => new SettingsFromAttributeParser(new HashSet<string> { }, m_contextWrapper.Object));
		}

		[TestMethod]
		public void OnVisitSyntax_TestNoSection()
		{
			Mock<INamedTypeSymbol> mock = new Mock<INamedTypeSymbol>();
			mock.Setup(x => x.Name).Returns("Hello");

			m_contextWrapper.Setup(x => x.GetAttributes(It.IsAny<GeneratorSyntaxContext>())).Returns(
				(new List<IAttributeWrapper>(), mock.Object));

			SettingsFromAttributeParser parser = new SettingsFromAttributeParser(new HashSet<string> {
				"Testing"
			}, m_contextWrapper.Object);

			parser.OnVisitSyntaxNode(new GeneratorSyntaxContext());
			Assert.AreEqual(0, parser.Classes.Count);
		}

		[TestMethod]
		public void OnVisitSyntax_TestHasSections_WithNameAttribute()
		{
			Mock<INamedTypeSymbol> mock = new Mock<INamedTypeSymbol>();
			mock.Setup(x => x.Name).Returns("Hello");

			Mock<IAttributeWrapper> mockAttr = new Mock<IAttributeWrapper>();
			mockAttr.Setup(x => x.Name).Returns("Testing");
			mockAttr.Setup(x => x.Arguments).Returns(new Dictionary<string, string> { {"Name", "Override" } });

			m_contextWrapper.Setup(x => x.GetAttributes(It.IsAny<GeneratorSyntaxContext>())).Returns(
				(new List<IAttributeWrapper> { mockAttr.Object }, mock.Object));

			SettingsFromAttributeParser parser = new SettingsFromAttributeParser(new HashSet<string> {
				"Testing"
			}, m_contextWrapper.Object);
			parser.OnVisitSyntaxNode(new GeneratorSyntaxContext());

			Assert.AreEqual(1, parser.Classes.Count);
			Assert.AreEqual("Override", parser.Classes.First().sectionName);
		}

		[TestMethod]
		public void OnVisitSyntax_TestHasSections_WithNoNameAttribute()
		{
			Mock<INamedTypeSymbol> mock = new Mock<INamedTypeSymbol>();
			mock.Setup(x => x.Name).Returns("Hello");

			Mock<IAttributeWrapper> mockAttr = new Mock<IAttributeWrapper>();
			mockAttr.Setup(x => x.Name).Returns("Testing");
			mockAttr.Setup(x => x.Arguments).Returns(new Dictionary<string, string> { });

			m_contextWrapper.Setup(x => x.GetAttributes(It.IsAny<GeneratorSyntaxContext>())).Returns(
				(new List<IAttributeWrapper> { mockAttr.Object }, mock.Object));

			SettingsFromAttributeParser parser = new SettingsFromAttributeParser(new HashSet<string> {
				"Testing"
			}, m_contextWrapper.Object);
			parser.OnVisitSyntaxNode(new GeneratorSyntaxContext());

			Assert.AreEqual(1, parser.Classes.Count);
			Assert.AreEqual("Hello", parser.Classes.First().sectionName);
		}

		[TestMethod]
		public void GetSettings_TestHasSections_WithNoNameAttribute()
		{
			Mock<INamedTypeSymbol> mock = new Mock<INamedTypeSymbol>();
			mock.Setup(x => x.Name).Returns("Hello");
			mock.Setup(x => x.GetMembers()).Returns(PropertySymbols());

			m_contextWrapper.Setup(x => x.GetAttributes(It.IsAny<ISymbol>())).Returns(
				 CreateNoNameWrappers());

			SettingsFromAttributeParser parser = new SettingsFromAttributeParser(new HashSet<string> {
				"Testing"
			}, m_contextWrapper.Object);
			parser.Classes.Add(("Something", mock.Object));
			SettingsXmlModel settings = parser.GetSettings();

			SettingsXmlModel expected = new SettingsXmlModel
			{
				Sections = new List<SectionModel> {
					new SectionModel
					{
						Name = "Something",
						Parameters = new List<ParameterModel>
						{
							new ParameterModel
							{
								Name ="Hello",
								Value = "Mocking"
							}
						}
					}
				}
			};

			Assert.AreEqual(expected, settings);
		}


		[TestMethod]
		public void GetSettings_TestHasSections_WithNameAttribute()
		{
			Mock<INamedTypeSymbol> mock = new Mock<INamedTypeSymbol>();
			mock.Setup(x => x.Name).Returns("Hello");
			mock.Setup(x => x.GetMembers()).Returns(PropertySymbols());

			m_contextWrapper.Setup(x => x.GetAttributes(It.IsAny<ISymbol>())).Returns(
				 CreateNameWrappers());

			SettingsFromAttributeParser parser = new SettingsFromAttributeParser(new HashSet<string> {
				"Testing"
			}, m_contextWrapper.Object);
			parser.Classes.Add(("Something", mock.Object));
			SettingsXmlModel settings = parser.GetSettings();

			SettingsXmlModel expected = new SettingsXmlModel
			{
				Sections = new List<SectionModel> {
					new SectionModel
					{
						Name = "Something",
						Parameters = new List<ParameterModel>
						{
							new ParameterModel
							{
								Name ="Override",
								Value = "Mocking"
							}
						}
					}
				}
			};

			Assert.AreEqual(expected, settings);
		}

		[TestMethod]
		public void GetSettings_TestHasSections_WithIgnoreAttribute()
		{
			Mock<INamedTypeSymbol> mock = new Mock<INamedTypeSymbol>();
			mock.Setup(x => x.Name).Returns("Hello");
			mock.Setup(x => x.GetMembers()).Returns(PropertySymbols());

			m_contextWrapper.Setup(x => x.GetAttributes(It.IsAny<ISymbol>())).Returns(
				 CreateIgnoreWrappers());

			SettingsFromAttributeParser parser = new SettingsFromAttributeParser(new HashSet<string> {
				"Testing"
			}, m_contextWrapper.Object);
			parser.Classes.Add(("Something", mock.Object));
			SettingsXmlModel settings = parser.GetSettings();

			SettingsXmlModel expected = new SettingsXmlModel
			{
				Sections = new List<SectionModel> {
					new SectionModel
					{
						Name = "Something",
						Parameters = new List<ParameterModel>
						{
						}
					}
				}
			};

			Assert.AreEqual(expected, settings);
		}

		private static ImmutableArray<ISymbol> PropertySymbols()
		{
			Mock<IPropertySymbol> mock = new Mock<IPropertySymbol>();
			mock.Setup(x => x.Name).Returns("Hello");
			mock.Setup(x => x.Kind).Returns(SymbolKind.Property);

			return new List<ISymbol>
			{
				mock.Object
			}.ToImmutableArray();
		}

		private IList<IAttributeWrapper> CreateNoNameWrappers()
		{
			IList<IAttributeWrapper> attributeWrappers = new List<IAttributeWrapper>();
			Mock<IAttributeWrapper> mockWrapper = new Mock<IAttributeWrapper>();
			mockWrapper.Setup(x => x.Name).Returns(AttributeNames.Parameter);
			mockWrapper.Setup(x => x.Arguments).Returns(new Dictionary<string, string>
			{ { "Value", "Mocking" } });

			attributeWrappers.Add(mockWrapper.Object);
			return attributeWrappers;
		}

		private IList<IAttributeWrapper> CreateNameWrappers()
		{
			IList<IAttributeWrapper> attributeWrappers = new List<IAttributeWrapper>();
			Mock<IAttributeWrapper> mockWrapper = new Mock<IAttributeWrapper>();
			mockWrapper.Setup(x => x.Name).Returns(AttributeNames.Parameter);
			mockWrapper.Setup(x => x.Arguments).Returns(new Dictionary<string, string>
			{ { "Value", "Mocking" }, { "Name", "Override" } });

			attributeWrappers.Add(mockWrapper.Object);
			return attributeWrappers;
		}

		private IList<IAttributeWrapper> CreateIgnoreWrappers()
		{
			IList<IAttributeWrapper> attributeWrappers = new List<IAttributeWrapper>();
			Mock<IAttributeWrapper> mockWrapper = new Mock<IAttributeWrapper>();
			mockWrapper.Setup(x => x.Name).Returns(AttributeNames.Ignore);
			mockWrapper.Setup(x => x.Arguments).Returns(new Dictionary<string, string>
			{ { "Value", "Mocking" }, { "Name", "Override" } });

			attributeWrappers.Add(mockWrapper.Object);
			return attributeWrappers;
		}
	}
}
