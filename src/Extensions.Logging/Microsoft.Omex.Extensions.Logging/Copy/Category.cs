// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Logging category
	/// </summary>
	public struct Category : IEquatable<Category>
	{
		/// <summary>
		/// Category
		/// </summary>
		public Category(string name) => Name = name;


		/// <summary>
		/// Name
		/// </summary>
		public string Name { get; }


		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type
		/// </summary>
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
		public override bool Equals(object obj) => ReferenceEquals(this, obj) || (obj is Category other && Equals(other));


		/// <summary>
		/// Returns a hash code for this instance
		/// </summary>
		public override int GetHashCode() => Name != null ? StringComparer.InvariantCulture.GetHashCode(Name) : 0;


		/// <summary>
		/// Implements the operator ==
		/// </summary>
		public static bool operator ==(Category left, Category right) => Equals(left, right);


		/// <summary>
		/// Implements the operator !=
		/// </summary>
		public static bool operator !=(Category left, Category right) => !Equals(left, right);
	}
}
