// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Xunit;
using Xunit.Abstractions;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.TimedScopes;

namespace Microsoft.Omex.System.UnitTests.TimedScopes
{
	/// <summary>
	/// Unit tests for verifying functionality of TimedScopeName class
	/// </summary>
	public class TimedScopeNameUnitTests : UnitTestBase
	{
		/// <summary>
		/// Test Scope
		/// </summary>
		private const string TestScope = "TestScope";


		/// <summary>
		/// Test Subtype
		/// </summary>
		private const string TestSubtype = "TestSubType";


		/// <summary>
		/// Test Metadata
		/// </summary>
		private const string TestMetadata = "TestMetadata";


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string OnlyScope = TestScope;


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeWithSubtype = TestScope + "/" + TestSubtype;


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeWithMetadata = TestScope + "/" + TestMetadata;


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeWithSubtypeAndMetadata = TestScope + "/" + TestSubtype + "/" + TestMetadata;


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeWithEmptySubtype = TestScope + "/";


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeWithEmptySubtypeAndMetadata = TestScope + "//" + TestMetadata;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="log">Log helper</param>
		public TimedScopeNameUnitTests(ITestOutputHelper log)
		{
		}


		[Fact]
		public void Verify_ValidScopeNames_Parsed()
		{
			TimedScopeName scopeName;

			if (VerifyTrueAndReturn(TimedScopeName.TryParse(OnlyScope, out scopeName)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(null, scopeName.SubType);
				Assert.Equal(null, scopeName.MetaData);
			}

			if (VerifyTrueAndReturn(TimedScopeName.TryParse(ScopeWithSubtype, out scopeName)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(TestSubtype, scopeName.SubType);
				Assert.Equal(null, scopeName.MetaData);
			}

			if (VerifyTrueAndReturn(TimedScopeName.TryParse(ScopeWithMetadata, out scopeName, preferMetaData: true)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(null, scopeName.SubType);
				Assert.Equal(TestMetadata, scopeName.MetaData);
			}

			if (VerifyTrueAndReturn(TimedScopeName.TryParse(ScopeWithSubtypeAndMetadata, out scopeName)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(TestSubtype, scopeName.SubType);
				Assert.Equal(TestMetadata, scopeName.MetaData);
			}

			if (VerifyTrueAndReturn(TimedScopeName.TryParse(ScopeWithEmptySubtype, out scopeName)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(string.Empty, scopeName.SubType);
				Assert.Equal(null, scopeName.MetaData);
			}

			if (VerifyTrueAndReturn(TimedScopeName.TryParse(ScopeWithEmptySubtypeAndMetadata, out scopeName)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(string.Empty, scopeName.SubType);
				Assert.Equal(TestMetadata, scopeName.MetaData);
			}
		}


		[Fact]
		public void Verify_MalformedScopeNames_NotParsed()
		{
			TimedScopeName scopeName;

			Assert.False(TimedScopeName.TryParse(null, out scopeName), "Malformed scope name should not be parsed");
			Assert.False(TimedScopeName.TryParse("", out scopeName), "Malformed scope name should not be parsed");
			Assert.False(TimedScopeName.TryParse("/x", out scopeName), "Malformed scope name should not be parsed");
			Assert.False(TimedScopeName.TryParse("/x/y", out scopeName), "Malformed scope name should not be parsed");
			Assert.False(TimedScopeName.TryParse("x/y/z/a", out scopeName), "Malformed scope name should not be parsed");
		}


		[Fact]
		public void Verify_HashCodesAndEquality()
		{
			CheckSingleScope(OnlyScope);
			CheckSingleScope(ScopeWithSubtype);
			CheckSingleScope(ScopeWithMetadata);
			CheckSingleScope(ScopeWithSubtypeAndMetadata);
			CheckSingleScope(ScopeWithEmptySubtype);
			CheckSingleScope(ScopeWithEmptySubtypeAndMetadata);
		}


		/// <summary>
		/// Verifies that parsing the same string (even with changed letter casing) gives equal TimedScopeName instances
		/// </summary>
		/// <param name="scopeName">The string to be parsed</param>
		private void CheckSingleScope(string scopeName)
		{
			TimedScopeName parsed1;
			TimedScopeName parsed2;
			if (VerifyTrueAndReturn(TimedScopeName.TryParse(scopeName, out parsed1)) &&
				VerifyTrueAndReturn(TimedScopeName.TryParse(scopeName, out parsed2)))
			{
				CheckHashCodesAndEquality(parsed1, parsed2);
			}
		}


		/// <summary>
		/// Check equality of two timed scope name instances by all possible ways
		/// </summary>
		/// <param name="x">First instance</param>
		/// <param name="y">Second instance</param>
		private void CheckHashCodesAndEquality(TimedScopeName x, TimedScopeName y)
		{
			Assert.Equal(x.GetHashCode(), y.GetHashCode());
			Assert.True(x.Equals(y));
			Assert.True(x.Equals((object)y));
			Assert.True(x == y);
			Assert.False(x != y);
		}


		[Fact]
		public void Verify_ParseAndToStringRoundTrip_EqualStringRepresentation()
		{
			CheckRoundTrip(OnlyScope);
			CheckRoundTrip(ScopeWithSubtype);
			CheckRoundTrip(ScopeWithMetadata);
			CheckRoundTrip(ScopeWithSubtypeAndMetadata);
			CheckRoundTrip(ScopeWithEmptySubtype);
			CheckRoundTrip(ScopeWithEmptySubtypeAndMetadata);
		}


		/// <summary>
		/// Checks that parsing a string and then converting back to string gives the same string
		/// </summary>
		/// <param name="scopeName">String to be parsed and converted back</param>
		private void CheckRoundTrip(string scopeName)
		{
			TimedScopeName parsed;
			if (VerifyTrueAndReturn(TimedScopeName.TryParse(scopeName, out parsed)))
			{
				Assert.Equal(parsed.ToString(), scopeName);
			}
		}

		/// <summary>
		/// Verifies that a condition is true and returns the value.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="message">The logged message.</param>
		/// <param name="args">Format string args.</param>
		/// <returns>The evaluation result.</returns>
		private bool VerifyTrueAndReturn(bool condition)
		{
			Assert.True(condition);
			return condition;
		}
	}
}