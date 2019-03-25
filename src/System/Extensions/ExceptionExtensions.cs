// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace Microsoft.Omex.System.Extensions
{
	/// <summary>
	/// Extension methods for exceptions
	/// </summary>
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Checks if exception is fatal and app should terminate
		/// </summary>
		/// <param name="exception">Exception object</param>
		/// <returns>True if exception is fatal</returns>
		public static bool IsFatalException(this Exception exception)
		{
			return
				exception is StackOverflowException ||
				exception is AccessViolationException ||
				exception is AppDomainUnloadedException ||
				exception is ThreadAbortException ||
				exception is SecurityException ||
				exception is SEHException;
		}
	}
}