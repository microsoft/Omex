// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Parser
{
	/// <summary>
	/// Finds classes 
	/// </summary>
	public sealed class ClassesWithAttributeFinder : IParser<SettingsXmlModel>
	{
		/// <summary>
		/// List of attributes to find
		/// </summary>
		public ISet<string> ClassAttributes { get; private set; }

		/// <summary>
		/// Property Attributes to look for
		/// </summary>
		public ISet<string> PropertyAttributes { get; private set; }

		/// <summary>
		/// List of section classes and the corresponding with their section name
		/// </summary>
		public IList<(string sectionName, INamedTypeSymbol className)> Classes { get; } = new List<(string, INamedTypeSymbol)>();

		/// <summary>
		/// Constructor
		/// </summary>
		public ClassesWithAttributeFinder(ISet<string> attributesToFind)
		{

			if (attributesToFind.Count == 0)
			{
				throw new ArgumentException("Attributes to find list has no elements");
			}
			ClassAttributes = attributesToFind;
			PropertyAttributes = new HashSet<string>()
			{
				AttributeNames.Ignore,
				AttributeNames.Parameter,
				AttributeNames.Required,
			};
		}

		/// <inheritdoc />
		public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
		{
			// any field with at least one attribute is a candidate for property generation
			if (context.Node is ClassDeclarationSyntax classDeclaration
				&& classDeclaration.AttributeLists.Count > 0)
			{
				// Named type symbol should not be null
				INamedTypeSymbol classSymbol = (context.SemanticModel.GetDeclaredSymbol(classDeclaration))!;
				ImmutableArray<AttributeData> attrs = classSymbol.GetAttributes();

				// Only get attributes we are looking for that can be on classes
				foreach (AttributeData attribute in attrs)
				{
					if (ClassAttributes.Any(x => string.Equals(attribute.AttributeClass?.Name, x, StringComparison.OrdinalIgnoreCase)))
					{
						// Get the name value of the attribute
						string name = classSymbol.Name;
						ImmutableArray<KeyValuePair<string, TypedConstant>> namedArgs = attribute.NamedArguments;

						foreach (KeyValuePair<string, TypedConstant> keyValue in namedArgs)
						{
							if (string.Equals(keyValue.Key, "Name", StringComparison.OrdinalIgnoreCase))
							{
								name = (keyValue.Value.Value as string) ?? name;
								break;
							}
						}
						Classes.Add((name, classSymbol));
					}
				}
			}
		}

		/// <inheritdoc/>
		public SettingsXmlModel GetSettings()
		{
			SettingsXmlModel settings = new();

			foreach ((string name, INamedTypeSymbol classSymbol) in Classes.OrderBy(x => x.sectionName))
			{
				SectionModel sectionModel = new()
				{
					Name = name
				};

				// Get properties for a given class
				IEnumerable<IPropertySymbol> properties = classSymbol.GetMembers()
					.Where(m => m.Kind == SymbolKind.Property).Cast<IPropertySymbol>();

				foreach (IPropertySymbol property in properties)
				{
					// Only get attributes we care about
					IEnumerable<AttributeData> attrs = property.GetAttributes()
						.Where(x => x.AttributeClass != null && PropertyAttributes.Contains(x.AttributeClass.Name));
					if (TryCreateParameter(property.Name, attrs, out ParameterModel parameterModel))
					{
						sectionModel.Parameters.Add(parameterModel);
					}
				}

				settings.Sections.Add(sectionModel);
			}

			return settings;
		}

		private bool TryCreateParameter(string propertyName, IEnumerable<AttributeData> attributes, out ParameterModel parameterModel)
		{
			parameterModel = new ParameterModel();

			// If it has the ignore attribute don't generate it
			if (attributes.Any(x => string.Equals(x.AttributeClass?.Name, AttributeNames.Ignore, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(x.AttributeClass?.Name, AttributeNames.Ignore, StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}

			// Handle parameter attribute present
			List<AttributeData> parameters = attributes.Where(
				x => string.Equals(x.AttributeClass?.Name,  AttributeNames.Parameter, StringComparison.OrdinalIgnoreCase)).ToList();
			if (parameters.Count > 0)
			{
				AttributeData parameterAttr = parameters.First();
				LoadParameterFromAttribute(parameterModel, parameterAttr, propertyName);
				return true;
			}

			// Handle no parameter tag present just assume property name is the setting name
			// and then return successful creation
			parameterModel.Name = propertyName;

			return true;
		}

		private void LoadParameterFromAttribute(ParameterModel parameterModel, AttributeData attributeData, string name)
		{
			parameterModel.Name = name;

			foreach (KeyValuePair<string, TypedConstant> keyValue in attributeData.NamedArguments)
			{
				// Ignore value if it is null
				if (keyValue.Value.IsNull)
				{
					continue;
				}
				string key = keyValue.Key;
				TypedConstant value = keyValue.Value;

				// Get the Name value of the field
				if (string.Equals(keyValue.Key, nameof(ParameterModel.Name), StringComparison.OrdinalIgnoreCase))
				{
					parameterModel.Name = (keyValue.Value.Value as string) ?? name;
				}

				// Set must override property
				if (string.Equals(keyValue.Key, nameof(ParameterAttribute.MustOverride), StringComparison.OrdinalIgnoreCase))
				{
					parameterModel.MustOverride = value.Value as string ?? string.Empty;
				}

				// Set is encrypted property
				if (string.Equals(key, nameof(ParameterAttribute.IsEncrypted), StringComparison.OrdinalIgnoreCase))
				{
					parameterModel.IsEncrypted = value.Value as string ?? string.Empty;
				}

				// Set value property
				if (string.Equals(key, nameof(ParameterAttribute.Value), StringComparison.OrdinalIgnoreCase))
				{
					parameterModel.Value = value.Value as string ?? string.Empty;
				}

				// set type property
				if (string.Equals(key, nameof(ParameterAttribute.Type), StringComparison.OrdinalIgnoreCase))
				{
					parameterModel.Type = value.Value as string;
				}
			}
		}
	}

}
