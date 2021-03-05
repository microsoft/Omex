// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Azure.Documents;

namespace Microsoft.Omex.DocumentDb
{
	/// <summary>
	/// Script options class.
	/// </summary>
	public class ScriptOptions
	{
		/// <summary>
		/// Triggers to get or create.
		/// </summary>
		public List<Trigger> Triggers { get; set; }

		/// <summary>
		/// Stored procedures to get or create.
		/// </summary>
		public List<StoredProcedure> StoredProcedures { get; set; }

		/// <summary>
		/// Indicator to delete existing scripts before creating new ones.
		/// </summary>
		public bool ResetScripts { get; set; }
	}
}
