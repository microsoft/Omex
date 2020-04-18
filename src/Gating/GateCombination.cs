// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// GateCombination, abstract base to allow for different combination of gates
	/// </summary>
	public abstract class GateCombination
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="gates">set of gates</param>
		protected GateCombination(params IGate[] gates)
		{
			Gates = Array.FindAll(gates, gate => gate != null);
		}

		/// <summary>
		/// Collection of gates used in the gate combination
		/// </summary>
		public IGate[] Gates { get; }

		/// <summary>
		/// Is the gate combination applicable
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="gates">applicable gates, can be null even if gate combination is applicable</param>
		/// <returns>true if the gate combination is applicable</returns>
		public abstract bool IsApplicable(IGateContext context, out IGate[] gates);
	}
}
