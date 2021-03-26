// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Wrappers
{
	/// <summary>
	/// Wrapper for attribute syntax
	/// </summary>
	public interface IAttributeWrapper
	{
		/// <summary>
		/// Name of the attribute class
		/// </summary>
		string Name { get;  }

		/// <summary>
		/// Arguments of the attribute, mapping name of attribute to value as a string
		/// </summary>
		IDictionary<string, string> Arguments { get; }
	}
}
