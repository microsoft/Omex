// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Omex.Extensions.Hosting.Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.UnitTests
{
	[TestClass]
	public class CertificateReaderTests
	{
		[TestMethod]
		public void GetCertificatesByCommonName_ReturnsCerts()
		{
			string commonName = "testName";
			StoreName storeName = StoreName.TrustedPeople;
			X509Certificate2[] expected = new[]
			{
				CreateCert(commonName),
				CreateCert(commonName)
			};

			(ICertificateReader reader, Action<Times, string> verify) = CreateReader(
					storeName,
					CreateCert("nontestName"),
					expected[0],
					CreateCert("dummyCert"),
					expected[1],
					CreateCert("anotherDummyCert")
				);

			X509Certificate2[] actualFirstCall = reader.GetCertificatesByCommonName(commonName, storeName: storeName).ToArray();
			CollectionAssert.AreEquivalent(expected, actualFirstCall, "Fist call to GetCertificatesByCommonName returned wrong data");
			verify(Times.Once(), "Certificate store should be called once to get certificates");

			X509Certificate2[] actualSecondCall = reader.GetCertificatesByCommonName(commonName, storeName: storeName).ToArray();
			CollectionAssert.AreEquivalent(expected, actualFirstCall, "Second call to GetCertificatesByCommonName returned wrong data");
			verify(Times.Never(), "Certificate store should not be called when certificates taken from cache");

			X509Certificate2[] actualNoCacheCall = reader.GetCertificatesByCommonName(commonName, refreshCache: true, storeName: storeName).ToArray();
			CollectionAssert.AreEquivalent(expected, actualFirstCall, "Call of GetCertificatesByCommonName (without cache) returned wrong data");
			verify(Times.Once(), "Certificate store should when cache not used");

			bool hasAnyCerts = reader.GetCertificatesByCommonName("missingName", storeName: storeName).Any();
			Assert.IsFalse(hasAnyCerts, "GetCertificatesByCommonName should return empty sequence when certificate not found");
		}

		[TestMethod]
		public void GetCertificatesByCommonName_WhenCertificatesNotFound_ReturnEmptySequence()
		{
			(ICertificateReader reader, _) = CreateReader();

			bool hasAnyCerts = reader.GetCertificatesByCommonName("SomeName").Any();
			Assert.IsFalse(hasAnyCerts, "GetCertificatesByCommonName should return empty sequence when there are no certificates");
		}

		[TestMethod]
		[Obsolete("To test method that marked as obsolete")]
		public void GetCertificateByThumbprint_ReturnsCerts()
		{
			StoreName storeName = StoreName.CertificateAuthority;
			X509Certificate2 expected = CreateCert("someName");

			(ICertificateReader reader, Action<Times, string> verify) = CreateReader(
					storeName,
					CreateCert("dummyCert"),
					expected,
					CreateCert("anotherDummyCert")
				);

			X509Certificate2? actual = reader.GetCertificateByThumbprint(expected.Thumbprint, storeName: storeName);

			X509Certificate2? actualFirstCall = reader.GetCertificateByThumbprint(expected.Thumbprint, storeName: storeName);
			Assert.AreEqual(expected, actualFirstCall, "Fist call to GetCertificateByThumbprint returned wrong data");
			verify(Times.Once(), "Certificate store should be called once to get certificates");

			X509Certificate2? actualSecondCall = reader.GetCertificateByThumbprint(expected.Thumbprint, storeName: storeName);
			Assert.AreEqual(expected, actualFirstCall, "Second call to GetCertificateByThumbprint returned wrong data");
			verify(Times.Never(), "Certificate store should not be called when certificates taken from cache");

			X509Certificate2? actualNoCacheCall = reader.GetCertificateByThumbprint(expected.Thumbprint, refreshCache: true, storeName: storeName);
			Assert.AreEqual(expected, actualFirstCall, "Call of GetCertificateByThumbprint (without cache) returned wrong data");
			verify(Times.Once(), "Certificate store should when cache not used");

			X509Certificate2? missingCert = reader.GetCertificateByThumbprint(expected.Thumbprint + "9", storeName: storeName);
			Assert.IsNull(missingCert, "GetCertificateByThumbprint should return null when certificate not found");
		}

		[TestMethod]
		[Obsolete("To test method that marked as obsolete")]
		public void GetCertificateByThumbprint_WhenCertificatesNotFound_ReturnEmptySequence()
		{
			X509Certificate2 missingCert = CreateCert("missingName");
			(ICertificateReader reader, _) = CreateReader();

			X509Certificate2? result = reader.GetCertificateByThumbprint(missingCert.Thumbprint);
			Assert.IsNull(result, "GetCertificateByThumbprint should return null when there are no certificates");
		}

		private static X509Certificate2 CreateCert(string commonName, DateTimeOffset? notBefore = null, DateTimeOffset? notAfter = null) =>
			new CertificateRequest($"cn={commonName}", ECDsa.Create(), HashAlgorithmName.SHA256)
				.CreateSelfSigned(
					notBefore.GetValueOrDefault(DateTimeOffset.Now.Date.AddYears(-1)),
					notAfter.GetValueOrDefault(DateTimeOffset.Now.Date.AddYears(1)));

		private static (ICertificateReader reader, Action<Times, string> verify) CreateReader(
			StoreName storeName = StoreName.My,
			params X509Certificate2[] certificates)
		{
			Expression<Func<ICertificateStore, IEnumerable<CertificateInformation>>> expression = store =>
				store.GetAllCertificates(storeName, StoreLocation.LocalMachine);

			Mock<ICertificateStore> storeMock = new Mock<ICertificateStore>();

			storeMock
				.Setup(expression)
				.Returns(certificates.Select(cert => new CertificateInformation(cert)));

			return (new CertificateReader(storeMock.Object, new NullLogger<CertificateReader>()),
				(times, message) =>
				{
					storeMock.Verify(expression, times, message);
					storeMock.Invocations.Clear();
				});
		}
	}
}
