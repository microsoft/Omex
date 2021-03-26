// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes;

namespace Microsoft.Omex.SettingsGen.Example
{
	/// <summary>
	/// Second section
	/// </summary>
	[Section]
	public class SectionTwo
	{
		/// <summary>
		/// Parameter in section 2
		/// </summary>
		public int TestingSectionTwo { get; } = 0;

		/// <summary>
		/// 
		/// </summary>
		public int TestingSectionThree { get; } = 0;

		/// <summary>
		/// 
		/// </summary>
		[Parameter(MustOverride = "true")]
		public string SomethingElse { get; } = string.Empty;
	}
}
