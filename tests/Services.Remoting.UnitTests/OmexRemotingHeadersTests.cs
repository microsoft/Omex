// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Omex.Extensions.Services.Remoting;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Services.Remoting
{
	[TestClass]
	public class OmexRemotingHeadersTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			Activity.DefaultIdFormat = ActivityIdFormat.W3C;
		}

		[TestMethod]
		public void OmexRemotingHeaders_AttachActivityToOutgoingRequest_HandlesNullActivityProperly()
		{
			Mock<IServiceRemotingRequestMessage> requestMock = new Mock<IServiceRemotingRequestMessage>();
			requestMock.Object.AttachActivityToOutgoingRequest(null);
		}

		[TestMethod]
		public void OmexRemotingHeaders_StartActivityFromIncomingRequestWhenListenerDisabled_ReturnsNull()
		{
			Mock<IServiceRemotingRequestMessage> requestMock = new Mock<IServiceRemotingRequestMessage>();
			DiagnosticListener disabledListener = new DiagnosticListener("DisabledListener");

			Assert.IsNull(requestMock.Object.StartActivityFromIncomingRequest(disabledListener, "SomeName"));
		}

		[TestMethod]
		public void OmexRemotingHeaders_WithoutBaggage_ProperlyTransferred()
		{
			Activity.DefaultIdFormat = ActivityIdFormat.W3C;
			Activity outgoingActivity = new Activity(nameof(OmexRemotingHeaders_WithoutBaggage_ProperlyTransferred));

			TestActivityTransfer(outgoingActivity);
		}

		[TestMethod]
		public void OmexRemotingHeaders_WithBaggage_ProperlyTransferred()
		{
			Activity.DefaultIdFormat = ActivityIdFormat.W3C;
			Activity outgoingActivity = new Activity(nameof(OmexRemotingHeaders_WithBaggage_ProperlyTransferred))
				.AddBaggage("TestValue", "Value @+&")
				.AddBaggage("QuotesValue", "value with \"quotes\" inside \" test ")
				.AddBaggage("UnicodeValue", "☕☘☔ (┛ಠ_ಠ)┛彡┻━┻");

			TestActivityTransfer(outgoingActivity);
		}

		[TestMethod]
		public void OmexRemotingHeaders_WithBaggage_KeepsOrder()
		{
			string key = "Key1";

			Activity.DefaultIdFormat = ActivityIdFormat.W3C;
			Activity outgoingActivity = new Activity(nameof(OmexRemotingHeaders_WithBaggage_ProperlyTransferred))
				.AddBaggage(key, "value1")
				.AddBaggage(key, "value2");

			TestActivityTransfer(outgoingActivity);
		}

		private void TestActivityTransfer(Activity outgoingActivity)
		{
			HeaderMock header = new HeaderMock();
			Mock<IServiceRemotingRequestMessage> requestMock = new Mock<IServiceRemotingRequestMessage>();
			requestMock.Setup(m => m.GetHeader()).Returns(header);

			outgoingActivity.Start();
			requestMock.Object.AttachActivityToOutgoingRequest(outgoingActivity);
			outgoingActivity.Stop();

			using DiagnosticListener listener = CreateActiveListener("TestListeners");
			Activity? incomingActivity = requestMock.Object.StartActivityFromIncomingRequest(listener, outgoingActivity.OperationName + "_Out");
			NullableAssert.IsNotNull(incomingActivity);
			incomingActivity.Stop();

			Assert.AreEqual(outgoingActivity.Id, incomingActivity.ParentId);
			CollectionAssert.AreEqual(outgoingActivity.Baggage.ToArray(), incomingActivity.Baggage.ToArray());
		}

		private DiagnosticListener CreateActiveListener(string name)
		{
			DiagnosticListener listener = new DiagnosticListener(name);
			listener.Subscribe(new TestDiagnosticsObserversInitializer());
			return listener;
		}

		private class TestDiagnosticsObserversInitializer : IObserver<KeyValuePair<string, object?>>
		{
			public void OnCompleted() { }

			public void OnError(Exception error) { }

			public void OnNext(KeyValuePair<string, object?> value) { }
		}

		public class HeaderMock : IServiceRemotingRequestMessageHeader
		{
			public int MethodId { get; set; }

			public int InterfaceId { get; set; }

			public Guid RequestId { get; set; }

			public string? InvocationId { get; set; }

			public string? MethodName { get; set; }

			private readonly Dictionary<string, byte[]> m_headers = new Dictionary<string, byte[]>();

			public void AddHeader(string headerName, byte[] headerValue) => m_headers.Add(headerName, headerValue);

			public bool TryGetHeaderValue(string headerName, out byte[]? headerValue) => m_headers.TryGetValue(headerName, out headerValue);
		}
	}
}
