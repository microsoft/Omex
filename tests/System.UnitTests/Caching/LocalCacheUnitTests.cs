// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Omex.System.Caching;
using Xunit;

namespace Microsoft.Omex.System.UnitTests.Shared.Caching
{
	/// <summary>
	/// Unit tests for the LocalCache class.
	/// </summary>
	public sealed class LocalCacheUnitTests : UnitTestBase
	{
		[Fact]
		public void Get_WithNullTypeParameter_ReturnsNull()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			object result = cache.Get(null);
			Assert.Null(result);
		}


		[Fact]
		public void Get_WithNonExistentTypeParameter_ReturnsNull()
		{
			LocalCache cache = new LocalCache();
			object result = cache.Get(typeof(TestStringObject));
			Assert.Null(result);
		}


		[Fact]
		public void Get_WithExistingTypeParameter_ReturnsValue()
		{
			LocalCache cache = new LocalCache();
			TestStringObject getOrAddResult = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasAdded) as TestStringObject;
			Assert.Equal(TestString1, getOrAddResult.Value);
			Assert.True(wasAdded);

			TestStringObject getResult = cache.Get(typeof(TestStringObject)) as TestStringObject;
			Assert.Equal(TestString1, getResult.Value);
		}


		[Fact]
		public void Get_InParallelWithExistingTypeParameter_ReturnsValue()
		{
			LocalCache cache = new LocalCache();
			TestStringObject getOrAddResult = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasAdded) as TestStringObject;
			Assert.Equal(TestString1, getOrAddResult.Value);
			Assert.True(wasAdded);

			Parallel.For(0, 10, i =>
			{
				TestStringObject getResult = cache.Get(typeof(TestStringObject)) as TestStringObject;
				Assert.Equal(TestString1, getResult.Value);
			});
		}


		[Fact]
		public void GetOrAdd_WithNullTypeParameter_ReturnsNull()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			object result = cache.GetOrAdd(null, () => new TestStringObject(TestString1), out bool wasAdded);
			Assert.Null(result);
			Assert.False(wasAdded);
		}


		[Fact]
		public void GetOrAdd_InParallelWithNullTypeParameter_ReturnsNull()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			Parallel.For(0, 10, i =>
			{
				object result = cache.GetOrAdd(null, () => new TestStringObject(TestString1), out bool wasAdded);
				Assert.Null(result);
				Assert.False(wasAdded);
			});
		}


		[Fact]
		public void GetOrAdd_WithNullValueParameter_ReturnsNull()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			object result = cache.GetOrAdd(typeof(TestStringObject), null, out bool wasAdded);
			Assert.Null(result);
			Assert.False(wasAdded);
		}


		[Fact]
		public void GetOrAdd_InParallelWithNullValueParameter_ReturnsNull()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			Parallel.For(0, 10, i =>
			{
				object result = cache.GetOrAdd(typeof(TestStringObject), null, out bool wasAdded);
				Assert.Null(result);
				Assert.False(wasAdded);
			});
		}


		[Fact]
		public void GetOrAdd_WithEmptyDictionary_ReturnsAddedObject()
		{
			LocalCache cache = new LocalCache();
			TestStringObject resultString = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasAdded) as TestStringObject;
			Assert.Equal(TestString1, resultString.Value);
			Assert.True(wasAdded);
		}


		[Fact]
		public void GetOrAdd_InParallelWithEmptyDictionary_ReturnsAddedObject()
		{
			LocalCache cache = new LocalCache();
			int addCount = 0;
			Parallel.For(0, 10, i =>
			{
				TestStringObject resultString = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasAdded) as TestStringObject;
				Assert.Equal(TestString1, resultString.Value);
				if (wasAdded)
				{
					Interlocked.Increment(ref addCount);
				}
			});

			Assert.Equal(addCount, 1);
		}


		[Fact]
		public void GetOrAdd_WithNonEmptyDictionary_ReturnsAddedObject()
		{
			LocalCache cache = new LocalCache();
			TestIntegerObject resultInt = cache.GetOrAdd(typeof(TestIntegerObject), () => new TestIntegerObject(TestInteger), out bool wasAdded) as TestIntegerObject;
			Assert.Equal(TestInteger, resultInt.Value);
			Assert.True(wasAdded, "GetOrAdd added an entry to the cache.");

			TestStringObject resultString = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString1), out wasAdded) as TestStringObject;
			Assert.Equal(TestString1, resultString.Value);
			Assert.True(wasAdded, "GetOrAdd added an entry to the cache.");
		}


		[Fact]
		public void GetOrAdd_InParallelWithNonEmptyDictionary_ReturnsAddedObject()
		{
			LocalCache cache = new LocalCache();
			TestIntegerObject resultInt = cache.GetOrAdd(typeof(TestIntegerObject), () => new TestIntegerObject(TestInteger), out bool wasAdded) as TestIntegerObject;
			Assert.Equal(TestInteger, resultInt.Value);
			Assert.True(wasAdded, "GetOrAdd added an entry to the cache.");

			int addCount = 0;
			Parallel.For(0, 10, i =>
			{
				TestStringObject resultString = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasAddedInternal) as TestStringObject;
				Assert.Equal(TestString1, resultString.Value);
				if (wasAddedInternal)
				{
					Interlocked.Increment(ref addCount);
				}
			});

			Assert.Equal(addCount, 1);
		}


		[Fact]
		public void GetOrAdd_WithRepeatedEntry_ReturnsFirstObject()
		{
			LocalCache cache = new LocalCache();
			TestStringObject resultString1 = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasAdded) as TestStringObject;
			Assert.Equal(TestString1, resultString1.Value);
			Assert.True(wasAdded);

			TestStringObject resultString2 = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString2), out wasAdded) as TestStringObject;
			Assert.Equal(TestString1, resultString2.Value);
			Assert.False(wasAdded);
		}


		[Fact]
		public void GetOrAdd_InParallelWithRepeatedEntry_ReturnsFirstObject()
		{
			LocalCache cache = new LocalCache();
			TestStringObject resultString1 = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasAdded) as TestStringObject;
			Assert.Equal(TestString1, resultString1.Value);
			Assert.True(wasAdded);

			Parallel.For(0, 10, i =>
			{
				TestStringObject resultString2 = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString2), out bool wasAddedInternal) as TestStringObject;
				Assert.Equal(TestString1, resultString2.Value);
				Assert.False(wasAddedInternal);
			});
		}


		[Fact]
		public void AddOrUpdate_WithNullTypeParameter_ReturnsNull()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			object result = cache.AddOrUpdate(null, () => new TestStringObject(TestString1), out bool wasUpdated);
			Assert.Null(result);
			Assert.False(wasUpdated);
		}


		[Fact]
		public void AddOrUpdate_InParallelWithNullTypeParameter_ReturnsNull()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			Parallel.For(0, 10, i =>
			{
				object result = cache.AddOrUpdate(null, () => new TestStringObject(TestString1), out bool wasUpdated);
				Assert.Null(result);
				Assert.False(wasUpdated);
			});
		}


		[Fact]
		public void AddOrUpdate_WithNullValueParameter_ReturnsNull()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			object result = cache.AddOrUpdate(typeof(TestStringObject), null, out bool wasUpdated);
			Assert.Null(result);
			Assert.False(wasUpdated);
		}


		[Fact]
		public void AddOrUpdate_InParallelWithNullValueParameter_ReturnsNull()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			Parallel.For(0, 10, i =>
			{
				object result = cache.AddOrUpdate(typeof(TestStringObject), null, out bool wasUpdated);
				Assert.Null(result);
				Assert.False(wasUpdated);
			});
		}


		[Fact]
		public void AddOrUpdate_WithEmptyDictionary_ReturnsUpdatedObject()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			TestStringObject resultString = cache.AddOrUpdate(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasUpdated) as TestStringObject;
			Assert.Equal(TestString1, resultString.Value);
			Assert.True(wasUpdated);
		}


		[Fact]
		public void AddOrUpdate_InParallelWithEmptyDictionary_ReturnsUpdatedObject()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			Parallel.For(0, 10, i =>
			{
				TestStringObject resultString = cache.AddOrUpdate(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasUpdated) as TestStringObject;
				Assert.Equal(TestString1, resultString.Value);
				Assert.True(wasUpdated);
			});
		}


		[Fact]
		public void AddOrUpdate_WithNonEmptyDictionary_ReturnsUpdatedObject()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			TestIntegerObject resultInt = cache.AddOrUpdate(typeof(TestIntegerObject), () => new TestIntegerObject(TestInteger), out bool wasUpdated) as TestIntegerObject;
			Assert.Equal(TestInteger, resultInt.Value);
			Assert.True(wasUpdated);

			TestStringObject resultString = cache.AddOrUpdate(typeof(TestStringObject), () => new TestStringObject(TestString1), out wasUpdated) as TestStringObject;
			Assert.Equal(TestString1, resultString.Value);
			Assert.True(wasUpdated);
		}


		[Fact]
		public void AddOrUpdate_InParallelWithNonEmptyDictionary_ReturnsUpdatedObject()
		{
			FailOnErrors = false;

			LocalCache cache = new LocalCache();
			Parallel.For(0, 10, i =>
			{
				TestIntegerObject resultInt = cache.AddOrUpdate(typeof(TestIntegerObject), () => new TestIntegerObject(TestInteger), out bool wasUpdated) as TestIntegerObject;
				Assert.Equal(TestInteger, resultInt.Value);
				Assert.True(wasUpdated);

				TestStringObject resultString = cache.AddOrUpdate(typeof(TestStringObject), () => new TestStringObject(TestString1), out wasUpdated) as TestStringObject;
				Assert.Equal(TestString1, resultString.Value);
				Assert.True(wasUpdated);
			});
		}


		[Fact]
		public void AddOrUpdate_WithRepeatedEntry_ReturnsSecondObject()
		{
			LocalCache cache = new LocalCache();
			TestStringObject resultString1 = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasAdded) as TestStringObject;
			Assert.Equal(TestString1, resultString1.Value);
			Assert.True(wasAdded);

			TestStringObject resultString2 = cache.AddOrUpdate(typeof(TestStringObject), () => new TestStringObject(TestString2), out bool wasUpdated) as TestStringObject;
			Assert.Equal(resultString2.Value, TestString2);
			Assert.True(wasUpdated);
		}


		[Fact]
		public void AddOrUpdate_InParallelWithRepeatedEntry_ReturnsSecondObject()
		{
			LocalCache cache = new LocalCache();
			TestStringObject resultString1 = cache.GetOrAdd(typeof(TestStringObject), () => new TestStringObject(TestString1), out bool wasAdded) as TestStringObject;
			Assert.Equal(TestString1, resultString1.Value);
			Assert.True(wasAdded);

			Parallel.For(0, 10, i =>
			{
				TestStringObject resultString2 = cache.AddOrUpdate(typeof(TestStringObject), () => new TestStringObject(TestString2), out bool wasUpdated) as TestStringObject;
				Assert.Equal(resultString2.Value, TestString2);
				Assert.True(wasUpdated);
			});
		}


		/// <summary>
		/// The first test string.
		/// </summary>
		private const string TestString1 = "string";


		/// <summary>
		/// The second test string.
		/// </summary>
		private const string TestString2 = "string2";


		/// <summary>
		/// The test integer.
		/// </summary>
		private const int TestInteger = 2;


		/// <summary>
		/// A test string object.
		/// </summary>
		/// <remarks>Each instance will have a different pointer, allowing all cache checks to be run.</remarks>
		private sealed class TestStringObject
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="TestStringObject"/> class.
			/// </summary>
			/// <param name="value">The value.</param>
			public TestStringObject(string value) => Value = value;


			/// <summary>
			/// Gets the value.
			/// </summary>
			public string Value { get; }
		}


		/// <summary>
		/// A test integer object.
		/// </summary>
		/// <remarks>Each instance will have a different pointer, allowing all cache checks to be run.</remarks>
		private sealed class TestIntegerObject
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="TestIntegerObject"/> class.
			/// </summary>
			/// <param name="value">The value.</param>
			public TestIntegerObject(int value) => Value = value;


			/// <summary>
			/// Gets the value.
			/// </summary>
			public int Value { get; }
		}
	}
}