// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	public static class MockServiceFabricHostBuilder
	{
		public static ServiceFabricHostBuilder<TService, TContext> CreateMockBuilder<TService, TContext>(IHostBuilder builder)
			where TService : IServiceFabricService<TContext>
			where TContext : ServiceContext =>
				new(builder);
	}
}
