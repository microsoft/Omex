// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Correlation data provider
	/// </summary>
	public interface ICorrelationDataProvider
	{
		/// <summary>
		/// Provide current correlation information
		/// </summary>
		CorrelationData CurrentCorrelation { get; }
	}
}
