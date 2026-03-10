// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.UnitTests;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class HealthChecksBuilderExtensionsTests
	{
		[TestMethod]
		[DynamicData(nameof(GetHeaders))]
		[Obsolete("The health check implementation based on AbstractHealthCheck is obsolete.")]
		public void AddServiceFabricHealthChecks_RegisterPublisherAndChecks(IReadOnlyDictionary<string, IEnumerable<string>> headers)
		{
			string checkName = "MockHttpCheck";
			string endpoitName = "MockEndpoitName";
			SfConfigurationProviderHelper.SetPortVariable(endpoitName, 80);
			string path = "MockPath";
			HttpMethod method = HttpMethod.Post;
			HttpStatusCode code = HttpStatusCode.HttpVersionNotSupported;
			string scheme = Uri.UriSchemeHttp;
			Func<HttpResponseMessage, HealthCheckResult, Task<HealthCheckResult>> additionalCheck = (r, h) =>
				Task.FromResult(HealthCheckResult.Degraded());
			KeyValuePair<string, object>[] reportData = new Dictionary<string, object>
			{
				{ "testKey1", new object() },
				{ "testKey2", "value" }
			}.ToArray();

			IServiceProvider provider = GetBuilder()
				.AddHttpEndpointCheck(checkName, endpoitName, path, method, scheme, headers, code, HealthStatus.Unhealthy, additionalCheck, reportData)
				.Services
				.BuildServiceProvider();

			HttpHealthCheckParameters parameters = GetParameters(provider, checkName);

			Assert.AreEqual(parameters.ExpectedStatus, code, nameof(HttpHealthCheckParameters.ExpectedStatus));
			Assert.AreEqual(additionalCheck, parameters.AdditionalCheck, nameof(HttpHealthCheckParameters.AdditionalCheck));
			CollectionAssert.AreEquivalent(reportData, parameters.ReportData.ToArray(), nameof(HttpHealthCheckParameters.ReportData));
		}

		[TestMethod]
		[DataRow("https://localhost")]
		[Obsolete("The health check implementation based on AbstractHealthCheck is obsolete.")]
		public void AddServiceFabricHealthChecks_InvalidPath_ThrowException(string path)
		{
			string endpoitName = "EndpointName";
			SfConfigurationProviderHelper.SetPortVariable(endpoitName, 80);
			Assert.ThrowsExactly<ArgumentException>(() =>
				GetBuilder().AddHttpEndpointCheck("ChecKName", endpoitName, path, scheme: Uri.UriSchemeHttps));
		}

		[TestMethod]
		[Obsolete("The health check implementation based on AbstractHealthCheck is obsolete.")]
		public void AddServiceFabricHealthChecks_HeaderKeyIsWhiteSpace_ThrowException()
		{
			string endpoitName = "EndpointName";
			SfConfigurationProviderHelper.SetPortVariable(endpoitName, 80);
			Assert.ThrowsExactly<ArgumentException>(() =>
				GetBuilder().AddHttpEndpointCheck("ChecKName", endpoitName, "/", scheme: Uri.UriSchemeHttps,
					headers: new Dictionary<string, IEnumerable<string>>
					{
						{ string.Empty, new List<string> { "value" } }
					}));
		}

		[Obsolete("The health check implementation based on AbstractHealthCheck is obsolete.")]
		private IHealthChecksBuilder GetBuilder() =>
			new ServiceCollection()
				.AddSingleton(new ActivitySource(nameof(HealthChecksBuilderExtensionsTests)))
				.AddSingleton(new Mock<IAccessor<IServicePartition>>().Object)
				.AddServiceFabricHealthChecks();

		[Obsolete("The health check implementation based on AbstractHealthCheck is obsolete.")]
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
