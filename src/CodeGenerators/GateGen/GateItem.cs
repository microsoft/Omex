// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.Omex.CodeGenerators.GateGen
{
	/// <summary>
	/// A gate item
	/// </summary>
	public class GateItem
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public GateItem()
		{
			Gates = new SortedDictionary<string, GateItem>(StringComparer.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Gate name
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// Name of the gate group (or section), used to group gates
		/// </summary>
		public string GateGroupName { get; set; }


		/// <summary>
		/// Set of gates that should be listed below the gate group
		/// </summary>
		public IDictionary<string, GateItem> Gates { get; private set; }
	}
}
