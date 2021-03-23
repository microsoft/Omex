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
	public static class AttributeNames
	{
		/// <summary>
		/// The ignore attribute
		/// </summary>
		public static readonly string Ignore = "Ignore";

		/// <summary>
		/// Parameter attribute
		/// </summary>
		public static readonly string Parameter = "ParameterAttribute";

		/// <summary>
		/// Section attribute
		/// </summary>
		public static readonly string Section = "Section";

		/// <summary>
		/// Required attribute, this is a DataAnnotation in IOptions
		/// </summary>
		public static readonly string Required = "Required";
	}
}
