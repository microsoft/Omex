// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Activities.UnitTests
{
	[TestClass]
	public class NullableAssertTests
	{
		[TestMethod]
		public void IsNotNull_IfNotNull_NotThrows()
		{
			NullableAssert.IsNotNull(new object());
		}

		[TestMethod]
		public void IsNotNull_IfNull_Throws()
		{
			Assert.ThrowsExactly<AssertFailedException>(() => NullableAssert.IsNotNull(null));
		}

		[TestMethod]
		public void IsNotNull_PropagatesErrorMessage()
		{
			AssertFailedException exception = Assert.ThrowsExactly<AssertFailedException>(
				() => NullableAssert.IsNotNull(null, s_message, s_parameters));

			StringAssert.Contains(exception.Message, s_expectedMessage);
		}

		[TestMethod]
		public void IsTrue_IfTrue_NotThrows()
		{
			NullableAssert.IsTrue(true);
		}

		[TestMethod]
		public void IsTrue_IfFalse_Throws()
		{
			Assert.ThrowsExactly<AssertFailedException>(() => NullableAssert.IsTrue(false));
		}

		[TestMethod]
		public void IsTrue_PropagatesErrorMessage()
		{
			AssertFailedException exception = Assert.ThrowsExactly<AssertFailedException>(
				() => NullableAssert.IsTrue(false, s_message, s_parameters));

			StringAssert.Contains(exception.Message, s_expectedMessage);
		}

		[TestMethod]
		public void IsFalse_IfFalse_NotThrows()
		{
			NullableAssert.IsFalse(false);
		}

		[TestMethod]
		public void IsFalse_IfTrue_Throws()
		{
			Assert.ThrowsExactly<AssertFailedException>(() => NullableAssert.IsFalse(true));
		}

		[TestMethod]
		public void IsFalse_PropagatesErrorMessage()
		{
			AssertFailedException exception = Assert.ThrowsExactly<AssertFailedException>(
				() => NullableAssert.IsFalse(true, s_message, s_parameters));

			StringAssert.Contains(exception.Message, s_expectedMessage);
		}


		[TestMethod]
		public void Fail_Throws()
		{
			Assert.ThrowsExactly<AssertFailedException>(() => NullableAssert.Fail());
		}

		[TestMethod]
		public void Fail_PropagatesErrorMessage()
		{
			AssertFailedException exception = Assert.ThrowsExactly<AssertFailedException>(
				() => NullableAssert.Fail(s_message, s_parameters));

			StringAssert.Contains(exception.Message, s_expectedMessage);
		}

		private static readonly string s_message = "Some message {0} {1} {2}";
		private static readonly object[] s_parameters = new object[] { 1, DayOfWeek.Tuesday, "A" };
		private static readonly string s_expectedMessage = string.Format(CultureInfo.InvariantCulture, s_message, s_parameters);

		/// <summary>
		/// Code in this class not intended to be executed, it validates compiler understanding of nullability attributes
		/// </summary>
		private class NullableAttributesCheck
		{
			public void CheckIsNotNull(object? test)
			{
				NullableAssert.IsNotNull(test);
				test.GetHashCode(); // should allow calling method on nullable variable without check
			}

			public void CheckIsFalse(object? test)
			{
				NullableAssert.IsFalse(test == null);
				test.GetHashCode(); // should allow calling method on nullable variable without check
			}

			public void CheckIsTrue(object? test)
			{
				NullableAssert.IsTrue(test != null);
				test.GetHashCode(); // should allow calling method on nullable variable without check
			}

			public void CheckFail(object? test)
			{
				if (test == null)
				{
					NullableAssert.Fail();
				}

				test.GetHashCode(); // should allow calling method on nullable variable without check
			}
		}
	}
}

