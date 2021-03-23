// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models
{
	/// <summary>
	/// 
	/// </summary>
	public class ParameterModel
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute]
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute]
		public string Value { get; set; } = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute]
		public bool? MustOverride { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute()]
		public bool? IsEncrypted { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute]
		public string? Type { get; set;} 

	}
}
