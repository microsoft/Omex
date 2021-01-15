// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	internal class OmexKestrelListener : ICommunicationListener
	{
		private readonly IServer m_server;

		private readonly string m_publishAddress;

		private readonly int m_port;

		public OmexKestrelListener(IServer server, string publishAddress, int port)
		{
			m_server = server;
			m_publishAddress = publishAddress;
			m_port = port;
		}

		public Task<string> OpenAsync(CancellationToken cancellationToken)
		{
			// Listener already opened so just returning listener uri
			string? address = m_server.Features.Get<IServerAddressesFeature>()
				.Addresses.FirstOrDefault(a => a.Contains(":" + m_port));

			if (address == null)
			{
				throw new ArgumentException($"Failed to find address for port '{m_port}'");
			}

			string publishAddress = m_publishAddress;
			if (address.Contains("://+:"))
			{
				address = address.Replace("://+:", "://" + publishAddress + ":");
			}
			else if (address.Contains("://[::]:"))
			{
				address = address.Replace("://[::]:", "://" + publishAddress + ":");
			}

			address = address.TrimEnd('/');

			return Task.FromResult(address);
		}

		public Task CloseAsync(CancellationToken cancellationToken)
		{
			// Since Kestrel is hosted in main generic host listener close is ignored (would be stopped with a service)
			return Task.CompletedTask;
		}

		public void Abort()
		{
			// Since Kestrel is hosted in main generic host listener Abort is ignored (would be stopped with a service)
		}
	}
}
