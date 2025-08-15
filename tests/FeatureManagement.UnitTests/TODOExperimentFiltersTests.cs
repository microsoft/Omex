//// Copyright (C) Microsoft Corporation. All rights reserved.

//namespace Microsoft.Omex.Extensions.FeatureManagement.UnitTests;

//using Microsoft.VisualStudio.TestTools.UnitTesting;

//[TestClass]
//public sealed class ExperimentFiltersTests
//{
//	#region ToString

//	[TestMethod]
//	public void ToString_WithDefaultValues_ReturnsExpectedString()
//	{
//		// ARRANGE
//		ExperimentFilters filters = new();

//		// ACT
//		string result = filters.ToString();

//		// ASSERT
//		const string expected =
//			"Browser:'';" +
//			"Campaign:'';" +
//			"CorrelationId:'00000000-0000-0000-0000-000000000000';" +
//			"DeviceType:'';" +
//			"Language:'';" +
//			"Market:'';" +
//			"Platform:''";
//		Assert.AreEqual(expected, result);
//	}

//	[TestMethod]
//	public void ToString_WithCustomValues_ReturnsExpectedString()
//	{
//		// ARRANGE
//		ExperimentFilters filters = new()
//		{
//			Browser = "Chrome",
//			Campaign = "Summer2025",
//			CorrelationId = new("11111111-1111-1111-1111-111111111111"),
//			DeviceType = "Mobile",
//			Language = new("en-US"),
//			Market = "US",
//			Platform = "MyPlatform",
//		};

//		// ACT
//		string result = filters.ToString();

//		// ASSERT
//		const string expected =
//			"Browser:'Chrome';" +
//			"Campaign:'Summer2025';" +
//			"CorrelationId:'11111111-1111-1111-1111-111111111111';" +
//			"DeviceType:'Mobile';" +
//			"Language:'en-US';" +
//			"Market:'US';" +
//			"Platform:'MyPlatform'";
//		Assert.AreEqual(expected, result);
//	}

//	#endregion
//}
