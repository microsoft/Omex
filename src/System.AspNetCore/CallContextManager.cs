// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using Microsoft.Omex.System.Context;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.TimedScopes;

namespace Microsoft.Omex.System.AspNetCore
{
	/// <summary>
	/// Call context manager class
	/// </summary>
	public class CallContextManager : ICallContextManager
	{
		/// <summary>
		/// An overridable call context for unit test purposes
		/// </summary>
		public ICallContext CallContextOverride { get; set; }


		/// <summary>
		/// The call context
		/// Depending on the 'UseLogicalCallContext' app setting it will either use classic - OperationCallContext -> ThreadCallContext chain
		/// or the new LogicalCallContext as backup
		/// </summary>
		private Lazy<ICallContext> s_callContext(IMachineInformation machineInformation)
		{
			return new Lazy<ICallContext>(() => new HttpCallContext(useLogicalCallContext: false, machineInformation: machineInformation), LazyThreadSafetyMode.PublicationOnly);
		}


		/// <summary>
		/// Current call context
		/// </summary>
		public ICallContext CallContextHandler(IMachineInformation machineInformation)
		{
			return CallContextOverride ?? new HttpCallContext(useLogicalCallContext: false, machineInformation: machineInformation);
		}
	}
}
