// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.UnitTests.Shared;
using System;
using Xunit;

namespace Microsoft.Omex.Gating.UnitTests
{
	/// <summary>
	/// Unit tests for the GateDataSetFactory class
	/// </summary>
	public sealed class GateDataSetFactoryUnitTests : UnitTestBase
	{
		[Fact]
		public void ConstructorWithValidParameters_ShouldSucceed()
		{
			IDataSetFactory<GateDataSet> dataSetFactory = new GateDataSetFactory("GatesResourceNameTest.xml", "TestGroupsResourceNameTest.xml");
			GateDataSet dataSetInstance = dataSetFactory.Create();

			Assert.NotNull(dataSetInstance);
		}


		[Fact]
		public void ConstructorWithNullParameters_Throws()
		{
			FailOnErrors = false;

			Assert.Throws<ArgumentNullException>(() => new GateDataSetFactory(null, "TestGroupsResourceNameTest.xml")); ;
			Assert.Throws<ArgumentNullException>(() => new GateDataSetFactory("GatesResourceNameTest.xml", null)); ;
			Assert.Throws<ArgumentNullException>(() => new GateDataSetFactory(null, null));
		}
	}
}
