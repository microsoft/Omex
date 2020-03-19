// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.DocumentDb
{
	/// <summary>
	/// Document db settings class.
	/// </summary>
	public class DocumentDbSettings
	{
		/// <summary>
		/// Document db settings class.
		/// </summary>
		protected DocumentDbSettings(string endpoint, DocumentDbSettingsConfig config = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(endpoint, nameof(endpoint), TaggingUtilities.ReserveTag(0x2381b146 /* tag_961fg */));

			Endpoint = new Uri(endpoint);
			Config = config;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="endpoint">Document db endpoint.</param>
		/// <param name="key">Document db key.</param>
		/// <param name="config">Document db settings config.</param>
		public DocumentDbSettings(string endpoint, string key, DocumentDbSettingsConfig config = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(endpoint, nameof(endpoint), TaggingUtilities.ReserveTag(0x2381b147 /* tag_961fh */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(key, nameof(key), TaggingUtilities.ReserveTag(0x2381b148 /* tag_961fi */));

			Endpoint = new Uri(endpoint);
			Key = key;
			Config = config;
		}

		/// <summary>
		/// Document db endpoint.
		/// </summary>
		public Uri Endpoint { get; }

		/// <summary>
		/// Document db key.
		/// </summary>
		public virtual string Key { get; }

		/// <summary>
		/// Document db settings configuration.
		/// </summary>
		public DocumentDbSettingsConfig Config { get; }
	}
}
