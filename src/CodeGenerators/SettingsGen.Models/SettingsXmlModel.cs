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
	/// Application class which represents the Application xml Element in local node deployment files
	/// </summary>
	[XmlRoot("Settings", Namespace = "http://schemas.microsoft.com/2011/01/fabric", IsNullable = false)]
	public class SettingsXmlModel
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlElement("Section")]
		public List<SectionModel> Sections = new();
	}
}
