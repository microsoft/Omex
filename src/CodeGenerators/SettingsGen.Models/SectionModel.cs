// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models
{
	/// <summary>
	/// 
	/// </summary>
	public class SectionModel
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute]
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		[XmlElement("Parameter")]
		public List<ParameterModel> Parameters = new();
	}
}
