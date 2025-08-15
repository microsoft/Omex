// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Constants;

/// <summary>
/// Constants for request parameter names.
/// </summary>
internal static class RequestParameters
{
	/// <summary>
	/// The HTTP headers.
	/// </summary>
	internal static class Header
	{
		/// <summary>
		/// The "X-Forwarded-For" HTTP header, which is present in some proxied calls.
		/// </summary>
		/// <remarks>This will only be present if Akamai is configured to allow this to pass through.</remarks>
		public const string ForwardedFor = "X-Forwarded-For";

		/// <summary>
		/// The "Partner" HTTP header, which is sent to us by the front-end.
		/// </summary>
		public const string Partner = "Partner";

		/// <summary>
		/// The "Platform" HTTP header, which is sent to us by the front-end.
		/// </summary>
		public const string Platform = "Platform";
	}

	/// <summary>
	/// The query-string parameters.
	/// </summary>
	internal static class Query
	{
		/// <summary>
		/// The campaign name query-string parameter.
		/// </summary>
		public const string Campaign = "campaign";

		/// <summary>
		/// The disabled features query-string parameter.
		/// </summary>
		public const string DisabledFeatures = "disabledFeatures";

		/// <summary>
		/// The enabled features query-string parameter.
		/// </summary>
		public const string EnabledFeatures = "enabledFeatures";

		/// <summary>
		/// The market query-string parameter.
		/// </summary>
		public const string Market = "market";

		/// <summary>
		/// The toggled features query-string parameter.
		/// </summary>
		public const string ToggledFeatures = "toggledFeatures";
	}
}
