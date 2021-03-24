// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes
{
	/// <summary>
	/// Name of the attributes we look for
	/// </summary>
	public static class AttributeNames
	{
		/// <summary>
		/// The ignore attribute
		/// </summary>
		public static readonly string Ignore = nameof(IgnoreAttribute);

		/// <summary>
		/// Parameter attribute
		/// </summary>
		public static readonly string Parameter = nameof(ParameterAttribute);

		/// <summary>
		/// Section attribute
		/// </summary>
		public static readonly string Section = nameof(SectionAttribute);

		/// <summary>
		/// Required attribute, this is a DataAnnotation in IOptions
		/// </summary>
		public static readonly string Required = "Required";
	}
}
