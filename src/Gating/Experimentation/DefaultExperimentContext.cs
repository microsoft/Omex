/**************************************************************************************************
	DefaultExperimentContext.cs

	Default context for non storefrontsite requests

**************************************************************************************************/

namespace Microsoft.Omex.Gating.Experimentation
{
	/// <summary>
	/// Default context for non storefrontsite requests
	/// </summary>
	public class DefaultExperimentContext : IExperimentContext
	{
		/// <summary>
		/// Is the experimenatal gate applicable for the current context
		/// </summary>
		/// <param name="gate">gate to check</param>
		/// <returns>true if applicable, false otherwise</returns>
		public bool IsExperimentalGateApplicable(IGate gate) => false;


		/// <summary>
		/// Is the user on the experiment
		/// </summary>
		/// <remarks>It is used to check that we only activate an experimental gate</remarks>
		/// <param name="experimentName">name of the experiment</param>
		/// <returns>true if user is on experiment, false otherwise</returns>
		public bool IsUserOnExperiment(string experimentName) => false;
	}
}
