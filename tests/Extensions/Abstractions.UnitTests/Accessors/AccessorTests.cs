﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Accessors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	[TestCategory(nameof(Accessor<object>))]
	public class ServiceAccessorTests
	{
		[TestMethod]
		public void ValuePassedInConstructor_ProperlyHandlesActions()
		{
			object value = new object();
			Accessor<object> accessor = new Accessor<object>(value);
			IAccessor<object> publicAccessor = accessor;

			Assert.AreEqual(value, publicAccessor.Value);
			object? recivedContext = null;
			publicAccessor.OnFirstSet(c => recivedContext = c);

			Assert.AreEqual(value, recivedContext);
		}

		[TestMethod]
		public void ValueAfterInitialization_ProperlyHandlesActions()
		{
			object value = new object();
			Accessor<object> accessor = new Accessor<object>();
			IAccessor<object> publicAccessor = accessor;
			IAccessorSetter<object> setter = accessor;

			object? receivedContext = null;
			publicAccessor.OnFirstSet(c => receivedContext = c);
			setter.SetValue(value);

			Assert.AreEqual(value, receivedContext);
		}
	}
}
