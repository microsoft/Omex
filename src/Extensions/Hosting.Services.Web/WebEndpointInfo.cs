// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Globalization;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	/// <summary>
	/// Endpoint description for sf web listener
	/// </summary>
	public class WebEndpointInfo
	{
		/// <summary>
		/// Endpoint name
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Setting name that will contain common name of the certificate for https
		/// </summary>
		public string? SettingForCertificateCommonName { get; }

		/// <summary>
		/// Endpoint port
		/// </summary>
		public int Port { get; }

		/// <summary>
		/// Use https instead of http
		/// </summary>
		public bool UseHttps { get; }

		private WebEndpointInfo(string endpointName, string? settingForCertificateCommonName)
		{
			Name = Validation.ThrowIfNullOrWhiteSpace(endpointName);
			SettingForCertificateCommonName = settingForCertificateCommonName;
			UseHttps = settingForCertificateCommonName != null;
			Port = SfConfigurationProvider.GetEndpointPort(endpointName);
		}

		internal string GetListenerUrl() =>
			string.Format(CultureInfo.InvariantCulture, "{0}://+:{1}", UseHttps ? "https" : "http", Port);

		/// <summary>
		/// Create http endpoint information
		/// </summary>
		/// <param name="endpointName">Endpoint name from ServiceManifest.xml</param>
		public static WebEndpointInfo CreateHttp(string endpointName) =>
			new WebEndpointInfo(endpointName, null);

		/// <summary>
		/// Create https endpoint information
		/// </summary>
		/// <param name="endpointName">Endpoint name from ServiceManifest.xml</param>
		/// <param name="settingForCertificateCommonName">Name of the setting to get certificate common name for https, default value is 'Certificates:SslCertificateCommonName'</param>
		public static WebEndpointInfo CreateHttps(string endpointName, string settingForCertificateCommonName = "Certificates:SslCertificateCommonName") =>
			new WebEndpointInfo(endpointName, settingForCertificateCommonName);
	}
}
