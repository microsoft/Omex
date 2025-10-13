﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hosting.Services.Web.UnitTests;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.UnitTests;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests.Internal
{
	[TestClass]
	public class HostBuilderExtensionsTests
	{
		[TestMethod]
		public void BuildStatelessWebService_UseUniqueServiceUrl_Failing()
		{
			Assert.ThrowsExactly<ArgumentException>(() =>
			{
				new HostBuilder().BuildStatelessWebService<MockStartup>(
					"someService1",
					[],
					ServiceFabricIntegrationOptions.UseUniqueServiceUrl | ServiceFabricIntegrationOptions.UseReverseProxyIntegration);
			});
		}

		[TestMethod]
		[DoNotParallelize] // Modifies static state in SfConfigurationProviderHelper
		public async Task BuildStatelessWebService_RegisterListeners()
		{
			// Use random ports from private range.
			Random random = new();
			(string name, int port) = ("httpListener", random.Next(49152, 65535));
			(string name, int port) httpListener2 = ("httpsListener", random.Next(49152, 65535));

			SfConfigurationProviderHelper.SetPublishAddress();
			SfConfigurationProviderHelper.SetPortVariable(name, port);
			SfConfigurationProviderHelper.SetPortVariable(httpListener2.name, httpListener2.port);

			IHost host = new HostBuilder().BuildStatelessWebService<MockStartup>(
				"someService2",
				[
					new WebEndpointInfo(name, settingForCertificateCommonName: null),
					new WebEndpointInfo(httpListener2.name, settingForCertificateCommonName: null)
				]);

			IListenerBuilder<OmexStatelessService>[] builders = [.. host.Services.GetRequiredService<IEnumerable<IListenerBuilder<OmexStatelessService>>>()];
			Assert.HasCount(2, builders, "Two endpoints should be registered as listeners");
			Assert.IsTrue(builders.Any(b => b.Name == name), $"Listener builder for {name} not found");
			Assert.IsTrue(builders.Any(b => b.Name == httpListener2.name), $"Listener builder for {httpListener2.name} not found");

			await host.StartAsync();

			ICollection<string>? addresses = host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
			Assert.HasCount(2, builders, "Two addresses should be registered");
			Assert.IsNotNull(addresses, "Addresses should be registered");
			Assert.IsTrue(addresses.Any(address => address.EndsWith($":{port}")), $"Address for {name} not found");
			Assert.IsTrue(addresses.Any(address => address.EndsWith($":{httpListener2.port}")), $"Address for {httpListener2.name} not found");
		}
	}
}
