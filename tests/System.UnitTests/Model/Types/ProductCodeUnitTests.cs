// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Microsoft.Omex.System.Model.Types;
using Microsoft.Omex.System.UnitTests.Shared;
using Xunit;

namespace Microsoft.Omex.System.UnitTests.Model.Types
{
	/// <summary>
	/// Unit tests for ProductCode class
	/// </summary>
	public sealed class ProductCodeUnitTests : UnitTestBase
	{
		[Fact]
		public void ProductCode_WithValidCode_ShouldPreserveApplicationPlatform()
		{
			DataContractSerializer serializer = new DataContractSerializer(typeof(ProductCode));

			ProductCode originalProductCode = new ProductCode("WAC_Excel");
			ProductCode deserializedProductCode = null;

			using (MemoryStream stream = new MemoryStream())
			{
				XmlWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
				serializer.WriteObject(writer, originalProductCode);
				writer.Flush();

				stream.Seek(0, SeekOrigin.Begin);

				deserializedProductCode = serializer.ReadObject(stream) as ProductCode;
			}

			Assert.NotNull(deserializedProductCode);
			Assert.Equal("WAC", deserializedProductCode.Platform);
			Assert.Equal("Excel", deserializedProductCode.Application);
		}


		[Fact]
		public void ProductCode_WithLegacyCode_ShouldNotPreserveApplicationPlatform()
		{
			DataContractSerializer serializer = new DataContractSerializer(typeof(ProductCode));

			ProductCode originalProductCode = new ProductCode("ZWD150");
			ProductCode deserializedProductCode = null;

			using (MemoryStream stream = new MemoryStream())
			{
				XmlWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
				serializer.WriteObject(writer, originalProductCode);
				writer.Flush();

				stream.Seek(0, SeekOrigin.Begin);

				deserializedProductCode = serializer.ReadObject(stream) as ProductCode;
			}

			Assert.NotNull(deserializedProductCode);
			Assert.Null(deserializedProductCode.Platform);
			Assert.Null(deserializedProductCode.Application);
		}
	}
}
