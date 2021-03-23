// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes;
using Microsoft.Omex.CodeGenerators.SettingsGen.Parser;

namespace Microsoft.Omex.CodeGenerators.SettingsGen
{
	/// <summary>
	/// Finds classes 
	/// </summary>
	public sealed class ClassesWithAttributeFinder : IParser<SettingsXmlModel>
	{
		/// <summary>
		/// List of attributes to find
		/// </summary>
		public ISet<string> ClassAttributes { get; init; }

		/// <summary>
		/// Property Attributes to look for
		/// </summary>
		public ISet<string> PropertyAttributes { get; init; }

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
				AttributeNames.Section
			};
		}

		/// <inheritdoc />
		public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
		{
			// any field with at least one attribute is a candidate for property generation
			if (context.Node is ClassDeclarationSyntax classDeclaration
				&& classDeclaration.AttributeLists.Count > 0)
			{
				// Named type symvbol should not be null
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
						foreach ((string key, TypedConstant value) in namedArgs)
						{
							if (string.Equals(key, "Name", StringComparison.OrdinalIgnoreCase))
							{
								name = (value.Value as string) ?? name;
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

			foreach ((string name, INamedTypeSymbol classSymbol) in Classes)
			{
				SectionModel sectionModel = new()
				{
					Name = name
				};
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
			}

			return settings;
		}

		private bool TryCreateParameter(string propertyName, IEnumerable<AttributeData> attributes, out ParameterModel parameterModel)
		{
			parameterModel = new ParameterModel();

			// If it has the ignore attribute don't generate it
			if (attributes.Any(x => string.Equals(x.AttributeClass?.Name, AttributeNames.Ignore, StringComparison.OrdinalIgnoreCase)))
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

			foreach ((string key, TypedConstant value) in attributeData.NamedArguments)
			{
				// Ignore value if it is null
				if (value.IsNull)
				{
					continue;
				}

				// Get the Name value of the field
				if (string.Equals(key, nameof(ParameterModel.Name), StringComparison.OrdinalIgnoreCase))
				{
					parameterModel.Name = (value.Value as string) ?? name;
				}

				// Set must override property
				if (string.Equals(key, nameof(ParameterAttribute.MustOverride), StringComparison.OrdinalIgnoreCase))
				{
					parameterModel.MustOverride = (bool?)value.Value;
				}

				// Set is encrypted property
				if (string.Equals(key, nameof(ParameterAttribute.IsEncrypted), StringComparison.OrdinalIgnoreCase))
				{
					parameterModel.IsEncrypted = (bool?)value.Value;
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
