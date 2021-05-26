// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class HealthChecksBuilderExtensionsTests
	{
		[DataTestMethod]
		[DynamicData(nameof(GetHeaders), DynamicDataSourceType.Method)]
		public void AddServiceFabricHealthChecks_RegisterPublisherAndChecks(IReadOnlyDictionary<string, IEnumerable<string>> headers)
		{
			string checkName = "MockHttpCheck";
			string endpoitName = "MockEndpoitName";
			Environment.SetEnvironmentVariable("Fabric_Endpoint_" + endpoitName, "80");
			string path = "MockPath";
			HttpMethod method = HttpMethod.Post;
			HttpStatusCode code = HttpStatusCode.HttpVersionNotSupported;
			string scheme = Uri.UriSchemeHttp;
			Func<HttpResponseMessage, HealthCheckResult, HealthCheckResult> additionalCheck = (r, h) => HealthCheckResult.Degraded();
			KeyValuePair<string, object>[] reportData = new Dictionary<string, object>
			{
				{ "testKey1", new object() },
				{ "testKey2", "value" }
			}.ToArray();

			IServiceProvider provider = GetBuilder()
				.AddHttpEndpointCheck(checkName, endpoitName, path, method, scheme, headers, code, additionalCheck, reportData)
				.Services
				.BuildServiceProvider();

			HttpHealthCheckParameters parameters = GetParameters(provider, checkName);

			Assert.AreEqual(code, parameters.ExpectedStatus, nameof(HttpHealthCheckParameters.ExpectedStatus));
			Assert.AreEqual(additionalCheck, parameters.AdditionalCheck, nameof(HttpHealthCheckParameters.AdditionalCheck));
			CollectionAssert.AreEquivalent(reportData, parameters.ReportData.ToArray(), nameof(HttpHealthCheckParameters.ReportData));
		}

		[DataTestMethod]
		[DataRow("https://localhost")]
		public void AddServiceFabricHealthChecks_InvalidPath_ThrowException(string path)
		{
			string endpoitName = "EndpointName";
			Environment.SetEnvironmentVariable("Fabric_Endpoint_" + endpoitName, "80");
			Assert.ThrowsException<ArgumentException>(() =>
				GetBuilder().AddHttpEndpointCheck("ChecKName", endpoitName, path, scheme: Uri.UriSchemeHttps));
		}

		[TestMethod]
		public void AddServiceFabricHealthChecks_HeaderKeyIsWhiteSpace_ThrowException()
		{
			string endpoitName = "EndpointName";
			Environment.SetEnvironmentVariable("Fabric_Endpoint_" + endpoitName, "80");
			Assert.ThrowsException<ArgumentException>(() =>
				GetBuilder().AddHttpEndpointCheck("ChecKName", endpoitName, "/", scheme: Uri.UriSchemeHttps,
					headers: new Dictionary<string, IEnumerable<string>>
					{
						{ string.Empty, new List<string> { "value" } }
					}));
		}

		private IHealthChecksBuilder GetBuilder() =>
			new ServiceCollection()
				.AddSingleton(new ActivitySource(nameof(HealthChecksBuilderExtensionsTests)))
				.AddSingleton(new Mock<IAccessor<IServicePartition>>().Object)
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

		private static IEnumerable<object?[]> GetHeaders()
		{
			yield return new object?[] {
				new Dictionary<string, IEnumerable<string>>
				{
					{ "testHeader", new List<string> { "value" } }
				}
			};
			yield return new object?[] {
				new Dictionary<string, IEnumerable<string>>
				{
					{ "testheader", new List<string>() }
				}
			};
			yield return new object?[] {
				new Dictionary<string, IEnumerable<string>>
				{
					{ "testheader", new List<string> { "value1", "value2" } },
					{ "testheader2", new List<string> { "value1", "value2" } }
				}
			};
		}
	}
}
