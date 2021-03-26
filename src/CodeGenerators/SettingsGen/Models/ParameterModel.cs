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
	public sealed class ParameterModel : IEqualityComparer<ParameterModel>, IEquatable<ParameterModel>
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

		/// <inheritdoc/>
		public bool Equals(ParameterModel other) => Equals(this, other);

		/// <inheritdoc/>
		public bool Equals(ParameterModel x, ParameterModel y) =>
			string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) &&
			string.Equals(x.Value, y.Value, StringComparison.OrdinalIgnoreCase) &&
			string.Equals(x.MustOverride, y.MustOverride, StringComparison.OrdinalIgnoreCase) &&
			string.Equals(x.IsEncrypted, y.IsEncrypted, StringComparison.OrdinalIgnoreCase) &&
			string.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase);

		/// <inheritdoc/>
		public int GetHashCode(ParameterModel obj)
		{
			unchecked
			{
				int hashCode = Name.GetHashCode();
				hashCode = (hashCode * 397) ^ (Value.GetHashCode());
				hashCode = (hashCode * 397) ^ (MustOverride?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (IsEncrypted?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (Type?.GetHashCode() ?? 0);
				return hashCode;
			}
		}

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj switch
		{
			ParameterModel model => Equals(model),
			_ => false
		};

		/// <inheritdoc/>
		public override int GetHashCode() => GetHashCode(this);

		/// <inheritdoc/>
		public static bool operator ==(ParameterModel x, ParameterModel y) => x.Equals(y);

		/// <inheritdoc/>
		public static bool operator !=(ParameterModel x, ParameterModel y) => !x.Equals(y);
	}
}
