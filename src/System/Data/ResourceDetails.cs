/***************************************************************************************************
	ResourceDetails.cs

	The details of a loaded resource.
***************************************************************************************************/

#region Using directives

using System;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Omex.System.Extensions;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

#endregion

namespace Microsoft.Omex.System.Data
{
	/// <summary>
	/// The details of a loaded file resource.
	/// </summary>
	public class ResourceDetails : IResourceDetails
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceDetails"/> class.
		/// </summary>
		/// <param name="lastWrite">The last write time for the file.</param>
		/// <param name="length">The length of the file in bytes.</param>
		/// <param name="contents">The file contents.</param>
		public ResourceDetails(DateTime lastWrite, long length, byte[] contents)
		{
			LastWrite = lastWrite;

			Code.Expects<ArgumentException>(length >= 0,
				string.Format(CultureInfo.InvariantCulture, "'{0}' must be greater than or equal to zero but is '{1}'.", nameof(length), length),
				TaggingUtilities.ReserveTag(0x238506c5 /* tag_97q1f */));
			Length = length;

			Contents = Code.ExpectsArgument(contents, nameof(contents), TaggingUtilities.ReserveTag(0x238506c6 /* tag_97q1g */));

			SHA256Hash = contents.GetHash<SHA256CryptoServiceProvider>();
		}


		/// <summary>
		/// Gets the last write time for the file.
		/// </summary>
		public DateTime LastWrite { get; }


		/// <summary>
		/// Gets the length of the file in bytes.
		/// </summary>
		public long Length { get; }


		/// <summary>
		/// Gets the file contents.
		/// </summary>
		public byte[] Contents { get; }


		/// <summary>
		/// Gets the SHA256 hash of the file contents.
		/// </summary>
		public string SHA256Hash { get; }
	}
}
