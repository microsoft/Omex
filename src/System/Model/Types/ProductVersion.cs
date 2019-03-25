// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.Serialization;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Model.Types
{
	/// <summary>
	/// Product Version class. It mimics functionality of the System.Version class however
	/// Build and Revision properties default to 0 when they are not set (instead of -1).
	/// </summary>
	[DataContract]
	public class ProductVersion : IComparable, IComparable<ProductVersion>, IEquatable<ProductVersion>
	{
		#region Standardization constants

		/// <summary>
		/// Standarized minimum version
		/// </summary>
		public static readonly string StandardizedMinVersion = "00000000.00000000.00000000.00000000";


		/// <summary>
		/// Maximum number of digits in major
		/// </summary>
		public static readonly int MaximumLengthOfMajor = 8;


		/// <summary>
		/// Maximum number of digits in minor
		/// </summary>
		public static readonly int MaximumLengthOfMinor = 8;


		/// <summary>
		/// Maximum number of digits in build
		/// </summary>
		public static readonly int MaximumLengthOfBuild = 8;


		/// <summary>
		/// Maximum number of digits in revision
		/// </summary>
		public static readonly int MaximumLengthOfRevision = 8;

		#endregion


		/// <summary>
		/// Minimum version
		/// </summary>
		public static readonly ProductVersion MinVersion = new ProductVersion(0, 0, 0, 0);


		/// <summary>
		/// Maximum version
		/// </summary>
		public static readonly ProductVersion MaxVersion = new ProductVersion((int)Math.Pow(10, MaximumLengthOfMajor) - 1,
			(int)Math.Pow(10, MaximumLengthOfMinor) - 1, (int)Math.Pow(10, MaximumLengthOfBuild) - 1, (int)Math.Pow(10, MaximumLengthOfRevision) - 1);


		/// <summary>
		/// Initializes a new instance of the ProductVersion class using the specified
		/// major and minor values.
		/// </summary>
		/// <param name="major">The major version number.</param>
		/// <param name="minor">The minor version number.</param>
		public ProductVersion(int major, int minor)
		{
			Code.Expects<ArgumentOutOfRangeException>(major >= 0, "Major is less than zero.", TaggingUtilities.ReserveTag(0x23850599 /* tag_97qwz */));
			Code.Expects<ArgumentOutOfRangeException>(minor >= 0, "Minor is less than zero.", TaggingUtilities.ReserveTag(0x2385059a /* tag_97qw0 */));

			Major = major;
			Minor = minor;
		}


		/// <summary>
		/// Initializes a new instance of the ProductVersion class using the specified
		/// major, minor and build values.
		/// </summary>
		/// <param name="major">The major version number.</param>
		/// <param name="minor">The minor version number.</param>
		/// <param name="build">The build number</param>
		public ProductVersion(int major, int minor, int build)
		{
			Code.Expects<ArgumentOutOfRangeException>(major >= 0, "Major is less than zero.", TaggingUtilities.ReserveTag(0x2385059b /* tag_97qw1 */));
			Code.Expects<ArgumentOutOfRangeException>(minor >= 0, "Minor is less than zero.", TaggingUtilities.ReserveTag(0x2385059c /* tag_97qw2 */));
			Code.Expects<ArgumentOutOfRangeException>(build >= 0, "Build is less than zero.", TaggingUtilities.ReserveTag(0x2385059d /* tag_97qw3 */));

			Major = major;
			Minor = minor;
			Build = build;
		}


		/// <summary>
		/// Initializes a new instance of the ProductVersion class using the specified
		/// major, minor, build and revision values.
		/// </summary>
		/// <param name="major">The major version number.</param>
		/// <param name="minor">The minor version number.</param>
		/// <param name="build">The build number</param>
		/// <param name="revision">The revision number</param>
		public ProductVersion(int major, int minor, int build, int revision)
		{
			Code.Expects<ArgumentOutOfRangeException>(major >= 0, "Major is less than zero.", TaggingUtilities.ReserveTag(0x2385059e /* tag_97qw4 */));
			Code.Expects<ArgumentOutOfRangeException>(minor >= 0, "Minor is less than zero.", TaggingUtilities.ReserveTag(0x2385059f /* tag_97qw5 */));
			Code.Expects<ArgumentOutOfRangeException>(build >= 0, "Build is less than zero.", TaggingUtilities.ReserveTag(0x238505a0 /* tag_97qw6 */));
			Code.Expects<ArgumentOutOfRangeException>(revision >= 0, "Revision is less than zero.", TaggingUtilities.ReserveTag(0x238505a1 /* tag_97qw7 */));

			Major = major;
			Minor = minor;
			Build = build;
			Revision = revision;
		}


		/// <summary>
		/// Determines whether two specified objects are not equal.
		/// </summary>
		/// <param name="v1">The first object.</param>
		/// <param name="v2">The second object.</param>
		/// <returns>True if v1 does not equals v2; false otherwise.</returns>
		public static bool operator !=(ProductVersion v1, ProductVersion v2) => !(v1 == v2);


		/// <summary>
		/// Determines whether the first specified object is less than
		/// the second specified object.
		/// </summary>
		/// <param name="v1">The first object.</param>
		/// <param name="v2">The second object.</param>
		/// <returns>True if v1 is less than v2; false otherwise.</returns>
		public static bool operator <(ProductVersion v1, ProductVersion v2)
		{
			Code.ExpectsArgument(v1, nameof(v1), TaggingUtilities.ReserveTag(0x238505a2 /* tag_97qw8 */));

			return (v1.CompareTo(v2) < 0);
		}


		/// <summary>
		/// Determines whether the first specified object is less than or equal
		/// to the second specified object.
		/// </summary>
		/// <param name="v1">The first object.</param>
		/// <param name="v2">The second object.</param>
		/// <returns>True if v1 is less than or equal to v2; false otherwise.</returns>
		public static bool operator <=(ProductVersion v1, ProductVersion v2)
		{
			Code.ExpectsArgument(v1, nameof(v1), TaggingUtilities.ReserveTag(0x238505a3 /* tag_97qw9 */));

			return (v1.CompareTo(v2) <= 0);
		}


		/// <summary>
		/// Determines whether two specified objects are equal.
		/// </summary>
		/// <param name="v1">The first object.</param>
		/// <param name="v2">The second object.</param>
		/// <returns>True if v1 equals v2; false otherwise.</returns>
		public static bool operator ==(ProductVersion v1, ProductVersion v2)
		{
			if (Object.ReferenceEquals(v1, null))
			{
				return Object.ReferenceEquals(v2, null);
			}

			return v1.Equals(v2);
		}


		/// <summary>
		/// Determines whether the first specified object is greater than
		/// the second specified object.
		/// </summary>
		/// <param name="v1">The first object.</param>
		/// <param name="v2">The second object.</param>
		/// <returns>True if v1 is greater than v2; false otherwise.</returns>
		public static bool operator >(ProductVersion v1, ProductVersion v2) => (v2 < v1);


		/// <summary>
		/// Determines whether the first specified object is greater than or equal
		/// to the second specified object.
		/// </summary>
		/// <param name="v1">The first object.</param>
		/// <param name="v2">The second object.</param>
		/// <returns>True if v1 is greater than or equal to v2; false otherwise.</returns>
		public static bool operator >=(ProductVersion v1, ProductVersion v2) => (v2 <= v1);


		/// <summary>
		/// The build number
		/// </summary>
		[DataMember]
		public int? Build { get; private set; }


		/// <summary>
		/// The major version number
		/// </summary>
		[DataMember]
		public int Major { get; private set; }


		/// <summary>
		/// The minor version number
		/// </summary>
		[DataMember]
		public int Minor { get; private set; }


		/// <summary>
		/// The revision number
		/// </summary>
		[DataMember]
		public int? Revision { get; private set; }


		/// <summary>
		/// Compares the current object to a specified object and returns
		/// an indication of their relative values.
		/// </summary>
		/// <param name="version">An object to compare, or null.</param>
		/// <returns>A signed integer that indicates the relative values of the two objects.
		/// Less than zero: The current object is a version before version.
		/// Zero:The current object is the same version as version.
		/// Greater than zero: The current object is a version subsequent to version,
		/// or version is null.</returns>
		public int CompareTo(object version) => CompareTo(version as ProductVersion);


		/// <summary>
		/// Compares the current object to a specified object and returns
		/// an indication of their relative values.
		/// </summary>
		/// <param name="value">An object to compare, or null.</param>
		/// <returns>A signed integer that indicates the relative values of the two objects.
		/// Less than zero: The current object is a version before value.
		/// Zero:The current object is the same version as value.
		/// Greater than zero: The current object is a version subsequent to value,
		/// or value is null.</returns>
		public int CompareTo(ProductVersion value)
		{
			if (value == null)
			{
				return 1;
			}

			if (Major != value.Major)
			{
				return (Major > value.Major) ? 1 : -1;
			}

			if (Minor != value.Minor)
			{
				return (Minor > value.Minor) ? 1 : -1;
			}

			int build = Build.GetValueOrDefault();
			int valueBuild = value.Build.GetValueOrDefault();
			if (build != valueBuild)
			{
				return (build > valueBuild) ? 1 : -1;
			}

			int revision = Revision.GetValueOrDefault();
			int valueRevision = value.Revision.GetValueOrDefault();
			if (revision != valueRevision)
			{
				return (revision > valueRevision) ? 1 : -1;
			}

			return 0;
		}


		/// <summary>
		/// Returns a value indicating whether the current object and
		/// a specified object represent the same value.
		/// </summary>
		/// <param name="obj">An object to compare to the current object, or null.</param>
		/// <returns>True if every component of the current object matches
		/// the corresponding component of the obj parameter; otherwise, false</returns>
		public override bool Equals(object obj) => Equals(obj as ProductVersion);


		/// <summary>
		/// Returns a value indicating whether the current object and
		/// a specified object represent the same value.
		/// </summary>
		/// <param name="value">An object to compare to the current object, or null.</param>
		/// <returns>True if every component of the current object matches
		/// the corresponding component of the value parameter; otherwise, false</returns>
		public bool Equals(ProductVersion value)
		{
			if (value == null)
			{
				return false;
			}

			// check that major, minor, build & revision numbers match
			if ((Major != value.Major) ||
				(Minor != value.Minor) ||
				(Build.GetValueOrDefault() != value.Build.GetValueOrDefault()) ||
				(Revision.GetValueOrDefault() != value.Revision.GetValueOrDefault()))
			{
				return false;
			}

			return true;
		}


		/// <summary>
		/// Returns a hash code for the current object.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			// Let's assume that most version numbers will be pretty small and just
			// OR some lower order bits together.

			int accumulator = 0;

			accumulator |= (Major & 0x0000000F) << 28;
			accumulator |= (Minor & 0x000000FF) << 20;
			accumulator |= (Build.GetValueOrDefault() & 0x000000FF) << 12;
			accumulator |= (Revision.GetValueOrDefault() & 0x00000FFF);

			return accumulator;
		}


		/// <summary>
		/// Converts the value of the current object to its equivalent string representation.
		/// </summary>
		/// <returns>The string representation of the values of the major, minor, build,
		/// and revision components of the current object, as depicted in the following format.
		/// Each component is separated by a period character ('.').</returns>
		public override string ToString()
		{
			if (!Build.HasValue)
			{
				return string.Concat(Major, ".", Minor);
			}
			else if (!Revision.HasValue)
			{
				return (string.Concat(Major, ".", Minor, ".", Build));
			}
			else
			{
				return (string.Concat(Major, ".", Minor, ".", Build, ".", Revision));
			}
		}


		/// <summary>
		/// Parse specified string into ProductVersion instance.
		/// </summary>
		/// <param name="version">A string containing the major, minor, build, and revision
		/// numbers, where each number is delimited with a period character ('.').</param>
		/// <returns>ProductVersion or null if version cannot be parsed</returns>
		public static ProductVersion Parse(string version)
		{
			if (TryParse(version, out ProductVersion result))
			{
				return result;
			}

			return null;
		}


		/// <summary>
		/// Tries to convert the string representation of a version number to an equivalent
		/// ProductVersion object, and returns a value that indicates whether the
		/// conversion succeeded.
		/// </summary>
		/// <param name="input">A string that contains a version number to convert.</param>
		/// <param name="result">ProductVersion if conversion succeeded; null otherwise</param>
		/// <returns>True if the input parameter was converted successfully; otherwise, false.</returns>
		public static bool TryParse(string input, out ProductVersion result)
		{
			result = null;

			if (int.TryParse(input, out int major) && major >= 0)
			{
				result = new ProductVersion(major, 0);
			}
			else if (Version.TryParse(input, out Version version))
			{
				if (version.Build == -1)
				{
					result = new ProductVersion(version.Major, version.Minor);
				}
				else if (version.Revision == -1)
				{
					result = new ProductVersion(version.Major, version.Minor, version.Build);
				}
				else
				{
					result = new ProductVersion(version.Major, version.Minor, version.Build, version.Revision);
				}
			}

			return result != null;
		}
	}
}