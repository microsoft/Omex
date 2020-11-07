// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	internal class WebListenerConfiguration
	{
		private const string PartitionIdEvnVariableName = "Fabric_PartitionId";

		private const string PublishAddressEvnVariableName = "Fabric_NodeIPOrFQDN";

		private const string EndpointPortEvnVariableSuffix = "Fabric_Endpoint_";

		public string EndpointName { get; }

		public ServiceFabricIntegrationOptions IntegrationOptions { get; }

		public bool UseHttps { get; }

		public string? CertificateCommonNameForHttps { get; }

		public string PartitionId { get; }

		public string PublishAddress { get; }

		public int EndpointPort { get; }

		public string UrlSuffix { get; }

		public string ListenerUrl { get; }

		public WebListenerConfiguration(string endpointName, ServiceFabricIntegrationOptions options, string? certificateCommonNameForHttps)
		{
			EndpointName = endpointName;

			IntegrationOptions = options;

			UseHttps = certificateCommonNameForHttps != null;

			CertificateCommonNameForHttps = certificateCommonNameForHttps;

			PartitionId = GetSfVariable(PartitionIdEvnVariableName) ?? throw CreateException(nameof(PartitionId));

			PublishAddress = GetSfVariable(PublishAddressEvnVariableName) ?? throw CreateException(nameof(PublishAddress));

			EndpointPort = int.TryParse(GetSfVariable(EndpointPortEvnVariableSuffix + endpointName), out int parseResult)
				? parseResult
				: throw CreateException(nameof(EndpointPort));

			UrlSuffix = GetUrlSuffix(IntegrationOptions, PartitionId);

			ListenerUrl = GetListenerUrl(UseHttps, EndpointPort);
		}

		private static string? GetSfVariable(string name) =>
			Environment.GetEnvironmentVariable(name); // Environment variables should be provided by SF

		private static Exception CreateException(string value) =>
			new ArgumentException($"Failed to get {value} from environment variables");

		private static string GetListenerUrl(bool useHttps, int port) =>
			string.Format(CultureInfo.InvariantCulture, "{0}://+:{1}", useHttps ? "https" : "http", port);

		private static string GetUrlSuffix(ServiceFabricIntegrationOptions options, string partitionId) =>
			options.HasFlag(ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
				? CreateUniqueServiceUrl(partitionId)
				: string.Empty;

		private static string CreateUniqueServiceUrl(string partitionId) =>
			string.Format(CultureInfo.InvariantCulture, "/{0}/{1}",
				partitionId,
				Guid.NewGuid()); // By default SF use PartitionId/InstanceId but since we not have instance id here random guid is used
	}
}
