// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Abstractions.UnitTests
{
	[TestClass]
	public class EventSourcesEventIdsTests
	{
		[TestMethod]
		public void EventIdsUnique()
		{
			int[] values = Enum.GetValues(typeof(EventSourcesEventIds)).Cast<int>().ToArray();
			CollectionAssert.AllItemsAreUnique(values, "All Event Ids should have unique int values");
		}
	}
}
