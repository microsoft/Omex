/***************************************************************************************************
	LogEventSender.cs

	Class used to define which test was run when a log event happened
***************************************************************************************************/

using System;

namespace Microsoft.Omex.System.UnitTests.Shared.Logging
{
	/// <summary>
	/// Class used to define which test was run when a log event happened
	/// </summary>
	internal class LogEventSender
	{
		[ThreadStatic]
		private static UnitTestBase s_runningTest;


		/// <summary>
		/// The currently running test
		/// </summary>
		public UnitTestBase Sender
		{
			get
			{
				return s_runningTest;
			}
			set
			{
				s_runningTest = value;
			}
		}
	}
}
