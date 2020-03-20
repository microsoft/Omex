// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Logging
{
	/// <summary>
	/// Logging category
	/// </summary>
	public class Category : IEquatable<Category>
	{
		/// <summary>
		/// Category
		/// </summary>
		/// <param name="name">name</param>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is null or white space</exception>
		public Category(string name)
		{
			Name = Code.ExpectsNotNullOrWhiteSpaceArgument(name, nameof(name), null);
		}

		/// <summary>
		/// Name
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type
		/// </summary>
		/// <param name="other">An object to compare with this object</param>
		public bool Equals(Category other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return string.Equals(Name, other.Name, StringComparison.InvariantCulture);
		}

		/// <summary>
		/// Determines whether the specified <see cref="object" />, is equal to this instance
		/// </summary>
		/// <param name="obj">The <see cref="object" /> to compare with this instance</param>
		public override bool Equals(object obj) => ReferenceEquals(this, obj) || (obj is Category other && Equals(other));

		/// <summary>
		/// Returns a hash code for this instance
		/// </summary>
		public override int GetHashCode() => Name != null ? StringComparer.InvariantCulture.GetHashCode(Name) : 0;

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
