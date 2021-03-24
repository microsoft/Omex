// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes
{
	/// <summary>
	/// Attribute representing Section element in Settings.xml
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class SectionAttribute : Attribute
	{
		/// <summary>
		/// Name of the section
		/// </summary>
		public string? Name { get; set; }
	}
}
