// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// A Gate combination that requires all provided gates to be applicable.
	/// </summary>
	public class GatesAll : GateCombination
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="gates">The gates to be used in the combination.</param>
		public GatesAll(params IGate[] gates)
			: base(gates)
		{
		}

		/// <summary>
		/// Checks for gate applicability
		/// </summary>
		/// <param name="context">The current gate context.</param>
		/// <param name="gates">A collection containing the applicable gates.</param>
		/// <returns>True if all the gates are applicable, false otherwise.</returns>
		public override bool IsApplicable(IGateContext context, out IGate[] gates)
		{
			gates = null;

			if (context == null)
			{
				return false;
			}

			if (Array.Exists(Gates, t => !context.IsGateApplicable(t)))
			{
				return false;
			}

			if (Gates != null && Gates.Length != 0)
			{
				gates = Gates;
				return true;
			}

			return false;
		}
	}
}
