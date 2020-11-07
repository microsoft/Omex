// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	internal class OmexKestrelListener : ICommunicationListener
	{
		private readonly IServer m_server;

		private readonly WebListenerConfiguration m_configuration;

		public OmexKestrelListener(IServer server, WebListenerConfiguration configuration)
		{
			m_server = server;
			m_configuration = configuration;
		}

		public Task<string> OpenAsync(CancellationToken cancellationToken)
		{
			// Listener already opened so just returning listener uri
			string text = m_server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();

			string publishAddress = m_configuration.PublishAddress;
			if (text.Contains("://+:"))
			{
				text = text.Replace("://+:", "://" + publishAddress + ":");
			}
			else if (text.Contains("://[::]:"))
			{
				text = text.Replace("://[::]:", "://" + publishAddress + ":");
			}

			text = text.TrimEnd('/') + m_configuration.UrlSuffix;

			return Task.FromResult(text);
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
