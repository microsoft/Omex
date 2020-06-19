﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Services.Remoting
{
	[TestClass]
	public class OmexRemotingHeadersTests
	{
		[TestMethod]
		public void OmexRemotingHeaders_AttachActivityToOutgoingRequest_HandlesNullActivityProperly()
		{
			Mock<IServiceRemotingRequestMessage> requestMock = new Mock<IServiceRemotingRequestMessage>();
			requestMock.Object.AttachActivityToOutgoingRequest(null);
		}

		[TestMethod]
		public void OmexRemotingHeaders_ExtractActivityFromIncomingRequest_HandlesNullActivityProperly()
		{
			Mock<IServiceRemotingRequestMessage> requestMock = new Mock<IServiceRemotingRequestMessage>();
			requestMock.Object.ExtractActivityFromIncomingRequest(null);
		}

		[TestMethod]
		public void OmexRemotingHeaders_WithoutBaggage_ProperlyTransferred()
		{
			Activity outgoingActivity = new Activity(nameof(OmexRemotingHeaders_WithoutBaggage_ProperlyTransferred));

			TestActivityTransfer(outgoingActivity);
		}

		[TestMethod]
		public void OmexRemotingHeaders_WithBaggage_ProperlyTransferred()
		{
			Activity outgoingActivity = new Activity(nameof(OmexRemotingHeaders_WithBaggage_ProperlyTransferred))
				.AddBaggage("NullValue", null)
				.AddBaggage("EmptyValue", string.Empty)
				.AddBaggage("TestValue", "Value @+&")
				.AddBaggage("QuotesValue", "value with \"quotes\" inside \" test ")
				.AddBaggage("UnicodeValue", "☕☘☔ (┛ಠ_ಠ)┛彡┻━┻");

			TestActivityTransfer(outgoingActivity);
		}

		private void TestActivityTransfer(Activity outgoingActivity)
		{
			// run in separate thread to avoid interference from other activities
			Task.Run(() =>
			{
				HeaderMock header = new HeaderMock();
				Mock<IServiceRemotingRequestMessage> requestMock = new Mock<IServiceRemotingRequestMessage>();
				requestMock.Setup(m => m.GetHeader()).Returns(header);

				outgoingActivity.Start();
				requestMock.Object.AttachActivityToOutgoingRequest(outgoingActivity);
				outgoingActivity.Stop();

				Activity incomingActivity = new Activity(outgoingActivity.OperationName + "_Out").Start();
				requestMock.Object.ExtractActivityFromIncomingRequest(incomingActivity);
				incomingActivity.Stop();

				Assert.AreEqual(outgoingActivity.Id, incomingActivity.ParentId);
				CollectionAssert.AreEquivalent(outgoingActivity.Baggage.ToArray(), incomingActivity.Baggage.ToArray());
			});
		}

		public class HeaderMock : IServiceRemotingRequestMessageHeader
		{
			public int MethodId { get; set; }

			public int InterfaceId { get; set; }

			public string? InvocationId { get; set; }

			public string? MethodName { get; set; }

			private readonly Dictionary<string, byte[]> m_headers = new Dictionary<string, byte[]>();

			public void AddHeader(string headerName, byte[] headerValue) => m_headers.Add(headerName, headerValue);

			public bool TryGetHeaderValue(string headerName, out byte[]? headerValue) => m_headers.TryGetValue(headerName, out headerValue);
		}
	}
}
