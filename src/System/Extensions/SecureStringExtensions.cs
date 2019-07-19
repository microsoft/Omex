// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Security;
using System.Runtime.InteropServices;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Extensions
{
	/// <summary>
	/// Extensions class for SecureString.
	/// </summary>
	public static class SecureStringExtensions
	{
		/// <summary>
		/// Converts a secure string to plain text
		/// </summary>
		/// <param name="secureString">Secure string</param>
		/// <returns>Plain text value of secure string</returns>
		public static string ToPlainText(this SecureString secureString)
		{
			if (!Code.ValidateArgument(secureString, nameof(secureString), TaggingUtilities.ReserveTag(0)))
			{
				return null;
			}

			IntPtr pointer = IntPtr.Zero;
			try
			{
				pointer = Marshal.SecureStringToGlobalAllocUnicode(secureString);
				return Marshal.PtrToStringUni(pointer);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(pointer);
			}
		}
	}
}
