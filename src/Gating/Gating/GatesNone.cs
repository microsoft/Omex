/***************************************************************************************************
	GatesNone.cs

	A negative combination where the construct returns applicable only
	when all the gates are non-applicable.
***************************************************************************************************/

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Combination of gates where no gates being applicable is used
	/// </summary>
	public class GatesNone : GatesAny
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="gates">set of gates</param>
		public GatesNone(params IGate[] gates)
			: base(gates)
		{

		}


		/// <summary>
		/// Is the gate combination applicable
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="gate">applicable gate, always null</param>
		/// <returns>true if no gates are applicable, false otherwise</returns>
		public override bool IsApplicable(IGateContext context, out IGate[] gate)
		{
			bool isApplicable = !base.IsApplicable(context, out gate);
			gate = null;
			return isApplicable;
		}
	}
}
