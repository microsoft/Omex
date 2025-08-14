// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests.Extensions;

using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.Omex.FeatureManagement.Extensions;
using Microsoft.OMEX.Experimentation.Contract;
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
		Mock<IConfigurationSection> mockEcsConfigSection = new();
		Mock<IConfigurationSection> mockFeatureOverrideSection = new();

		Mock<IConfiguration> mockConfiguration = new();
		mockConfiguration.Setup(c => c.GetSection(nameof(EcsOptions))).Returns(mockEcsConfigSection.Object);
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

	#endregion
}
