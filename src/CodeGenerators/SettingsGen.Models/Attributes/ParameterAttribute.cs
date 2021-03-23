// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ParameterAttribute : Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public string Value { get; set; } = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		public bool? MustOverride { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool? IsEncrypted { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string? Type { get; set; }
	}
}
