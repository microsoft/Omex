﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Type for accessing an object that is not available during dependency injection container creation
	/// </summary>
	/// <typeparam name="TValue">Type that will be accessable after dependency injection creation</typeparam>
	public class Accessor<TValue> : IAccessor<TValue>, IAccessorSetter<TValue>
		where TValue : class
	{
		/// <summary>
		/// Creates accessor for dependency injection, for values that not available during container creation
		/// </summary>
		/// <param name="value">Value if it's immediately available from dependency injection</param>
		public Accessor(TValue? value = null)
		{
			m_value = value;
			m_actions = new LinkedList<WeakReference<Action<TValue>>>();
		}

		/// <inheritdoc />
		void IAccessorSetter<TValue>.SetContext(TValue value)
		{
			m_value = value;

			foreach (WeakReference<Action<TValue>> actionReference in m_actions)
			{
				if (actionReference.TryGetTarget(out Action<TValue>? action) && action != null)
				{
					action(value);
				}
			}

			m_actions.Clear();
		}

		/// <inheritdoc />
		void IAccessor<TValue>.OnAvailable(Action<TValue> action)
		{
			if (m_value != null)
			{
				action(m_value);
			}
			else
			{
				m_actions.AddLast(new WeakReference<Action<TValue>>(action));
			}
		}

		/// <inheritdoc />
		TValue? IAccessor<TValue>.Value => m_value;

		private readonly LinkedList<WeakReference<Action<TValue>>> m_actions;
		private TValue? m_value;
	}
}
