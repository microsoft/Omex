// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Store Timed Scope name
	/// </summary>
	public class TimedScopeDefinition
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
		public TimedScopeDefinition(string name)
		{
			// Activity won't be stated in case of an empty string
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("TimedScope name should not be empty or null");
			}

			Name = name;
		}
	}
}
