// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Types
{
	/// <summary>
	/// An enumeration representing the type of device making a request.
	/// </summary>
	internal enum DeviceType
	{
		/// <summary>
		/// The device type is unknown.
		/// </summary>
		Unknown,

		/// <summary>
		/// The device is a desktop or laptop computer.
		/// </summary>
		Desktop,

		/// <summary>
		/// The device is a mobile phone or tablet.
		/// </summary>
		Mobile,
	}
}
