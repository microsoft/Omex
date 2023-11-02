// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	public class TagExtensionsTests
	{
		[DataTestMethod]
		[DataRow(0, "0000")]
		[DataRow(0xFFFE, "fffe")]
		[DataRow(0xFFFF, "ffff")]
		// following values provided to validate old behaviour since method ported as is
		[DataRow(0x3FFFFFFE, "?????")]
		[DataRow(0x3FFFFFFF, "?????")]
		[DataRow(0x40000000, "@\0\0\0")]
		[DataRow(int.MaxValue, "\u007f???")]
		public void TagIdAsString_ConvertsProperly(int tagId, string expected)
		{
# pragma warning disable 618
			Assert.AreEqual(expected, new EventId(tagId).ToTagId());
#pragma warning restore 618
		}
	}
}
