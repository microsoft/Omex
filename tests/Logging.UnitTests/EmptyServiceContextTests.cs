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
		public void Constructor_SetsDefaultValues()
		{
			EmptyServiceContext emptyContext = new EmptyServiceContext();
			Assert.AreEqual(Guid.Empty, emptyContext.PartitionId);
			Assert.AreEqual(0, emptyContext.ReplicaOrInstanceId);
		}
	}
}
