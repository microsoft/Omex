// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Omex.Extensions.Configuration.UnitTests.Support
{
	public class ConfigurationExample
	{
		[Required(AllowEmptyStrings = false)]
		public string ValidatedConfiguration { get; set; } = "";

		public string? UnvalidatedConfiguration { get; set; }

		[Range(1, 10)]
		public int ValidatedInteger { get; set; } = 0;
	}
}
