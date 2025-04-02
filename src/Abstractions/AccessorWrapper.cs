using System;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.Omex.Extensions.Hosting.Services;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// A wrapper class for IAccessor that provides additional functionality.
	/// </summary>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class AccessorWrapper<TValue> : IAccessor<TValue> where TValue : class
	{
		private readonly IAccessorSetter<TValue> m_accessorSetter;

		/// <summary>
		/// Initializes a new instance of the <see cref="AccessorWrapper{TValue}"/> class.
		/// </summary>
		/// <param name="accessorSetter">The accessor setter.</param>
		public AccessorWrapper(IAccessorSetter<TValue> accessorSetter)
		{
			m_accessorSetter = accessorSetter;
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		public TValue? Value => m_accessorSetter is IAccessor<TValue> accessor ? accessor.Value : null;

		/// <summary>
		/// Gets the value or throws an exception if the value is not available.
		/// </summary>
		/// <returns>The value.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the value is not available.</exception>
		public TValue GetValueOrThrow()
		{
			if (m_accessorSetter is IAccessor<TValue> accessor)
			{
				return accessor.GetValueOrThrow();
			}
			throw new InvalidOperationException("Value is not available.");
		}

		/// <summary>
		/// Registers a function to be called when the value is first set.
		/// </summary>
		/// <param name="function">The function to call.</param>
		/// <exception cref="InvalidOperationException">Thrown when the accessor does not support OnFirstSet.</exception>
		public void OnFirstSet(Action<TValue> function)
		{
			if (m_accessorSetter is IAccessor<TValue> accessor)
			{
				accessor.OnFirstSet(function);
			}
			else
			{
				throw new InvalidOperationException("Accessor does not support OnFirstSet.");
			}
		}
	}
}
