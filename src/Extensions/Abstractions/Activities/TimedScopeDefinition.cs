// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Store Timed Scope name
	/// </summary>
	public readonly struct TimedScopeDefinition
	{
		/// <summary>
		/// Timed Scope name
		/// </summary>
		public string Name { get; }


		/// <summary>
		/// TimedScopeDefinition constructor
		/// </summary>
		/// <param name="name">Timed Scope name</param>
		/// <exception cref="ArgumentException">Thrown when name null or empty</exception>
		public TimedScopeDefinition(string name) =>
			Name = string.IsNullOrWhiteSpace(name)
				? throw new ArgumentException("TimedScope name must not be empty or null") // Activity won't be stated in case of an empty string
				: name;


		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type
		/// </summary>
		/// <param name="other">An object to compare with this object</param>
		public bool Equals(TimedScopeDefinition other) => string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);


		/// <summary>
		/// Determines whether the specified <see cref="object" />, is equal to this instance
		/// </summary>
		/// <param name="obj">The <see cref="object" /> to compare with this instance</param>
		public override bool Equals(object? obj) => obj is TimedScopeDefinition other && Equals(other);


		/// <summary>
		/// Returns a hash code for this instance
		/// </summary>
		public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);


		/// <summary>
		/// Implements the operator ==
		/// </summary>
		/// <param name="left">The left</param>
		/// <param name="right">The right</param>
		public static bool operator ==(TimedScopeDefinition left, TimedScopeDefinition right) => Equals(left, right);


		/// <summary>
		/// Implements the operator !=
		/// </summary>
		/// <param name="left">The left</param>
		/// <param name="right">The right</param>
		public static bool operator !=(TimedScopeDefinition left, TimedScopeDefinition right) => !Equals(left, right);
	}
}
