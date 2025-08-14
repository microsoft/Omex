// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using Microsoft.Omex.Extensions.FeatureManagement.Types;

namespace Microsoft.Omex.Extensions.FeatureManagement.Experimentation
{
	/// <summary>
	/// The filters that can be used by experiments.
	/// </summary>
	public sealed class ExperimentFilters
	{
		/// <summary>
		/// Gets or sets target browser, e.g., "Edge" or "Chrome".
		/// </summary>
		public string Browser { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the campaign name.
		/// </summary>
		public string Campaign { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the correlation ID.
		/// </summary>
		public Guid CorrelationId { get; set; } = Guid.Empty;

		/// <summary>
		/// Gets or sets the customer ID.
		/// </summary>
		public CustomerId CustomerId { get; set; } = new();

		/// <summary>
		/// Gets or sets the type of device, e.g., "Mobile" or "Desktop".
		/// </summary>
		public string DeviceType { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the target language.
		/// </summary>
		public CultureInfo Language { get; set; } = CultureInfo.InvariantCulture;

		/// <summary>
		/// Gets or sets the target market.
		/// </summary>
		public string Market { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the platform.
		/// </summary>
		public string Platform { get; set; } = string.Empty;

		/// <inheritdoc/>
		public override string ToString() =>
			string.Join(';',
			[
				CreateToStringEntry(nameof(Browser), Browser),
				CreateToStringEntry(nameof(Campaign), Campaign),
				CreateToStringEntry(nameof(CorrelationId), CorrelationId.ToString()),
				CreateToStringEntry($"{nameof(CustomerId)}.{nameof(CustomerId.Id)}", CustomerId.Id),
				CreateToStringEntry($"{nameof(CustomerId)}.{nameof(CustomerId.Type)}", CustomerId.Type),
				CreateToStringEntry(nameof(DeviceType), DeviceType),
				CreateToStringEntry(nameof(Language), Language.Name),
				CreateToStringEntry(nameof(Market), Market),
				CreateToStringEntry(nameof(Platform), Platform),
			]);

		private string CreateToStringEntry(string fieldName, string value) =>
			$"{fieldName}:'{value}'";
	}
}
