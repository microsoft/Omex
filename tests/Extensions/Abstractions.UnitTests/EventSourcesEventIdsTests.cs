using System;
using System.Linq;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Abstractions.UnitTests
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
