// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.DocumentDb
{
	/// <summary>
	/// Document Db Access Type
	/// </summary>
	public enum AccessType
	{
		/// <summary>
		/// Read and Write access.
		/// </summary>
		ReadWrite = 0,

		/// <summary>
		/// Read only access.
		/// </summary>
		ReadOnly = 1
	}

	/// <summary>
	/// Document Db Settings Configuration. This class can be used by the Settings provider to decide which settings to provide.
	/// </summary>
	public class DocumentDbSettingsConfig
	{
		/// <summary>
		/// Environment. ie. Dev, Prod, etc.
		/// </summary>
		public string Environment { get; set; }

		/// <summary>
		/// Azure Region.
		/// </summary>
		public string Region { get; set; }

		/// <summary>
		/// Azure Region.
		/// </summary>
		public AccessType AccessType { get; set; }
	}
}
