/***************************************************************************************************
	GatesAny.cs

	Combination of gates where any gate being applicable is used
***************************************************************************************************/

using System;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Combination of gates where any gate being applicable can be used
	/// </summary>
	public class GatesAny : GateCombination
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="gates">set of gates</param>
		public GatesAny(params IGate[] gates)
			: base(gates)
		{
		}


		/// <summary>
		/// Is the gate combination applicable
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="gates">applicable gate, always set if the gate combination is applicable</param>
		/// <returns>true if at least one gate is applicable</returns>
		public override bool IsApplicable(IGateContext context, out IGate[] gates)
		{
			gates = null;
			if (context == null)
			{
				return false;
			}

			IGate gate = Array.Find(Gates, t => context.IsGateApplicable(t));
			if (gate != null)
			{
				gates = new[] { gate };
				return true;
			}

			return false;
		}
	}
}
