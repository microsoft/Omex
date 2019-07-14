// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.DocumentDb
{
	/// <summary>
	/// Document db settings class.
	/// </summary>
	public class DocumentDbSettings
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="endpoint">Document db endpoint.</param>
		/// <param name="key">Document db key.</param>
		public DocumentDbSettings(string endpoint, string key)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(endpoint, nameof(endpoint), 0);
			Code.ExpectsNotNullOrWhiteSpaceArgument(key, nameof(key), 0);

			Endpoint = new Uri(endpoint);
			Key = key;
		}


		/// <summary>
		/// Document db endpoint.
		/// </summary>
		public Uri Endpoint { get; }


		/// <summary>
		/// Document db key.
		/// </summary>
		public string Key { get; }
	}
}