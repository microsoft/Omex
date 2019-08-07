// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Context;
using Microsoft.Omex.System.Diagnostics;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Interface for handling call context
	/// </summary>
	public interface ICallContextManager
	{
		/// <summary>
		/// Current call context
		/// </summary>
		ICallContext CallContextHandler(IMachineInformation machineInformation);


		/// <summary>
		/// An overridable call context for unit test purposes
		/// </summary>
		ICallContext CallContextOverride { get; set; }
	}
}
