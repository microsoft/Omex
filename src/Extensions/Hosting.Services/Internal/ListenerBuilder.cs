// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class ListenerBuilder<TService> : IListenerBuilder<TService>
	{
		public ListenerBuilder(
			string name,
			IServiceProvider provider,
			Func<IServiceProvider, TService, ICommunicationListener> createListener) =>
			(Name, m_provider, m_createListener) = (name, provider, createListener);

		public string Name { get; }

		public ICommunicationListener Build(TService service) =>
			m_createListener(m_provider, service);

		private readonly Func<IServiceProvider, TService, ICommunicationListener> m_createListener;
		private readonly IServiceProvider m_provider;
	}
}
