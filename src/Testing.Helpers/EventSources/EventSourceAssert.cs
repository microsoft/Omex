// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.Tracing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Testing.Helpers
{
	/// <summary>
	/// Helper methods for ETW classes
	/// </summary>
	public static class EventSourceAssert
	{
		/// <summary>
		/// Asserts that ETW event payload has proper value
		/// </summary>
		public static void AssertPayload<TPayloadType>(this EventWrittenEventArgs info, string name, TPayloadType? expected)
			where TPayloadType : class
		{
			int index = info.PayloadNames?.IndexOf(name) ?? -1;

			TPayloadType? value = (TPayloadType?)(index < 0 ? null : info.Payload?[index]);

			Assert.AreEqual(expected, value, $"Wrong value for {name}");
		}
	}
}
