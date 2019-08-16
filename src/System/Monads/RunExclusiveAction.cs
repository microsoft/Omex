// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Monads
{
	/// <summary>
	/// A class that runs an action exclusively, ensuring no other instances of the action are
	/// running simultaneously.
	/// </summary>
	public class RunExclusiveAction
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RunExclusiveAction"/> class.
		/// </summary>
		/// <param name="runOnlyOnce">If set to <c>true</c>, the action will be run only once.</param>
		public RunExclusiveAction(bool runOnlyOnce = false) => RunOnlyOnce = runOnlyOnce;


		/// <summary>
		/// Initializes a new instance of the <see cref="RunExclusiveAction"/> class.
		/// </summary>
		/// <param name="action">The action to run.</param>
		/// <param name="runOnlyOnce">If set to <c>true</c>, the action will be run only once.</param>
		/// <exception cref="ArgumentNullException">Thrown if the action is null. Note that this
		/// explicitly does not use Code.Expects to avoid any re-entrance issues if the action is
		/// used as part of setting up CrossCuttingConcerns and ULSLogging.</exception>
		public RunExclusiveAction(Action action, bool runOnlyOnce = false)
		{
			m_action = action ?? throw new ArgumentNullException(nameof(action));
			RunOnlyOnce = runOnlyOnce;
		}


		/// <summary>
		/// Gets a value indicating whether the action is running.
		/// </summary>
		public bool IsRunning => m_status == (int)Status.Running;


		/// <summary>
		/// Gets a value indicating whether this instance has run.
		/// </summary>
		public bool HasRun => m_status == (int)Status.RunAndMayRunAgain || m_status == (int)Status.RunAndMayNotRunAgain;


		/// <summary>
		/// Gets or sets the m_action to run.
		/// </summary>
		protected readonly Action m_action;


		/// <summary>
		/// Gets or sets a value indicating whether the action should be run only once.
		/// </summary>
		protected bool RunOnlyOnce { get; }


		/// <summary>
		/// A flag indicating if the action has ran, is currently running or can be run again.
		/// </summary>
		private int m_status;


		/// <summary>
		/// Do the action.
		/// </summary>
		public virtual void Do() => Do(m_action);


		/// <summary>
		/// Do the m_action.
		/// </summary>
		/// <param name="action">The m_action to run.</param>
		public virtual void Do(Action action)
		{
			if (!Code.ValidateArgument(action, nameof(action), TaggingUtilities.ReserveTag(0x2382084f /* tag_9667p */)))
			{
				return;
			}

			if (InterlockedCompareExchange(Status.Running, Status.NotRun) == Status.NotRun ||
				InterlockedCompareExchange(Status.Running, Status.RunAndMayRunAgain) == Status.RunAndMayRunAgain)
			{
				try
				{
					action();
				}
				finally
				{
					InterlockedExchange(RunOnlyOnce ? Status.RunAndMayNotRunAgain : Status.RunAndMayRunAgain);
				}
			}
		}


		/// <summary>
		/// Performs an interlocked compare exchange on the status value.
		/// </summary>
		/// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
		/// <param name="comparand">The value that is compared to the status.</param>
		/// <returns>The original status value.</returns>
		protected Status InterlockedCompareExchange(Status value, Status comparand) => (Status)Interlocked.CompareExchange(ref m_status, (int)value, (int)comparand);


		/// <summary>
		/// Performs an interlocked exchange on the status value.
		/// </summary>
		/// <param name="value">The value to which the status is set.</param>
		protected void InterlockedExchange(Status value) => Interlocked.Exchange(ref m_status, (int)value);


		/// <summary>
		/// The status values.
		/// </summary>
		protected enum Status
		{
			/// <summary>
			/// A value indicating the m_action has not run.
			/// </summary>
			NotRun,


			/// <summary>
			/// A value indicating the m_action is currently running.
			/// </summary>
			Running,


			/// <summary>
			/// A value indicating the m_action has run and may run again.
			/// </summary>
			RunAndMayRunAgain,


			/// <summary>
			/// A value indicating the m_action has run and may not run again.
			/// </summary>
			RunAndMayNotRunAgain
		}
	}
}