// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Constants;

/// <summary>
/// Activity names related to feature management.
/// </summary>
internal static class FeatureManagementActivityNames
{
	private const string Separator = "_";
	private const string ActivityNamePrefix = $"Omex{Separator}";

	/// <summary>
	/// Activity names related to <see cref="FeatureGatesService"/>.
	/// </summary>
	internal static class FeatureGatesService
	{
		private const string ActivityNamePrefix = $"{FeatureManagementActivityNames.ActivityNamePrefix}{nameof(FeatureGatesService)}{Separator}";

		/// <summary>
		/// The activity name for the <c>GetExperimentalFeaturesAsync</c> method.
		/// </summary>
		public const string GetExperimentalFeaturesAsync = $"{ActivityNamePrefix}{nameof(GetExperimentalFeaturesAsync)}";

		/// <summary>
		/// The activity name for the <c>GetFeatureGatesAsync</c> method.
		/// </summary>
		public const string GetFeatureGatesAsync = $"{ActivityNamePrefix}{nameof(GetFeatureGatesAsync)}";

		/// <summary>
		/// The activity name for the <c>IsFeatureGateApplicableAsync</c> method.
		/// </summary>
		public const string IsFeatureGateApplicableAsync = $"{ActivityNamePrefix}{nameof(IsFeatureGateApplicableAsync)}";

		/// <summary>
		/// The activity name for the <c>IsExperimentApplicableAsync</c> method.
		/// </summary>
		public const string IsExperimentApplicableAsync = $"{ActivityNamePrefix}{nameof(IsExperimentApplicableAsync)}";
	}

	/// <summary>
	/// Activity names related to <see cref="FeatureGatesConsolidator"/>.
	/// </summary>
	internal static class FeatureGatesConsolidator
	{
		private const string ActivityNamePrefix = $"{FeatureManagementActivityNames.ActivityNamePrefix}{nameof(FeatureGatesConsolidator)}{Separator}";

		/// <summary>
		/// The activity name for the <c>GetExperimentalFeaturesAsync</c> method.
		/// </summary>
		public const string GetExperimentalFeaturesAsync = $"{ActivityNamePrefix}{nameof(GetExperimentalFeaturesAsync)}";
	}
}
