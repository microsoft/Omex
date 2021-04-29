// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Wrappers;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class WrapperTests
	{
		[TestMethod]
		public void Constructor_InitializesPropertiesProperly()
		{
			// Act.
			Wrapper<int> wrapper = new(() => Task.FromResult(42));

			// Assert.
			// If we are here, PublishAsync didn't explode.
		}

		[TestMethod]
		public async Task Get_ReturnsValidObjectAsync()
		{
			// Arrange.
			Wrapper<string> wrapper = new(() => Task.FromResult("42"));

			// Act.
			string value = await wrapper.GetAsync();

			// Assert.
			NullableAssert.IsNotNull(value);
		}
	}
}
