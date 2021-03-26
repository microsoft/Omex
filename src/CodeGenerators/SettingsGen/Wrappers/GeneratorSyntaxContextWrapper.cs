// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Wrappers
{
	internal sealed class GeneratorSyntaxContextWrapper : IContextWrapper
	{
		/// <inheritdoc/>
		public (IList<IAttributeWrapper>, INamedTypeSymbol?) GetAttributes(GeneratorSyntaxContext context)
		{
			IList<IAttributeWrapper> wrappers = new List<IAttributeWrapper>();
			INamedTypeSymbol? classSymbol = null;

			// any field with at least one attribute is a candidate for property generation
			if (context.Node is ClassDeclarationSyntax classDeclaration
				&& classDeclaration.AttributeLists.Count > 0)
			{
				// Named type symbol should not be null
				classSymbol = (context.SemanticModel.GetDeclaredSymbol(classDeclaration))!;
				wrappers = GetAttributes(classSymbol);
			}
			return (wrappers, classSymbol);
		}

		public IList<IAttributeWrapper> GetAttributes(ISymbol symbol)
		{
			ImmutableArray<AttributeData> attrs = symbol.GetAttributes();
			IList<IAttributeWrapper> attributeWrappers = new List<IAttributeWrapper>();

			// Format the attributes
			foreach (AttributeData attribute in attrs)
			{
				string? attrClassName = attribute.AttributeClass?.Name;

				if (attrClassName is null)
				{
					continue;
				}

				// Parse the attribute arguments into a dictionary
				AttributeWrapper attributeWrapper = new(attrClassName);
				ImmutableArray<KeyValuePair<string, TypedConstant>> namedArgs = attribute.NamedArguments;
				foreach (KeyValuePair<string, TypedConstant> arg in namedArgs)
				{
					if (arg.Value.IsNull)
					{
						continue;
					}

					string valueString = arg.Value.Value?.ToString() ?? string.Empty;

					if (string.Equals(arg.Key, nameof(ParameterAttribute.MustOverride), StringComparison.OrdinalIgnoreCase) ||
						string.Equals(arg.Key, nameof(ParameterAttribute.IsEncrypted), StringComparison.OrdinalIgnoreCase))
					{
						valueString = valueString.ToLowerInvariant();
					}

					attributeWrapper.Arguments.Add(arg.Key, valueString);
				}
				attributeWrappers.Add(attributeWrapper);
			}

			return attributeWrappers;
		}
	}
}
