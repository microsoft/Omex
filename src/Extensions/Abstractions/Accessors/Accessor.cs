// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Abstractions.Accessors
{
	/// <summary>
	/// Type for accessing an object that is not available during dependency injection container creation
	/// </summary>
	/// <typeparam name="TValue">Type that will be accessible after dependency injection creation</typeparam>
	public class Accessor<TValue> : IAccessor<TValue>, IAccessorSetter<TValue>
		where TValue : class
	{
		/// <summary>
		/// Creates accessor for dependency injection, for values that not available during container creation
		/// </summary>
		/// <param name="logger">logger to write callback failures</param>
		/// <param name="value">Value if it's immediately available from dependency injection</param>
		public Accessor(ILogger<Accessor<TValue>> logger, TValue? value = null)
		{
			m_logger = logger;
			m_value = value;
			m_actions = new LinkedList<WeakReference<Action<TValue>>>();
		}

		/// <inheritdoc />
		void IAccessorSetter<TValue>.SetValue(TValue value)
		{
			m_value = value;

			foreach (WeakReference<Action<TValue>> actionReference in m_actions)
			{
				if (actionReference.TryGetTarget(out Action<TValue>? action) && action != null)
				{
					action(value);
				}
			}
		}

		/// <inheritdoc />
		void IAccessor<TValue>.OnUpdated(Action<TValue> action)
		{
			if (m_value != null)
			{
				try
				{
					action(m_value);
				}
				catch (Exception ex)
				{
					m_logger.LogError(ex, "Exception in accessor callback execution");
				}
			}
			else
			{
				m_actions.AddLast(new WeakReference<Action<TValue>>(action));
			}
		}

		/// <inheritdoc />
		TValue? IAccessor<TValue>.Value => m_value;

		private readonly LinkedList<WeakReference<Action<TValue>>> m_actions;
		private readonly ILogger<Accessor<TValue>> m_logger;
		private TValue? m_value;
	}
}
