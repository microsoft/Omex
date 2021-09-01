// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Class provides common validation methods
	/// </summary>
	public static class Validation
	{
		/// <summary>
		/// Throw exception if string is null or empty or white space, otherwise returns not-nullable value
		/// </summary>
		/// <param name="value">value to check</param>
		/// <param name="name">name of the value for exception message</param>
		/// <exception cref="ArgumentNullException">if value is null</exception>
		/// <exception cref="ArgumentException">if value is empty or white space</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ThrowIfNullOrWhiteSpace(string? value, string? name = null)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				return value!; // `!` required because in netstandard2.0 IsNullOrWhiteSpace does not have proper attributes
			}

			_ = value ?? throw new ArgumentNullException(name);

			if (value.Length == 0)
			{
				throw new ArgumentException("String is empty", name);
			}

			throw new ArgumentException("String contains only white space characters", name);
		}

		/// <summary>
		/// Throw exception if object is null, otherwise returns not-nullable object
		/// </summary>
		/// <param name="value">nullable object to check</param>
		/// <param name="name">name of the object for exception message</param>
		/// <exception cref="ArgumentNullException">if object is null</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object ThrowIfNull(object? value, string? name = null)
		{
			if (value == null)
			{
				throw new ArgumentNullException(message: "Object is null", paramName: name);
			}
			return value;
		}
	}
}
