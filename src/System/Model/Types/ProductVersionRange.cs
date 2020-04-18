// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.Serialization;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Model.Types
{
	/// <summary>
	/// Product Version Range class.
	/// </summary>
	[DataContract]
	public class ProductVersionRange
	{
		/// <summary>
		/// Initializes a new instance of the ProductVersionRange class using the specified
		/// min and max values.
		/// </summary>
		/// <param name="min">The min version number.</param>
		/// <param name="max">The max version number.</param>
		public ProductVersionRange(ProductVersion min, ProductVersion max)
		{
			Min = Code.ExpectsArgument(min, nameof(min), TaggingUtilities.ReserveTag(0x23820852 /* tag_9667s */));
			Max = Code.ExpectsArgument(max, nameof(max), TaggingUtilities.ReserveTag(0x23820853 /* tag_9667t */));
		}

		/// <summary>
		/// The min number
		/// </summary>
		[DataMember]
		public ProductVersion Min { get; private set; }

		/// <summary>
		/// The max number
		/// </summary>
		[DataMember]
		public ProductVersion Max { get; private set; }
	}
}
