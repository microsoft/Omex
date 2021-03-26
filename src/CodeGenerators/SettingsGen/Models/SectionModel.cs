// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models
{
	/// <summary>
	/// Xml Model representing the section in Settings.xml
	/// </summary>
	public sealed record SectionModel : IEqualityComparer<SectionModel>, IEquatable<SectionModel>
	{
		/// <summary>
		/// Parameterless constructor for the Xml serialising
		/// </summary>
		public SectionModel()
		{
		}

		/// <summary>
		/// Name of the section
		/// </summary>
		[XmlAttribute]
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// List of parameters
		/// </summary>
		[XmlElement("Parameter")]
		public List<ParameterModel> Parameters = new();

		/// <inheritdoc/>
		public bool Equals(SectionModel other) => Equals(this, other);

		/// <inheritdoc/>
		public bool Equals(SectionModel x, SectionModel y) =>
			string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) &&
			x.Parameters.Count == y.Parameters.Count &&
			x.Parameters.All(param => y.Parameters.Contains(param));

		/// <inheritdoc/>
		public int GetHashCode(SectionModel obj)
		{
			unchecked
			{
				int hashCode = obj.Name.GetHashCode();
				hashCode = (hashCode * 397) ^ obj.Parameters.GetHashCode();
				return hashCode;
			}
		}

		/// <inheritdoc/>
		public override int GetHashCode() => GetHashCode(this);
	}
}
