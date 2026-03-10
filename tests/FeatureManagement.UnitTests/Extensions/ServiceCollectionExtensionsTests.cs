// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests.Extensions;

using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
	#region AddOmexFeatureManagement

	[TestMethod]
	public void AddOmexFeatureManagement_WhenCalled_RegistersAllServices()
	{
		// ARRANGE
		IServiceCollection services = new ServiceCollection();
		Mock<IConfigurationSection> mockFeatureOverrideSection = new();

		Mock<IConfiguration> mockConfiguration = new();
		mockConfiguration.Setup(c => c.GetSection(nameof(FeatureOverrideSettings))).Returns(mockFeatureOverrideSection.Object);

		// ACT
		IServiceCollection result = services.AddOmexFeatureManagement(mockConfiguration.Object);

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
