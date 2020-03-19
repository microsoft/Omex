﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Compatibility.Logger
{
	/// <summary>
	/// Logging category
	/// </summary>
	[Obsolete("In ILogger category provided as generic type parameter for ILogger<T> so Category struct should not be used", false)]
	public readonly struct Category : IEquatable<Category>
	{
		/// <summary>
		/// Category
		/// </summary>
		/// <param name="name">Category name</param>
		public Category(string name) =>
			Name = string.IsNullOrWhiteSpace(name)
				? throw new ArgumentException("Category name must not null or empty", nameof(name))
				: name;

		/// <summary>
		/// Name
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type
		/// </summary>
		/// <param name="other">An object to compare with this object</param>
		public bool Equals(Category other) => string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// Determines whether the specified <see cref="object" />, is equal to this instance
		/// </summary>
		/// <param name="obj">The <see cref="object" /> to compare with this instance</param>
		public override bool Equals(object? obj) => obj is Category other && Equals(other);

		/// <summary>
		/// Returns a hash code for this instance
		/// </summary>
		public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

		/// <summary>
		/// Implements the operator ==
		/// </summary>
		/// <param name="left">The left</param>
		/// <param name="right">The right</param>
		public static bool operator ==(Category left, Category right) => Equals(left, right);

		/// <summary>
		/// Implements the operator !=
		/// </summary>
		/// <param name="left">The left</param>
		/// <param name="right">The right</param>
		public static bool operator !=(Category left, Category right) => !Equals(left, right);
	}
}
