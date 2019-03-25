// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.System.Context
{
	/// <summary>
	/// Interface to provide context data specific to the current call
	/// </summary>
	public interface ICallContextProvider
    {
		/// <summary>
		/// Start the call context
		/// </summary>
		/// <returns>id of the context node</returns>
		Guid? StartCallContext();


		/// <summary>
		/// End the call context
		/// </summary>
		/// <param name="threadId">id of the thread on which to end the context</param>
		/// <param name="nodeId">id of the context node</param>
		void EndCallContext(int? threadId = null, Guid? nodeId = null);


		/// <summary>
		/// Get the existing call context if there is one
		/// </summary>
		/// <param name="threadId">id of the thread from which to take the context</param>
		/// <returns>the call context that is being used</returns>
		ICallContext ExistingCallContext(int? threadId = null);
	}
}