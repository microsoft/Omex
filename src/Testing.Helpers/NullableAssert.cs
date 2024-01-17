// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Testing.Helpers
{
	/// <summary>
	/// Provider asserts checks with appropriate nullability attributes
	/// </summary>
	public static class NullableAssert
	{
		/// <summary>
		/// Tests whether the specified condition is true and throws an exception if the condition is false.
		/// </summary>
		public static void IsTrue([DoesNotReturnIf(false)] bool value, string message = "", params object[] parameters) =>
			Assert.IsTrue(value, message, parameters);

		/// <summary>
		/// Tests whether the specified condition is true and throws an exception if the condition is true.
		/// </summary>
		public static void IsFalse([DoesNotReturnIf(true)] bool value, string message = "", params object[] parameters) =>
			Assert.IsFalse(value, message, parameters);

		/// <summary>
		/// Throws an AssertFailedException
		/// </summary>
		[DoesNotReturn]
		public static void Fail(string message = "", params object[] parameters) =>
#pragma warning disable CS8763 // Assert.Fail will throw exception so method will never return
			Assert.Fail(message, parameters);
#pragma warning restore CS8763
	}
}

#if NETSTANDARD
namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>
	/// NotNull attribute stub for netstandard2.0
	/// </summary>
	public class NotNullAttribute : Attribute { }

	/// <summary>
	/// DoesNotReturn attribute stub for netstandard2.0
	/// </summary>
	public class DoesNotReturnAttribute : Attribute { }

	/// <summary>
	/// DoesNotReturn attribute stub for netstandard2.0
	/// </summary>
	public class DoesNotReturnIfAttribute : Attribute
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public DoesNotReturnIfAttribute(bool parameterValue) { }
	}
}
#endif
