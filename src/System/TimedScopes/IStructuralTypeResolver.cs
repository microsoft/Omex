// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Interface to resolve a type to an instance, with parent inheritance
	/// </summary>
	public interface IStructuralTypeResolver : ITypeResolver
	{
		/// <summary>
		/// Parent resolver
		/// </summary>
		IStructuralTypeResolver Parent { get; set; }


		/// <summary>
		/// Resolve type 'TFrom' to an instance of type 'TFrom' in context of the current call
		/// </summary>
		/// <typeparam name="TFrom">type to resolve</typeparam>
		/// <returns>instance of type 'TFrom'</returns>
		TFrom ResolveInCallContext<TFrom>()
			where TFrom : class;


		/// <summary>
		/// Is the type 'TFrom' known to the type resolver in context of the current call
		/// </summary>
		/// <typeparam name="TFrom">type to resolve</typeparam>
		/// <param name="instance">the stored value, if one exists</param>
		/// <returns>true if a type is known to the resover, false otherwise</returns>
		bool DoesTypeExistInCallContext<TFrom>(out TFrom instance)
			where TFrom : class;
	}
}