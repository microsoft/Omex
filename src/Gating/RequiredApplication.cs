// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Model.Types;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Required application versions
	/// </summary>
	public class RequiredApplication
	{
		/// <summary>
		/// HashSet to store version range
		/// </summary>
		private HashSet<ProductVersionRange> Range { get; }


		/// <summary>
		/// Constructor
		/// </summary>
		public RequiredApplication() => Range = new HashSet<ProductVersionRange>();


		/// <summary>
		/// Name of AppCode
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// Minimum version required
		/// </summary>
		public ProductVersion MinVersion { get; set; }


		/// <summary>
		/// Maximum version allowed
		/// </summary>
		public ProductVersion MaxVersion { get; set; }


		/// <summary>
		/// Allowed version range
		/// </summary>
		public HashSet<ProductVersionRange> VersionRanges => Range;


		/// <summary>
		/// Value of Audience Group
		/// </summary>
		public string AudienceGroup { get; set; }


		/// <summary>
		/// Add version range to range set
		/// </summary>
		public bool AddVersionRange(ProductVersionRange range)
		{
			if (!Code.ValidateArgument(range, nameof(range), TaggingUtilities.ReserveTag(0x23850601 /* tag_97qyb */)))
			{
				return false;
			}

			return Range.Add(range);
		}
	}
}