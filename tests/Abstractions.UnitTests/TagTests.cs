// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Abstractions.UnitTests
{
	[TestClass]
	public class TagTests
	{
		[TestMethod]
		public void Create_HasProperValues()
		{
			EventId tag1 = Tag.Create();
			// empty line to check that Tag.Create() use line number for Id value and not just incriment
			EventId tag2 = Tag.Create();

			Assert.AreEqual(tag1.Id + 2, tag2.Id, "EventId Id should correspond to line number");

			Assert.AreEqual(tag1.Name, tag2.Name, "EventId Name should be the same in the same file");


			string relativePath = "/tests/Abstractions.UnitTests/" + nameof(TagTests) + ".cs"; //This value should be change in case of changing file path or name
			Assert.AreEqual(tag1.Name, relativePath, "tag1 Name should point to current file");
			Assert.AreEqual(tag2.Name, relativePath, "tag2 Name should point to current file");
		}

		[DataTestMethod]
		[DataRow(-19)]
		[DataRow(0)]
		[DataRow(13)]
		[DataRow(int.MaxValue)]
		public void ReserveTag_PreserveTagValue(int tagId)
		{
			EventId tag = Tag.ReserveTag(tagId);
			Assert.AreEqual(tagId, tag.Id);
		}
	}
}
