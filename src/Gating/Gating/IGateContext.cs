/**************************************************************************************************
	IGateContext.cs

	Interface handling the context for gating.
**************************************************************************************************/

#region Using Directives

using System.Collections.Generic;

#endregion

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Interface for handling the context for gating.
	/// </summary>
	public interface IGateContext
	{
		/// <summary>
		/// The current request
		/// </summary>
		IGatedRequest Request { get; }


		/// <summary>
		/// Is the gate applicable for the current context
		/// </summary>
		/// <param name="gate">gate to check</param>
		/// <returns>true if applicable, false otherwise</returns>
		bool IsGateApplicable(IGate gate);


		/// <summary>
		/// Enter a scope of code that is gated if it is applicable
		/// </summary>
		/// <param name="gate">the gate to enter scope for</param>
		void EnterScope(IGate gate);


		/// <summary>
		/// Exit a scope of code that is gated
		/// </summary>
		/// <param name="gate">the gate to enter scope for</param>
		void ExitScope(IGate gate);


		/// <summary>
		/// The current active gates for the context
		/// </summary>
		/// <returns>collection of all active gates</returns>
		IEnumerable<string> CurrentGates { get; }


		/// <summary>
		/// The collection of all gates that have been used for the current request
		/// </summary>
		/// <returns>collection of all gates that have been used</returns>
		IEnumerable<IGate> ActivatedGates { get; }
	}
}
