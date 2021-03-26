// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Parser
{
	/// <summary>
	/// Parser interface which parses syntax and gets the settings from the syntax
	/// </summary>
	public interface IParser<TSettings> : ISyntaxContextReceiver where TSettings : class
	{
		/// <summary>
		/// Get settings from syntax
		/// </summary>
		/// <returns>Settings model</returns>
		TSettings GetSettings();
	}
}
