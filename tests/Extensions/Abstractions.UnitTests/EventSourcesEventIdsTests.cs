// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Abstractions.UnitTests
{
	[TestClass]
	public class EventSourcesEventIdsTests
	{
		[TestMethod]
		public void EventIdsUnique()
		{
			CollectionAssert.AllItemsAreUnique(
				Enum.GetValues(typeof(EventSourcesEventIds)),
				"All Event Ids should have unique int values");
		}
	}
}
