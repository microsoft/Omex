// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Omex.System.Logging;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Implements a class that captures Performance Diagnostics.
	/// </summary>
	public class PerfDiagnostics
	{
		/// <summary>
		/// Status of the last timer run
		/// </summary>
		public bool LastStatus { get; private set; }


		/// <summary>
		/// Number of cycles used (updated after Stop is called)
		/// </summary>
		public long CyclesUsed { get; private set; }


		/// <summary>
		/// Approximate number of milliseconds spent in User Mode
		/// </summary>
		public decimal UserModeMilliseconds { get; private set; }


		/// <summary>
		/// Approximate number of milliseconds spent in Kernel Mode
		/// </summary>
		public decimal KernelModeMilliseconds { get; private set; }


		/// <summary>
		/// Backing field for number of http requests
		/// </summary>
		private int m_httpRequests;


		/// <summary>
		/// Number of HttpRequests
		/// </summary>
		public int HttpRequestCount
		{
			get
			{
				return m_httpRequests;
			}
		}


		/// <summary>
		/// Backing field for number of service calls
		/// </summary>
		private int m_serviceCalls;


		/// <summary>
		/// Number of service calls
		/// </summary>
		public int ServiceCallCount
		{
			get
			{
				return m_serviceCalls;
			}
		}


		/// <summary>
		/// Psuedo handle to the current thread
		/// </summary>
		private static readonly IntPtr s_currentThreadHandle = NativeMethods.GetCurrentThread();


		/// <summary>
		/// Parent diagnostics if available
		/// </summary>
		private readonly PerfDiagnostics m_parent;


		/// <summary>
		/// Id of the thread being timed
		/// </summary>
		private uint m_threadId;


		/// <summary>
		/// Kernel mode ticks at start of timing
		/// </summary>
		private long m_startKernel;


		/// <summary>
		/// User mode ticks at start of timing
		/// </summary>
		private long m_startUser;


		/// <summary>
		/// Cpu cycles at start of timing
		/// </summary>
		private ulong m_startCycles;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent">Parent diagnostics, or null if not available</param>
		public PerfDiagnostics(PerfDiagnostics parent = null)
		{
			m_parent = parent;
		}


		/// <summary>
		/// Reset the state of the Timer
		/// </summary>
		public void Reset()
		{
			CyclesUsed = 0;
			UserModeMilliseconds = 0;
			KernelModeMilliseconds = 0;
			LastStatus = false;
			m_threadId = uint.MaxValue;
			m_httpRequests = 0;
			m_serviceCalls = 0;
		}


		/// <summary>
		/// Start the timer
		/// </summary>
		/// <returns>Success status</returns>
		public bool Start()
		{
			m_threadId = uint.MaxValue;

			long creationTime, exitTime;

			if (!NativeMethods.GetThreadTimes(s_currentThreadHandle, out creationTime, out exitTime, out m_startKernel, out m_startUser))
			{
				ULSLogging.LogTraceTag(0x2388a54f /* tag_98kvp */, Categories.Infrastructure, Levels.Error, "GetThreadTimes system call failed.");
				return false;
			}

			if (!NativeMethods.QueryThreadCycleTime(s_currentThreadHandle, out m_startCycles))
			{
				ULSLogging.LogTraceTag(0x2388a550 /* tag_98kvq */, Categories.Infrastructure, Levels.Error, "QueryThreadCycleTime system call failed.");
				return false;
			}

			m_threadId = NativeMethods.GetCurrentThreadId();
			LastStatus = false;

			return true;
		}


		/// <summary>
		/// Get timing metrics since Start was called
		/// </summary>
		/// <returns>Success status</returns>
		public bool Stop()
		{
			LastStatus = false;

			if (NativeMethods.GetCurrentThreadId() != m_threadId)
			{
				// Thread Id mismatch between Start and Stop.
				// This is expected in async methods so not logging anything, would be too much spam.
				m_threadId = uint.MaxValue;
				return false;
			}

			m_threadId = uint.MaxValue;

			long creationTime, exitTime, kernelModeTime, userModeTime;

			if (!NativeMethods.GetThreadTimes(s_currentThreadHandle, out creationTime, out exitTime, out kernelModeTime, out userModeTime))
			{
				ULSLogging.LogTraceTag(0x2388a551 /* tag_98kvr */, Categories.Infrastructure, Levels.Error, "GetThreadTimes system call failed.");
				return false;
			}

			ulong cycles;

			if (!NativeMethods.QueryThreadCycleTime(s_currentThreadHandle, out cycles))
			{
				ULSLogging.LogTraceTag(0x2388a552 /* tag_98kvs */, Categories.Infrastructure, Levels.Error, "QueryThreadCycleTime system call failed.");
				return false;
			}

			LastStatus = true;
			CyclesUsed += m_startCycles >= cycles ? 0 : (long)(cycles - m_startCycles);
			KernelModeMilliseconds += m_startKernel >= kernelModeTime ? 0 : (decimal)(kernelModeTime - m_startKernel) / 10000m;
			UserModeMilliseconds += m_startUser >= userModeTime ? 0 : (decimal)(userModeTime - m_startUser) / 10000m;

			return true;
		}


		/// <summary>
		/// Suspend Cpu timer tree
		/// </summary>
		public void Suspend()
		{
			Stop();

			if (m_parent != null)
			{
				m_parent.Stop();
			}
		}


		/// <summary>
		/// Resume Cpu timer tree
		/// </summary>
		public void Resume()
		{
			Start();

			if (m_parent != null)
			{
				m_parent.Start();
			}
		}


		/// <summary>
		/// Increment the number of Service Calls
		/// </summary>
		public void IncrementServiceCallCount()
		{
			if (m_parent != null)
			{
				m_parent.IncrementServiceCallCount();
			}

			Interlocked.Increment(ref m_serviceCalls);
		}


		/// <summary>
		/// Increment the number of Http Requests
		/// </summary>
		public void IncrementHttpRequestCount()
		{
			if (m_parent != null)
			{
				m_parent.IncrementHttpRequestCount();
			}

			Interlocked.Increment(ref m_httpRequests);
		}


		/// <summary>
		/// Private Windows API definitions
		/// </summary>
		private static class NativeMethods
		{
			/// <summary>
			/// Gets the Id of the current native thread
			/// </summary>
			/// <returns>Thread Id</returns>
			[DllImport("kernel32.dll")]
			public static extern uint GetCurrentThreadId();


			/// <summary>
			/// Gets a psuedo handle to the current native thread
			/// </summary>
			/// <returns>Psuedo handle, doesn't need to be closed</returns>
			[DllImport("kernel32.dll")]
			public static extern IntPtr GetCurrentThread();


			/// <summary>
			/// Gets Cpu times for the specified native thread, accurate to around 15ms
			/// </summary>
			/// <param name="thread">Target thread</param>
			/// <param name="creationTime">Creation time as 100-nanosecond intervals since January 1, 1601 (UTC)</param>
			/// <param name="exitTime">Exit time as 100-nanosecond intervals since January 1, 1601 (UTC)</param>
			/// <param name="kernelModeTime">Number of 100-nanosecond intervals thread has spent in Kernel mode</param>
			/// <param name="userModeTime">Number of 100-nanosecond intervals thread has spent in User mode</param>
			/// <returns>Success status</returns>
			[DllImport("kernel32.dll")]
			public static extern bool GetThreadTimes(IntPtr thread, out long creationTime, out long exitTime, out long kernelModeTime, out long userModeTime);


			/// <summary>
			/// Query the current Cpu cycle count for a specified thread
			/// </summary>
			/// <param name="thread">Target thread</param>
			/// <param name="cycles">Number of cycles</param>
			/// <returns>Success status</returns>
			[DllImport("kernel32.dll")]
			public static extern bool QueryThreadCycleTime(IntPtr thread, out ulong cycles);
		}
	}
}