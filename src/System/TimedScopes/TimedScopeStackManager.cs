// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Context;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Class for managing scopes stack 
	/// </summary>
	public class TimedScopeStackManager : ITimedScopeStackManager
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="callContextManager">Call context manager</param>
		/// <param name="machineInformation">Machine information</param>
		public TimedScopeStackManager(ICallContextManager callContextManager, IMachineInformation machineInformation)
		{
			Code.ExpectsArgument(callContextManager, nameof(callContextManager), TaggingUtilities.ReserveTag(0));
			Code.ExpectsArgument(machineInformation, nameof(machineInformation), TaggingUtilities.ReserveTag(0));

			CallContextManager = callContextManager;
			MachineInformation = machineInformation;
		}


		/// <summary>
		/// Call Context Manager
		/// </summary>
		public ICallContextManager CallContextManager { get; }


		/// <summary>
		/// Timed Scope Definition
		/// </summary>
		public IMachineInformation MachineInformation { get; }


		/// <summary>
		/// Get a stack of active scopes, creating a new stack if one does not exist
		/// </summary>
		/// <returns>stack of scopes</returns>
		public TimedScopeStack GetTimedScopeStack()
		{
			ICallContext callContext = CallContextManager.CallContextHandler(MachineInformation);

			TimedScopeStack stack = null;
			if (callContext != null)
			{
				object stackObject = null;
				if (callContext.Data.TryGetValue(ActiveScopesDataKey, out stackObject))
				{
					stack = stackObject as TimedScopeStack;
				}

				if (stack == null)
				{
					stack = TimedScopeStack.Root;
					callContext.Data[ActiveScopesDataKey] = stack;
				}
			}

			return stack;
		}


		/// <summary>
		/// Set stack of active scopes
		/// </summary>
		/// <param name="timedScopeStack">Timed scope stack</param>
		public void SetTimedScopeStack(TimedScopeStack timedScopeStack)
		{
			ICallContext callContext = CallContextManager.CallContextHandler(MachineInformation);

			if (callContext != null)
			{
				callContext.Data[ActiveScopesDataKey] = timedScopeStack;
			}
		}


		/// <summary>
		/// Data key used to store the active time scopes on the call context
		/// </summary>
		private const string ActiveScopesDataKey = "TimedScope.ActiveScopes";
	}
}
