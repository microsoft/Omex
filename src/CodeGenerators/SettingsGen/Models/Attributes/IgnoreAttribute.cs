// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes
{
	/// <summary>
	/// Ignore attribute which indicates the property should not be generated to settings file or used in validation
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class IgnoreAttribute : Attribute
	{
		/// <summary>
		/// Reason for it to be ignored
		/// </summary>
		public string? Reason { get; set; }
	}
}
