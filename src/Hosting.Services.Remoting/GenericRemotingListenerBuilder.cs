// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Remoting
{
	internal sealed class GenericRemotingListenerBuilder<TService> : RemotingListenerBuilder<TService>
		where TService : IServiceFabricService<ServiceContext>
	{
		private readonly IServiceProvider m_provider;
		private readonly Func<IServiceProvider, TService, IService> m_createService;

		public GenericRemotingListenerBuilder(
			string name,
			IServiceProvider provider,
			Func<IServiceProvider, TService, IService> createService,
			FabricTransportRemotingListenerSettings? settings = null)
				: base(name, settings)
		{
			m_provider = provider;
			m_createService = createService;
		}

		public override IService BuildService(TService service) => m_createService(m_provider, service);
	}
}
