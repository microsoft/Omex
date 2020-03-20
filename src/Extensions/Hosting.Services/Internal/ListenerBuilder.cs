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
			Func<TService, ICommunicationListener> createListener) =>
			(Name, m_createListener) = (name, createListener);

		public string Name { get; }

		public ICommunicationListener Build(TService service) =>
			m_createListener(service);

		private readonly Func<TService, ICommunicationListener> m_createListener;
	}
}
