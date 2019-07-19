// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Security;
using Microsoft.Omex.System.Extensions;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.DocumentDb
{
	/// <summary>
	/// Document db settings class.
	/// </summary>
	public class DocumentDbSecureSettings : DocumentDbSettings
	{
		private readonly Lazy<string> m_lazyKey = null;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="endpoint">Document db endpoint.</param>
		/// <param name="key">Document db key.</param>
		public DocumentDbSecureSettings(string endpoint, SecureString key) : base(endpoint)
		{
			Code.ExpectsArgument(key, nameof(key), TaggingUtilities.ReserveTag(0));

			SecureKey = key;
			m_lazyKey = new Lazy<string>(() => SecureKey.ToPlainText(), LazyThreadSafetyMode.ExecutionAndPublication);
		}


		/// <summary>
		/// Document db key.
		/// </summary>
		public override string Key => m_lazyKey.Value;


		/// <summary>
		/// Document db key in secure string.
		/// </summary>
		public SecureString SecureKey { get; }
	}
}
