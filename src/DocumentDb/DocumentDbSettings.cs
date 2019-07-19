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
		protected DocumentDbSettings(string endpoint, DocumentDbSettingsConfig config = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(endpoint, nameof(endpoint), TaggingUtilities.ReserveTag(0));

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
			Code.ExpectsNotNullOrWhiteSpaceArgument(endpoint, nameof(endpoint), TaggingUtilities.ReserveTag(0));
			Code.ExpectsNotNullOrWhiteSpaceArgument(key, nameof(key), TaggingUtilities.ReserveTag(0));

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