// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Type for accessing object that are not available during dependency injection container creation
	/// </summary>
	/// <typeparam name="TValue">Type that will be accessable after dependency injection creation</typeparam>
	public class Accessor<TValue> : IAccessor<TValue>, IAccessorSetter<TValue>
		where TValue : class
	{
		/// <summary>
		/// Creates Service context accessor for dependency injection, because it could be not available during container creation
		/// </summary>
		/// <param name="context">ServiceContext if it's immediately available from dependency injection</param>
		public Accessor(TValue? context = null)
		{
			m_serviceContext = context;
			m_actions = new LinkedList<WeakReference<Action<TValue>>>();
		}

		/// <inheritdoc />
		void IAccessorSetter<TValue>.SetContext(TValue context)
		{
			m_serviceContext = context;

			foreach (WeakReference<Action<TValue>> actionReference in m_actions)
			{
				if (actionReference.TryGetTarget(out Action<TValue>? action) && action != null)
				{
					action(context);
				}
			}

			m_actions.Clear();
		}

		/// <inheritdoc />
		void IAccessor<TValue>.OnAvailable(Action<TValue> action)
		{
			if (m_serviceContext != null)
			{
				action(m_serviceContext);
			}
			else
			{
				m_actions.AddLast(new WeakReference<Action<TValue>>(action));
			}
		}

		/// <inheritdoc />
		TValue? IAccessor<TValue>.Value => m_serviceContext;

		private readonly LinkedList<WeakReference<Action<TValue>>> m_actions;
		private TValue? m_serviceContext;
	}
}
