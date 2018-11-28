/***************************************************************************************************
	CategoryUnitTests.cs

	Unit tests for Category class
***************************************************************************************************/

using System;
using Microsoft.Omex.System.Logging;
using Xunit;

namespace Microsoft.Omex.System.UnitTests.Logging
{
	/// <summary>
	/// Unit tests for Category class
	/// </summary>
	public sealed class CategoryUnitTests
	{
		[Fact]
		public void ConstructorWithName_CreatesCategoryWithSameName()
		{
			string categoryName = "My Category";
			Assert.Equal(categoryName, new Category(categoryName).Name);
		}


		[Fact]
		public void ConstructorWithNullArgument_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new Category(null));
		}


		[Fact]
		public void ConstructorWithEmptyArgument_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => new Category(""));
		}


		[Fact]
		public void ConstructorWithWhiteSpaceArgument_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => new Category(" "));
		}
	}
}
