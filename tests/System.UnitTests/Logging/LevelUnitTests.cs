// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Logging;
using Xunit;

namespace Microsoft.Omex.System.UnitTests.Shared.Logging
{
	/// <summary>
	/// Unit tests for Level class
	/// </summary>
	public sealed class LevelUnitTests : UnitTestBase
	{
		[Fact]
		public void ComparingLevelUsingEquals_WithNull_ShouldReturnFalse()
		{
			Assert.False(Levels.Error.Equals(null));
		}

		[Fact]
		public void ComparingLevelUsingEquals_WithNonLevelObject_ShouldReturnFalse()
		{
			Assert.False(Levels.Error.Equals(this));
		}

		[Fact]
		public void ComparingLevelUsingEquals_WithDifferentLevel_ShouldReturnFalse()
		{
			Assert.False(Levels.Error.Equals(Levels.Warning));
		}

		[Fact]
		public void ComparingLevelUsingEquals_WithEqualLevel_ShouldReturnTrue()
		{
			Assert.True(Levels.Error.Equals(Levels.Error));
		}

		[Fact]
		public void ComparingLevelUsingEqualOperator_WithDifferentLevel_ShouldReturnFalse()
		{
			Assert.False(Levels.Error == Levels.Warning);
		}

		[Fact]
		public void ComparingLevelUsingEqualOperator_WithSameLevel_ShouldReturnTrue()
		{
			Level expectedLevel = Levels.Error;
			Assert.True(Levels.Error == expectedLevel);
		}

		[Fact]
		public void ComparingLevelUsingInequalityOperator_WithDifferentLevel_ShouldReturnTrue()
		{
			Assert.True(Levels.Error != Levels.Warning);
		}

		[Fact]
		public void ComparingLevelUsingInequalityOperator_WithSameLevel_ShouldReturnFalse()
		{
			Level expectedLevel = Levels.Error;
			Assert.False(Levels.Error != expectedLevel);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(4)]
		public void GetHashCode_ShouldReturnUnderlyingIntegerValue(int levelValue)
		{
			Assert.Equal(levelValue, Levels.LevelFromLogLevel((Levels.LogLevel)levelValue).GetHashCode());
		}

		[Theory]
		[InlineData(Levels.LogLevel.Error)]
		[InlineData(Levels.LogLevel.Info)]
		[InlineData(Levels.LogLevel.Spam)]
		[InlineData(Levels.LogLevel.Verbose)]
		[InlineData(Levels.LogLevel.Warning)]
		public void ToString_ShouldReturnUnderlyingEnumValue(Levels.LogLevel levelValue)
		{
			Assert.Equal(levelValue.ToString(), Levels.LevelFromLogLevel(levelValue).ToString());
		}
	}
}
