﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Model.Types;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Required client versions
	/// </summary>
	public class RequiredClient
	{
		/// <summary>
		/// HashSet to store version range
		/// </summary>
		private HashSet<ProductVersionRange> Range { get; }

		/// <summary>
		/// HashSet to store app override
		/// </summary>
		private SortedDictionary<string, RequiredApplication> App { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public RequiredClient()
		{
			Range = new HashSet<ProductVersionRange>();
			App = new SortedDictionary<string, RequiredApplication>(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Name of client
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
		/// Audience Group of client
		/// </summary>
		public string AudienceGroup { get; set; }

		/// <summary>
		/// Add version range to range set
		/// </summary>
		public bool AddVersionRange(ProductVersionRange range)
		{
			if (!Code.ValidateArgument(range, nameof(range), TaggingUtilities.ReserveTag(0x23821042 /* tag_967bc */)))
			{
				return false;
			}

			return Range.Add(range);
		}

		/// <summary>
		/// List of app override
		/// </summary>
		public SortedDictionary<string, RequiredApplication> Overrides => App;

		/// <summary>
		/// Add app override to overrides set
		/// </summary>
		public bool AddOverride(RequiredApplication app)
		{
			if (!Code.ValidateArgument(app, nameof(app), TaggingUtilities.ReserveTag(0x23821043 /* tag_967bd */)))
			{
				return false;
			}

			try
			{
				App.Add(app.Name, app);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
