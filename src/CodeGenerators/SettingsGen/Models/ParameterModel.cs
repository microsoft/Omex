// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models
{
	/// <summary>
	/// Parameter element used for service fabric settings
	/// </summary>
	public sealed record ParameterModel
	{
		/// <summary>
		/// Parameterless constructor for the Xml serialising
		/// </summary>
		public ParameterModel()
		{
		}

		/// <summary>
		/// Name attribute of the Parameter element
		/// </summary>
		[XmlAttribute]
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Value attribute of the Parameter element
		/// </summary>
		[XmlAttribute]
		public string Value { get; set; } = string.Empty;

		/// <summary>
		/// MustOverride attribute of the Parameter element
		/// </summary>
		[XmlAttribute]
		public string? MustOverride { get; set; }

		/// <summary>
		/// IsEncrypted attribute of the Parmater element
		/// </summary>
		[XmlAttribute]
		public string? IsEncrypted { get; set; }

		/// <summary>
		/// Type attribute of the Parameter element
		/// </summary>
		[XmlAttribute]
		public string? Type { get; set;}
	}
}
