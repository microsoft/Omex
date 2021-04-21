using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes;

#nullable enable

namespace Microsoft.Omex.SettingsGen.Example
{
	/// <summary>
	/// Section class
	/// </summary>
	[Section(Name = "NewSection")]
	public class Testing
	{
		/// <summary>
		/// Parameter
		/// </summary>
		public string Whatever { get; set; } = string.Empty;

		/// <summary>
		/// Parameter
		/// </summary>
		[Parameter(Name = "TestingThis", Value = "Hmmmm")]
		public string DiffName { get; set; } = string.Empty;

		/// <summary>
		/// Ignore this
		/// </summary>
		[Ignore]
		public int Hello { get; set; }

		/// <summary>
		/// Parameter
		/// </summary>
		[Parameter(IsEncrypted =true, MustOverride = true)]
		public string? Overriding { get; set; }
	}
}
