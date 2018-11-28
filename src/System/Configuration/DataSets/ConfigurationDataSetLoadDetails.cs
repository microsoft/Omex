/***************************************************************************************************
	ConfigurationDataSetLoadDetails.cs

	The load details for a configuration data set.
***************************************************************************************************/

#region Using directives

using System;
using System.Globalization;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

#endregion

namespace Microsoft.Omex.System.Configuration.DataSets
{
	/// <summary>
	/// The load details for a configuration data set.
	/// </summary>
	public class ConfigurationDataSetLoadDetails
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationDataSetLoadDetails"/> class.
		/// </summary>
		/// <param name="name">The file name.</param>
		/// <param name="lastWrite">The last write time for the file.</param>
		/// <param name="length">The length of the file in bytes.</param>
		/// <param name="sha256Hash">The SHA256 hash of the file.</param>
		public ConfigurationDataSetLoadDetails(string name, DateTime lastWrite, long length, string sha256Hash)
		{
			Name = Code.ExpectsNotNullOrWhiteSpaceArgument(name, nameof(name), TaggingUtilities.ReserveTag(0x238506d5 /* tag_97q1v */));

			LastWrite = lastWrite;

			Code.Expects<ArgumentException>(length > 0,
				string.Format(CultureInfo.InvariantCulture, "'{0}' must be greater than zero but is '{1}'.", nameof(length), length),
				TaggingUtilities.ReserveTag(0x238506d7 /* tag_97q1x */));
			Length = length;

			SHA256Hash = Code.ExpectsNotNullOrWhiteSpaceArgument(sha256Hash, nameof(sha256Hash), 0x238506d8 /* tag_97q1y */);
			Code.Expects<ArgumentException>(sha256Hash.Length == 44,
				string.Format(CultureInfo.InvariantCulture, "'{0}' should be 44 bytes long but is '{1}' which is '{2}' bytes long.", nameof(sha256Hash), sha256Hash, sha256Hash.Length),
				TaggingUtilities.ReserveTag(0x238506d9 /* tag_97q1z */));
			Code.Expects<ArgumentException>(sha256Hash.EndsWith("=", StringComparison.Ordinal),
				string.Format(CultureInfo.InvariantCulture, "'{0}' should end with = but is '{1}'.", nameof(sha256Hash), sha256Hash),
				TaggingUtilities.ReserveTag(0x238506da /* tag_97q10 */));
		}


		/// <summary>
		/// Gets the file name.
		/// </summary>
		private string Name { get; }


		/// <summary>
		/// Gets the last write time for the file.
		/// </summary>
		private DateTime LastWrite { get; }


		/// <summary>
		/// Gets the length of the file in bytes.
		/// </summary>
		private long Length { get; }


		/// <summary>
		/// The SHA256 hash of the file.
		/// </summary>
		private string SHA256Hash { get; }


		/// <summary>
		/// Returns a <see cref="string"/> that represents this instance.
		/// </summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Name: '{0}'. Last write: '{1}'. Length: '{2}' bytes. SHA256 hash: '{3}'.",
				Name, LastWrite, Length, SHA256Hash);
		}
	}
}
