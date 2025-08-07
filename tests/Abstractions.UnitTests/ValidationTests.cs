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
		[DataRow("apothem")]
		[DataRow(" (⊙_☉)  ")]
		[DataRow("-")]
		public void ThrowIfNullOrWhiteSpace_WhenValueValid_ReturnsIt(string expected)
			=> Assert.AreEqual(expected, Validation.ThrowIfNullOrWhiteSpace(expected));

		[TestMethod]
		[DataRow("")]
		[DataRow("     ")]
		[DataRow("\t\r\n")]
		public void ThrowIfNullOrWhiteSpace_WhenValueEmptyOrWhiteSpace_ThrowsException(string value)
			=> Assert.ThrowsExactly<ArgumentException>(() => Validation.ThrowIfNullOrWhiteSpace(value));

		[TestMethod]
		public void ThrowIfNullOrWhiteSpace_WhenValueNull_ThrowsException()
			=> Assert.ThrowsExactly<ArgumentNullException>(() => Validation.ThrowIfNullOrWhiteSpace(null));
	}
}
