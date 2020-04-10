// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Helper methods for <see cref="Exception" />
	/// </summary>
	public static class ExceptionHelpers
	{
		/// <summary>
		/// Returns true if exception could be handled or logged and false for unrecoverable exeptions
		/// like <see cref="OutOfMemoryException" /> and <see cref="StackOverflowException" />
		/// </summary>
		public static bool IsRecoverable(this Exception exception) =>
			!(exception is OutOfMemoryException)
			|| !(exception is StackOverflowException);
	}
}
