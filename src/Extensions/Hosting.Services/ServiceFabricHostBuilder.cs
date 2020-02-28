// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Wrapper on top of IHostBuilder to propagete proper context type and avoid type registration mistakes
	/// </summary>
	public sealed class ServiceFabricHostBuilder<TContext> where TContext : ServiceContext
	{
		/// <summary>
		/// Method should be called only by extensions of this class
		/// If you are creating service please register dependencies before colling BuildService
		/// </summary>
		public ServiceFabricHostBuilder<TContext> ConfigureServices(Action<HostBuilderContext, IServiceCollection> action)
		{
			m_builder.ConfigureServices(action);
			return this;
		}


		internal ServiceFabricHostBuilder(IHostBuilder builder) => m_builder = builder;


		private readonly IHostBuilder m_builder;
	}
}
