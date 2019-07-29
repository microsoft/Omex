// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Xunit;
using Xunit.Abstractions;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.TimedScopes;

namespace Microsoft.Omex.System.UnitTests.TimedScopes
{
	/// <summary>
	/// Unit tests for verifying functionality of TimedScopeInstanceName class
	/// </summary>
	[Trait("TestCategory", "SharedInfrastructure")]
	public class TimedScopeInstanceNameUnitTests : UnitTestBase
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
		/// Test Classification
		/// </summary>
		private const TimedScopeResult TestClassification = TimedScopeResult.SystemError;


		/// <summary>
		/// Test Scope with classification suffix
		/// </summary>
		private const string TestScopeWithClassification = TestScope + ".SystemError";


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string OnlyScope = TestScopeWithClassification;


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeWithSubtype = TestScopeWithClassification + "/" + TestSubtype;


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeWithMetadata = TestScopeWithClassification + "/" + TestMetadata;


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeWithSubtypeAndMetadata = TestScopeWithClassification + "/" + TestSubtype + "/" + TestMetadata;


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeWithEmptySubtype = TestScopeWithClassification + "/";


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeWithEmptySubtypeAndMetadata = TestScopeWithClassification + "//" + TestMetadata;


		/// <summary>
		/// Valid scope name
		/// </summary>
		private const string ScopeNameWithDescription = TestScopeWithClassification + ".Description/" + TestSubtype + "/" + TestMetadata;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="log">Log helper</param>
		public TimedScopeInstanceNameUnitTests(ITestOutputHelper log)
		{
		}


		[Fact]
		public void Verify_ValidScopeInstanceNames_Parsed()
		{
			TimedScopeInstanceName scopeName;

			if (VerifyTrueAndReturn(TimedScopeInstanceName.TryParse(OnlyScope, out scopeName)))
			{
				Assert.Equal(scopeName.Scope, TestScope);
				Assert.Equal(null, scopeName.SubType);
				Assert.Equal(null, scopeName.MetaData);
				Assert.Equal(TestClassification, scopeName.Classification);
			}

			if (VerifyTrueAndReturn(TimedScopeInstanceName.TryParse(ScopeWithSubtype, out scopeName)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(TestSubtype, scopeName.SubType);
				Assert.Equal(null, scopeName.MetaData);
				Assert.Equal(TestClassification, scopeName.Classification);
			}

			if (VerifyTrueAndReturn(TimedScopeInstanceName.TryParse(ScopeWithMetadata, out scopeName, preferMetaData: true)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(null, scopeName.SubType);
				Assert.Equal(TestMetadata, scopeName.MetaData);
				Assert.Equal(TestClassification, scopeName.Classification);
			}

			if (VerifyTrueAndReturn(TimedScopeInstanceName.TryParse(ScopeWithSubtypeAndMetadata, out scopeName)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(TestSubtype, scopeName.SubType);
				Assert.Equal(TestMetadata, scopeName.MetaData);
				Assert.Equal(TestClassification, scopeName.Classification);
			}

			if (VerifyTrueAndReturn(TimedScopeInstanceName.TryParse(ScopeNameWithDescription, out scopeName)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(TestSubtype, scopeName.SubType);
				Assert.Equal(TestMetadata, scopeName.MetaData);
				Assert.Equal(TestClassification, scopeName.Classification);
			}

			if (VerifyTrueAndReturn(TimedScopeInstanceName.TryParse(ScopeWithEmptySubtype, out scopeName)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(string.Empty, scopeName.SubType);
				Assert.Equal(null, scopeName.MetaData);
				Assert.Equal(TestClassification, scopeName.Classification);
			}

			if (VerifyTrueAndReturn(TimedScopeInstanceName.TryParse(ScopeWithEmptySubtypeAndMetadata, out scopeName)))
			{
				Assert.Equal(TestScope, scopeName.Scope);
				Assert.Equal(string.Empty, scopeName.SubType);
				Assert.Equal(TestMetadata, scopeName.MetaData);
				Assert.Equal(TestClassification, scopeName.Classification);
			}
		}


		[Fact]
		public void Verify_MalformedScopeNames_NotParsed()
		{
			TimedScopeInstanceName scopeName;

			Assert.False(TimedScopeInstanceName.TryParse(".SystemError", out scopeName));
			Assert.False(TimedScopeInstanceName.TryParse(null, out scopeName));
			Assert.False(TimedScopeInstanceName.TryParse("", out scopeName));
			Assert.False(TimedScopeInstanceName.TryParse("///", out scopeName));
			Assert.False(TimedScopeInstanceName.TryParse("...///", out scopeName));
			Assert.False(TimedScopeInstanceName.TryParse("Scope.MalformedClassification/x", out scopeName));
			Assert.False(TimedScopeInstanceName.TryParse("/x/y", out scopeName));
			Assert.False(TimedScopeInstanceName.TryParse("x/y/z/a", out scopeName));
		}


		[Fact]
		public void Verify_HashCodesAndEquality()
		{
			CheckSingleScope(OnlyScope);
			CheckSingleScope(ScopeWithSubtype);
			CheckSingleScope(ScopeWithMetadata);
			CheckSingleScope(ScopeWithSubtypeAndMetadata);
			CheckSingleScope(ScopeNameWithDescription);
			CheckSingleScope(ScopeWithEmptySubtype);
			CheckSingleScope(ScopeWithEmptySubtypeAndMetadata);
		}


		/// <summary>
		/// Verifies that parsing the same string gives equal TimedScopeInstanceName instances
		/// </summary>
		/// <param name="scopeName">The string to be parsed</param>
		private void CheckSingleScope(string scopeName)
		{
			TimedScopeInstanceName parsed1;
			TimedScopeInstanceName parsed2;
			if (VerifyTrueAndReturn(TimedScopeInstanceName.TryParse(scopeName, out parsed1)) &&
				VerifyTrueAndReturn(TimedScopeInstanceName.TryParse(scopeName, out parsed2)))
			{
				CheckHashCodesAndEquality(parsed1, parsed2);
			}
		}


		/// <summary>
		/// Check equality of two timed scope instance names by all possible ways
		/// </summary>
		/// <param name="x">First instance</param>
		/// <param name="y">Second instance</param>
		private void CheckHashCodesAndEquality(TimedScopeInstanceName x, TimedScopeInstanceName y)
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

			// We are not checking round trip for ScopeNameWithDescription as we do not store the description in the scope name
		}


		/// <summary>
		/// Checks that parsing a string and then converting back to string gives the same string
		/// </summary>
		/// <param name="scopeName">String to be parsed and converted back</param>
		private void CheckRoundTrip(string scopeName)
		{
			TimedScopeInstanceName parsed;
			if (VerifyTrueAndReturn(TimedScopeInstanceName.TryParse(scopeName, out parsed)))
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