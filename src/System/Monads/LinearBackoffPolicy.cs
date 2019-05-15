// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Monads
{
	/// <summary>
	/// Linear Backoff Policy utilities.
	/// </summary>
	public class LinearBackoffPolicy : IBackoffPolicy
	{
		/// <summary>
		/// Calculates linear backoff
		/// </summary>
		/// <param name="exponent">Parameter used to calculate backoff. Exponent, not used in LinearBackoff.</param>
		/// <param name="factor">Parameter used to calculate backoff. Multiplicative factor.</param>
		/// <returns>Backoff timespan if success else returns default TimeSpan.</returns>
		public TimeSpan CalculateBackoff(int exponent, int factor)
		{
			if (!Code.Validate(exponent >= 0, "exponent should be non negative.", TaggingUtilities.ReserveTag(0)) ||
				!Code.Validate(factor >= 0, "factor should be non negative.", TaggingUtilities.ReserveTag(0)))
			{
				return default(TimeSpan);
			}

			return TimeSpan.FromMilliseconds(factor);
		}
	}
}