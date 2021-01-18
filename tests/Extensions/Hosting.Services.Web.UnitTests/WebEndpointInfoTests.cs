// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.UnitTests.Internal
{
	[TestClass]
	public class WebEndpointInfoTests
	{
		[TestMethod]
		public void CreateHttp_SetsProperties()
		{
			string endpointName = "TestHttpEndpointName";
			int port = 8081;

			SfConfigurationProviderHelper.SetPortVariable(endpointName, port);

			WebEndpointInfo info = WebEndpointInfo.CreateHttp(endpointName);
			Assert.AreEqual(endpointName, info.Name);
			Assert.IsNull(info.SettingForCertificateCommonName);
			Assert.IsFalse(info.UseHttps);
			Assert.AreEqual(port, info.Port);
			Assert.AreEqual($"http://+:{port}", info.GetListenerUrl());
		}

		[TestMethod]
		public void CreateHttps_SetsProperties()
		{
			string endpointName = "TestHttpsEndpointName";
			string settingForCertificateCommonName = "SomeCertName";
			int port = 8083;

			SfConfigurationProviderHelper.SetPortVariable(endpointName, port);

			WebEndpointInfo info = WebEndpointInfo.CreateHttps(endpointName, settingForCertificateCommonName);
			Assert.AreEqual(endpointName, info.Name);
			Assert.AreEqual(settingForCertificateCommonName, info.SettingForCertificateCommonName);
			Assert.IsTrue(info.UseHttps);
			Assert.AreEqual(port, info.Port);
			Assert.AreEqual($"https://+:{port}", info.GetListenerUrl());
		}

		[TestMethod]
		public void CreateHttp_WithEmptyEndpointName_Throws() => Assert.ThrowsException<ArgumentException>(() => WebEndpointInfo.CreateHttp(string.Empty));

		[TestMethod]
		public void CreateHttps_WithEmptyEndpointName_Throws() => Assert.ThrowsException<ArgumentException>(() => WebEndpointInfo.CreateHttps(string.Empty));

		[TestMethod]
		public void CreateHttp_WithWrongEndpointName_Throws() => Assert.ThrowsException<SfConfigurationException>(() => WebEndpointInfo.CreateHttp("UnexistingEndpoint"));

		[TestMethod]
		public void CreateHttps_WithoutCertSettings_Throws()
		{
			string someEndpoint = "SomeTestEndpoint";
			SfConfigurationProviderHelper.SetPortVariable(someEndpoint, 8080);
			Assert.ThrowsException<ArgumentException>(() => WebEndpointInfo.CreateHttps(someEndpoint, string.Empty));
		}
	}
}
