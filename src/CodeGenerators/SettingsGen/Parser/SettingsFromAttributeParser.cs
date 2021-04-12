// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes;
using Microsoft.Omex.CodeGenerators.SettingsGen.Wrappers;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Parser
{
	/// <summary>
	/// Finds classes 
	/// </summary>
	public sealed class SettingsFromAttributeParser : IParser<SettingsXmlModel>
	{
		private readonly IContextWrapper m_contextWrapper;

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
		internal SettingsFromAttributeParser(ISet<string> attributesToFind, IContextWrapper contextWrapper)
		{
			if (attributesToFind.Count == 0)
			{
				throw new ArgumentException("Attributes to find list has no elements");
			}
			m_contextWrapper = contextWrapper;
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
			(IList<IAttributeWrapper> attributes, INamedTypeSymbol? classSymbol) = m_contextWrapper.GetAttributes(context);

			if (attributes.Count > 0 && classSymbol != null)
			{
				foreach (IAttributeWrapper attribute in attributes)
				{
					if (ClassAttributes.Contains(attribute.Name))
					{
						// Get the name value of the class
						string name = classSymbol.Name;

						if (attribute.Arguments.TryGetValue(nameof(ParameterModel.Name), out string value))
						{
							name = value;
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
					IList<IAttributeWrapper>? attrs = m_contextWrapper.GetAttributes(property);
					if (TryCreateParameter(property.Name, attrs, out ParameterModel parameterModel))
					{
						sectionModel.Parameters.Add(parameterModel);
					}
				}

				settings.Sections.Add(sectionModel);
			}

			return settings;
		}

		private bool TryCreateParameter(string propertyName, IList<IAttributeWrapper> attributes, out ParameterModel parameterModel)
		{
			parameterModel = new ParameterModel();

			// If it has the ignore attribute don't generate it
			if (attributes.Any(x => string.Equals(x.Name, AttributeNames.Ignore, StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}

			// Handle parameter attribute present
			IAttributeWrapper? parameterAttr = attributes.FirstOrDefault(
				x => string.Equals(x.Name, AttributeNames.Parameter, StringComparison.OrdinalIgnoreCase));
			if (parameterAttr is not null)
			{
				LoadParameterFromAttribute(parameterModel, parameterAttr, propertyName);
				return true;
			}

			// Handle no parameter tag present: just assume property name is the setting name
			// and then return successful creation
			parameterModel.Name = propertyName;

			return true;
		}

		private void LoadParameterFromAttribute(ParameterModel parameterModel, IAttributeWrapper attributeData, string name)
		{
			parameterModel.Name = name;

			// Get the Name value of the field
			if (attributeData.Arguments.TryGetValue(nameof(ParameterModel.Name), out string value))
			{
				parameterModel.Name = value;
			}

			// Set must override property
			if (attributeData.Arguments.TryGetValue(nameof(ParameterModel.MustOverride), out value))
			{
				parameterModel.MustOverride = value;
			}

			// Set is encrypted property
			if (attributeData.Arguments.TryGetValue(nameof(ParameterModel.IsEncrypted), out value))
			{
				parameterModel.IsEncrypted = value;
			}

			// Set value property
			if (attributeData.Arguments.TryGetValue(nameof(ParameterModel.Value), out value))
			{
				parameterModel.Value = value;
			}

			// set type property
			if (attributeData.Arguments.TryGetValue(nameof(ParameterModel.Type), out value))
			{
				parameterModel.Type = value;
			}
		}
	}
}
