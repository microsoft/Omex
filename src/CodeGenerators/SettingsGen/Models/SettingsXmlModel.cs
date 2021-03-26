// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models
{
	/// <summary>
	/// Application class which represents the Application xml Element in local node deployment files
	/// </summary>
	[XmlRoot("Settings", Namespace = "http://schemas.microsoft.com/2011/01/fabric", IsNullable = false)]
	public sealed record SettingsXmlModel : IEqualityComparer<SettingsXmlModel>, IEquatable<SettingsXmlModel>
	{
		/// <summary>
		/// Parameterless constructor for the Xml serialising
		/// </summary>
		public SettingsXmlModel()
		{
		}

		/// <summary>
		/// List of settings sections
		/// </summary>
		[XmlElement("Section")]
		public List<SectionModel> Sections { get; set; } = new();

		/// <inheritdoc/>
		public bool Equals(SettingsXmlModel other) => Equals(this, other);
		
		/// <inheritdoc/>
		public bool Equals(SettingsXmlModel x, SettingsXmlModel y) =>
			x.Sections.Count == y.Sections.Count && x.Sections.All(param => y.Sections.Any(paramTwo => paramTwo.Equals(param)));

		/// <inheritdoc/>
		public int GetHashCode(SettingsXmlModel obj) => obj.Sections.GetHashCode();

		/// <inheritdoc/>
		public override int GetHashCode() => GetHashCode(this);
	}

}
