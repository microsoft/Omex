// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class SimpleScopeProviderTests
	{
		[TestMethod]
		[TestCategory(nameof(Activity))]
		public void CreateAndStart_TimedScopeCreated()
		{
			ActivityResult result = ActivityResult.ExpectedError;
			TimedScopeDefinition definition = new TimedScopeDefinition(nameof(CreateAndStart_TimedScopeCreated));
			SimpleScopeProvider provider = new SimpleScopeProvider();
			TimedScope scope = provider.CreateAndStart(definition, result);
			Assert.IsNotNull(scope);
		}
	}
}
