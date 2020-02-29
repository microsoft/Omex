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
		public ServiceContextAccessor(TContext? context = null)
		{
			m_serviceContext = context;
			m_actions = new LinkedList<Action<TContext>>();
		}


		internal void SetContext(TContext context)
		{
			m_serviceContext = context;

			foreach (Action<TContext> action in m_actions)
			{
				action(context);
			}

			m_actions.Clear();
		}


		void IServiceContextAccessor<TContext>.OnContextAvailable(Action<TContext> action)
		{
			if (m_serviceContext != null)
			{
				action(m_serviceContext);
			}
			else
			{
				m_actions.AddLast(action);
			}
		}


		TContext? IServiceContextAccessor<TContext>.ServiceContext => m_serviceContext;


		private readonly LinkedList<Action<TContext>> m_actions;
		private TContext? m_serviceContext;
	}
}
