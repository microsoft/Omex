// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class HealthChecksBuilderExtensionsTests
	{
		[TestMethod]
		public void AddServiceFabricHealthChecks_RegisterPublisherAndChecks()
		{
			string checkName = "MockHttpCheck";
			string endpoitName = "MockEndpoitName";
			string path = "MockPath";
			HttpMethod method = HttpMethod.Post;
			string scheme = Uri.UriSchemeGopher;
			Action<HttpResponseMessage, HealthCheckResult> additionalCheck = (r, h) => { };
			KeyValuePair<string, object>[] reportData = new Dictionary<string, object>
			{
				{ "testKey1", new object() },
				{ "testKey2", "value" }
			}.ToArray();

			IServiceProvider provider = new ServiceCollection()
				.AddSingleton(new Mock<ITimedScopeProvider>().Object)
				.AddSingleton(new Mock<IAccessor<IServicePartition>>().Object)
				.AddSingleton(new Mock<IAccessor<ServiceContext>>().Object)
				.AddServiceFabricHealthChecks()
				.AddHttpEndpointCheck(checkName, endpoitName, path, method, scheme, additionalCheck, reportData)
				.Services
				.BuildServiceProvider();

			IOptions<HealthCheckServiceOptions> options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
			HealthCheckRegistration registration = options.Value.Registrations.SingleOrDefault(r => string.Equals(checkName, r.Name, StringComparison.Ordinal));

			Assert.IsNotNull(registration, "HealthCheck should be registered");

			IHealthCheck check = registration.Factory(provider);

			Assert.IsInstanceOfType(check, typeof(HttpEndpointHealthCheck));

			HttpHealthCheckParameters parameters = ((HttpEndpointHealthCheck)check).Parameters;

			Assert.AreEqual(endpoitName, parameters.EndpointName, nameof(HttpHealthCheckParameters.EndpointName));
			Assert.AreEqual(path, parameters.RelativeUri.ToString(), nameof(HttpHealthCheckParameters.RelativeUri));
			Assert.AreEqual(method, parameters.Method, nameof(HttpHealthCheckParameters.Method));
			Assert.AreEqual(scheme, parameters.Scheme, nameof(HttpHealthCheckParameters.Scheme));
			Assert.AreEqual(additionalCheck, parameters.AdditionalCheck, nameof(HttpHealthCheckParameters.AdditionalCheck));
			CollectionAssert.AreEquivalent(reportData, parameters.ReportData.ToArray(), nameof(HttpHealthCheckParameters.ReportData));
		}

		[DataTestMethod]
		[DataRow("http://localhost")]
		public void AddServiceFabricHealthChecks_InvalidPath_ThrowException(string path)
		{
			Assert.ThrowsException<UriFormatException>(() =>
				new ServiceCollection()
					.AddServiceFabricHealthChecks()
					.AddHttpEndpointCheck("CheKName", "EndpointName", path));
		}
	}
}
