// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Extensions;

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Authentication;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
	#region ConfigureFeatureManagement

	[TestMethod]
	public void ConfigureFeatureManagement_WhenCalled_RegistersAllServices()
	{
		// ARRANGE
		IServiceCollection services = new ServiceCollection();
		Mock<IConfigurationSection> mockFeatureOverrideSection = new();

		Mock<IConfiguration> mockConfiguration = new();
		mockConfiguration.Setup(c => c.GetSection(nameof(FeatureOverrideSettings))).Returns(mockFeatureOverrideSection.Object);

		// ACT
		IServiceCollection result = services.ConfigureFeatureManagement(mockConfiguration.Object);

		// ASSERT
		Assert.IsNotNull(result);
		Assert.AreSame(services, result);

		// Verify IFeatureManager is registered (from AddFeatureManagement).
		ServiceDescriptor? featureManagerDescriptor = services.FirstOrDefault(sd =>
			sd.ServiceType == typeof(IFeatureManager));
		Assert.IsNotNull(featureManagerDescriptor);
	}

	[TestMethod]
	public void ConfigureFeatureManagement_WhenCustomerIdProviderIsNull_RegistersEntraIdProvider()
	{
		// ARRANGE
		IServiceCollection services = new ServiceCollection();
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		services.AddLogging();

		Mock<IConfigurationSection> mockFeatureOverrideSection = new();
		Mock<IConfiguration> mockConfiguration = new();
		mockConfiguration.Setup(c => c.GetSection(nameof(FeatureOverrideSettings))).Returns(mockFeatureOverrideSection.Object);

		// ACT
		services.ConfigureFeatureManagement(mockConfiguration.Object, customerIdProvider: null);

		// ASSERT
		ServiceProvider serviceProvider = services.BuildServiceProvider();
		ICustomerIdProvider? customerIdProvider = serviceProvider.GetService<ICustomerIdProvider>();

		Assert.IsNotNull(customerIdProvider);
		Assert.IsInstanceOfType<EntraIdProvider>(customerIdProvider);
	}

	[TestMethod]
	public void ConfigureFeatureManagement_WhenCustomerIdProviderIsProvided_RegistersProvidedInstance()
	{
		// ARRANGE
		IServiceCollection services = new ServiceCollection();
		Mock<ICustomerIdProvider> mockCustomerIdProvider = new();

		Mock<IConfigurationSection> mockFeatureOverrideSection = new();
		Mock<IConfiguration> mockConfiguration = new();
		mockConfiguration.Setup(c => c.GetSection(nameof(FeatureOverrideSettings))).Returns(mockFeatureOverrideSection.Object);

		// ACT
		services.ConfigureFeatureManagement(mockConfiguration.Object, customerIdProvider: mockCustomerIdProvider.Object);

		// ASSERT
		ServiceProvider serviceProvider = services.BuildServiceProvider();
		ICustomerIdProvider? customerIdProvider = serviceProvider.GetService<ICustomerIdProvider>();

		Assert.IsNotNull(customerIdProvider);
		Assert.AreSame(mockCustomerIdProvider.Object, customerIdProvider);
	}

	#endregion
}
