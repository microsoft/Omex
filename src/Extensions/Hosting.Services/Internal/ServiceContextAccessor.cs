// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal class ServiceContextAccessor<TContext> : IServiceContextAccessor<TContext>
		where TContext : ServiceContext
	{
		/// <summary>
		/// Creates Service context acceessor for dependency injection, because it could be not available during container creation
		/// </summary>
		/// <param name="context">ServiceContext if it's immediately available from dependency injection</param>
		public ServiceContextAccessor(TContext? context = null)
		{
			m_serviceContext = context;
			m_actions = new LinkedList<WeakReference<Action<TContext>>>();
		}


		/// <summary>
		/// Set ServiceContext when it's available
		/// </summary>
		/// <remarks>
		/// Beside saving context it will also execute saved activities that require context,
		/// after execution activity list will be cleared
		/// </remarks>
		internal void SetContext(TContext context)
		{
			m_serviceContext = context;

			foreach (WeakReference<Action<TContext>> actionReference in m_actions)
			{
				if (actionReference.TryGetTarget(out Action<TContext>? action) && action != null)
				{
					action(context);
				}
			}

			m_actions.Clear();
		}


		/// <inheritdoc />
		void IServiceContextAccessor<TContext>.OnContextAvailable(Action<TContext> action)
		{
			if (m_serviceContext != null)
			{
				action(m_serviceContext);
			}
			else
			{
				m_actions.AddLast(new WeakReference<Action<TContext>>(action));
			}
		}


		/// <inheritdoc />
		TContext? IServiceContextAccessor<TContext>.ServiceContext => m_serviceContext;


		private readonly LinkedList<WeakReference<Action<TContext>>> m_actions;
		private TContext? m_serviceContext;
	}
}
