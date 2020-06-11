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
		/// Throw exception if value is null, otherwise returns not-nullable value
		/// </summary>
		/// <typeparam name="T">type of the value</typeparam>
		/// <param name="value">value to check</param>
		/// <param name="name">name of the value for exception message</param>
		/// <exception cref="ArgumentNullException">if value is null</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ThrowIfNull<T>(T? value, string? name = null) where T : class
			=> value ?? throw new ArgumentNullException(name);

		/// <summary>
		/// Throw exception if value is null, otherwise returns not-nullable value
		/// </summary>
		/// <typeparam name="T">type of the value</typeparam>
		/// <param name="value">value to check</param>
		/// <param name="name">name of the value for exception message</param>
		/// <exception cref="ArgumentNullException">if value is null</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ThrowIfNull<T>(T? value, string? name = null) where T : struct
			=> value ?? throw new ArgumentNullException(name);

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
				return value!; // `!` required because in net472 IsNullOrWhiteSpace does not have proper attributes
			}

			if (ThrowIfNull(value, name).Length == 0)
			{
				throw new ArgumentException("String is empty", name);
			}

			throw new ArgumentException("String contains only white space characters", name);
		}
	}
}
