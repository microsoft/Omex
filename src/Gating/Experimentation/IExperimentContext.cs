/**************************************************************************************************
	IExperimentContext.cs

	Interface handling the context for experimentation

**************************************************************************************************/

namespace Microsoft.Omex.Gating.Experimentation
{
	/// <summary>
	/// Interface for handling the context for experimentation
	/// </summary>
	public interface IExperimentContext
	{
		/// <summary>
		/// Is the experimenatal gate applicable for the current context
		/// </summary>
		/// <param name="gate">gate to check</param>
		/// <returns>true if applicable, false otherwise</returns>
		bool IsExperimentalGateApplicable(IGate gate);


		/// <summary>
		/// Is the user on the experiment
		/// </summary>
		/// <remarks>It is used to check that we only activate an experimental gate</remarks>
		/// <param name="experimentName">name of the experiment</param>
		/// <returns>true if user is on experiment, false otherwise</returns>
		bool IsUserOnExperiment(string experimentName);
	}
}
