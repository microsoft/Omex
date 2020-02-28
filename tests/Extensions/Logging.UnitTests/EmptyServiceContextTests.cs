// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class EmptyServiceContextTests
	{
		[TestMethod]
		public void EmptyServiceContextSetsDefaultValues()
		{
			EmptyServiceContext emptyContext = new EmptyServiceContext();
			Assert.AreEqual(Guid.Empty, emptyContext.GetPartitionId());
			Assert.AreEqual(0, emptyContext.GetReplicaOrInstanceId());
		}
	}
}
