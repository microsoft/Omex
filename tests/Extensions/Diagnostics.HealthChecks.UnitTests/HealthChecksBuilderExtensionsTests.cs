// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Testing.Helpers;
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
			HttpStatusCode code = HttpStatusCode.HttpVersionNotSupported;
			string scheme = Uri.UriSchemeGopher;
			Func<HttpResponseMessage, HealthCheckResult, HealthCheckResult> additionalCheck = (r, h) => HealthCheckResult.Degraded();
			KeyValuePair<string, object>[] reportData = new Dictionary<string, object>
			{
				{ "testKey1", new object() },
				{ "testKey2", "value" }
			}.ToArray();

			IServiceProvider provider = GetBuilder()
				.AddHttpEndpointCheck(checkName, endpoitName, path, method, scheme, code, additionalCheck, reportData)
				.Services
				.BuildServiceProvider();

			HttpHealthCheckParameters parameters = GetParameters(provider, checkName);

			Assert.AreEqual(endpoitName, parameters.EndpointName, nameof(HttpHealthCheckParameters.EndpointName));
			Assert.AreEqual(path, parameters.RelativeUri.ToString(), nameof(HttpHealthCheckParameters.RelativeUri));
			Assert.AreEqual(method, parameters.Method, nameof(HttpHealthCheckParameters.Method));
			Assert.AreEqual(scheme, parameters.Scheme, nameof(HttpHealthCheckParameters.Scheme));
			Assert.AreEqual(code, parameters.ExpectedStatus, nameof(HttpHealthCheckParameters.ExpectedStatus));
			Assert.AreEqual(additionalCheck, parameters.AdditionalCheck, nameof(HttpHealthCheckParameters.AdditionalCheck));
			CollectionAssert.AreEquivalent(reportData, parameters.ReportData.ToArray(), nameof(HttpHealthCheckParameters.ReportData));
		}

		[DataTestMethod]
		[DataRow("https://localhost")]
		public void AddServiceFabricHealthChecks_InvalidPath_ThrowException(string path)
		{
			Assert.ThrowsException<UriFormatException>(() =>
				GetBuilder().AddHttpEndpointCheck("CheKName", "EndpointName", path, scheme: Uri.UriSchemeHttps));
		}

		private IHealthChecksBuilder GetBuilder() =>
			new ServiceCollection()
				.AddSingleton(new Mock<ActivitySource>().Object)
				.AddSingleton(new Mock<IAccessor<IServicePartition>>().Object)
				.AddSingleton(new Mock<IAccessor<ServiceContext>>().Object)
				.AddServiceFabricHealthChecks();

		private HttpHealthCheckParameters GetParameters(IServiceProvider provider, string checkName)
		{
			IOptions<HealthCheckServiceOptions> options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
			HealthCheckRegistration? registration = options.Value.Registrations.SingleOrDefault(r => string.Equals(checkName, r.Name, StringComparison.Ordinal));
			NullableAssert.IsNotNull(registration, "HealthCheck should be registered");

			IHealthCheck check = registration.Factory(provider);
			Assert.IsInstanceOfType(check, typeof(HttpEndpointHealthCheck));
			return ((HttpEndpointHealthCheck)check).Parameters;
		}
	}
}
