// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes;

namespace SettingsGen.Example
{
	/// <summary>
	/// 
	/// </summary>
	[Section(Name = "hello")]
	public class SectionTwo
	{
		/// <summary>
		/// 
		/// </summary>
		public int TestingSectionTwo { get; } = 0;
	}
}
