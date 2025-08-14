// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Authentication
{
	/// <summary>
	/// Provides the customer ID for the current request.
	/// </summary>
	public interface ICustomerIdProvider
	{
		/// <summary>
		/// Gets the customer ID for the current request.
		/// </summary>
		/// <returns>The customer ID, or <see cref="string.Empty"/> if there is no ID.</returns>
		string GetCustomerId();
	}
}
