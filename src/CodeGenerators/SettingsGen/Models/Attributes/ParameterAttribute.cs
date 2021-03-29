// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes
{
	/// <summary>
	/// Attribute representing a parameter in the Settings.xml file
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class ParameterAttribute : Attribute
	{
		/// <summary>
		/// Name of the parameter
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Value of the parameter
		/// </summary>
		public string Value { get; set; } = string.Empty;

		/// <summary>
		/// MustOverride field of the Parameter element
		/// </summary>
		public bool MustOverride { get; set; } = false;

		/// <summary>
		/// IsEncrypted field of the Parameter element
		/// </summary>
		public bool IsEncrypted { get; set; } = false;

		/// <summary>
		/// Type field of the Parameter element
		/// </summary>
		public TypeKind Type { get; set; }
	}
}
