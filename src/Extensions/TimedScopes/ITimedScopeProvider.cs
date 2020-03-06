// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>
	/// Interface to create TimedScope
	/// </summary>
	public interface ITimedScopeProvider
	{
		/// <summary>
		/// Creates and start TimedScope
		/// </summary>
		/// <exception cref="System.ArgumentException">Thrown when name null or empty</exception>
		TimedScope Start(string name, TimedScopeResult result);


		/// <summary>
		/// Creates TimedScope
		/// </summary>
		/// <exception cref="System.ArgumentException">Thrown when name null or empty</exception>
		TimedScope Create(string name, TimedScopeResult result);
	}
}
