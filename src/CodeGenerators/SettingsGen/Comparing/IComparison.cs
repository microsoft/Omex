// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Comparing
{
	/// <summary>
	/// Comparison interface used to determine if new settings and exsiting settings in file are the same
	/// </summary>
	public interface IComparison<TSetting> where TSetting: class
	{
		/// <summary>
		/// Determine whether new settings and existing settings are the same.
		/// </summary>
		/// <param name="newSettings">New settings gotten from build</param>
		/// <param name="filename">Filename containing existing settings</param>
		/// <returns>Whether or not the settings match</returns>
		bool AreExistingSettingsEqual(TSetting newSettings, AdditionalText filename);
	}
}
