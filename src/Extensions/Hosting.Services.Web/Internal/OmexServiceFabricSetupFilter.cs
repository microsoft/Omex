// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	internal class OmexServiceFabricSetupFilter : IStartupFilter
	{
		private readonly string m_urlSuffix;
		private readonly ServiceFabricIntegrationOptions m_options;

		internal OmexServiceFabricSetupFilter(string urlSuffix, ServiceFabricIntegrationOptions options)
		{
			m_urlSuffix = urlSuffix;
			m_options = options;
		}

		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
		{
			return app =>
			{
				app.UseServiceFabricMiddleware(m_urlSuffix);
				if (m_options.HasFlag(ServiceFabricIntegrationOptions.UseReverseProxyIntegration))
				{
					app.UseServiceFabricReverseProxyIntegrationMiddleware();
				}
				next(app);
			};
		}
	}
}
