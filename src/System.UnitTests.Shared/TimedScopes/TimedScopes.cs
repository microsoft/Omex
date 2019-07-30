// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.CodeDom.Compiler;
using Microsoft.Omex.System.TimedScopes;

namespace Microsoft.Omex.System.UnitTests.Shared.TimedScopes
{
	internal static class UnitTestTimedScopes
	{
		/// <summary>
		/// TryGetHash Timed Scope
		/// </summary>
		public static TimedScopeDefinition DefaultScope => new TimedScopeDefinition("OmexDefaultTimedScope",
			"Omex default timed scope");


		/// <summary>
		/// Test counters
		/// </summary>
		public static class TestCounters
		{
			/// <summary>
			/// Unit test timed scope
			/// </summary>
			public static TimedScopeDefinition UnitTest => new TimedScopeDefinition("UnitTest",
				"Omex unit test timed scope");
		}
	}


	/// <summary>
	/// Timed scopes
	/// </summary>
	internal static class TimedScopes
	{
		/// <summary>
		/// TryGetHash Timed Scope
		/// </summary>
		public static TimedScopeDefinition DefaultScope => new TimedScopeDefinition("OmexDefaultTimedScope",
			"Tries to retrieve a dictionary of values from Omex Redis Cache.");
	}
}
