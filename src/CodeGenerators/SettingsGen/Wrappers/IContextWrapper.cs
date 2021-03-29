// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Wrappers
{
	/// <summary>
	/// Context wrapper to remove references to struct to make testing easier.
	/// </summary>
	internal interface IContextWrapper
	{
		/// <summary>
		/// Get attributes and the class INamedTypeSymbol they are in.
		/// </summary>
		/// <param name="context">Generator syntax context</param>
		/// <returns>List of attributes and the class they are on</returns>
		(IList<IAttributeWrapper>, INamedTypeSymbol?) GetAttributes(GeneratorSyntaxContext context);

		/// <summary>
		/// Get attributes from the symbol they are in.
		/// </summary>
		/// <param name="symbol">Syntax symbol</param>
		/// <returns>List of attributes</returns>
		IList<IAttributeWrapper> GetAttributes(ISymbol symbol);

	}
}
