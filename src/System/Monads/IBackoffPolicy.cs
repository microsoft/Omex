/***************************************************************************************************
	IBackoffPolicy.cs

	Backoff Policy utilities.
***************************************************************************************************/

#region Using directives

using System;

#endregion

namespace Microsoft.Omex.System.Monads
{
	/// <summary>
	/// Backoff Policy utilities.
	/// </summary>
	public interface IBackoffPolicy
	{
		/// <summary>
		/// Calculates backoff
		/// </summary>
		/// <param name="exponent">Parameter used to calculate backoff. The higher the value, the higher the backoff. This is usually the attempt number.</param>
		/// <param name="factor">Parameter used to calculate backoff.</param>
		/// <returns>Backoff timespan if success else returns default TimeSpan.</returns>
		TimeSpan CalculateBackoff(int exponent, int factor);
	}
}
