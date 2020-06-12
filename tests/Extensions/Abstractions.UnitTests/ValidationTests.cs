// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Abstractions.UnitTests
{
	[TestClass]
	public class ValidationTests
	{
		[TestMethod]
		public void ThrowIfNull_WhenValueValidRefType_ReturnsIt()
		{
			object expected = new object();
			object actual = Validation.ThrowIfNull(expected);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ThrowIfNull_WhenValueValidStruct_ReturnsIt()
		{
			int? expected = 1;
			int actual = Validation.ThrowIfNull(expected);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ThrowIfNull_WhenValueNull_ThrowsException()
			=> Assert.ThrowsException<ArgumentNullException>(() => Validation.ThrowIfNull((object?)null));

		[TestMethod]
		public void ThrowIfNull_WhenValueNullableStruct_ThrowsException()
			=> Assert.ThrowsException<ArgumentNullException>(() => Validation.ThrowIfNull((int?)null));

		[DataTestMethod]
		[DataRow("apothem")]
		[DataRow(" (⊙_☉)  ")]
		[DataRow("-")]
		public void ThrowIfNullOrWhiteSpace_WhenValueValid_ReturnsIt(string expected)
			=> Assert.AreEqual(expected, Validation.ThrowIfNullOrWhiteSpace(expected));

		[DataTestMethod]
		[DataRow("")]
		[DataRow("     ")]
		[DataRow("\t\r\n")]
		public void ThrowIfNullOrWhiteSpace_WhenValueEmptyOrWhiteSpace_ThrowsException(string value)
			=> Assert.ThrowsException<ArgumentException>(() => Validation.ThrowIfNullOrWhiteSpace(value));

		[TestMethod]
		public void ThrowIfNullOrWhiteSpace_WhenValueNull_ThrowsException()
			=> Assert.ThrowsException<ArgumentNullException>(() => Validation.ThrowIfNullOrWhiteSpace(null));
	}
}
