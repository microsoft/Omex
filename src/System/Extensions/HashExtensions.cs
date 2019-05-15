// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Extensions
{
	/// <summary>
	/// Extensions class for hashing methods
	/// </summary>
	public static class HashExtensions
	{
		/// <summary>
		/// Gets the hash of a byte array instance.
		/// </summary>
		/// <typeparam name="T">Type of hashing algorithm to use.</typeparam>
		/// <param name="instance">The byte array instance.</param>
		/// <param name="convertToHexString">Indicator to convert the hash to hex string.</param>
		/// <returns>Hash as a Base64 string.</returns>
		public static string GetHash<T>(this byte[] instance, bool convertToHexString = false) where T : HashAlgorithm, new()
		{
			if (!Code.ValidateArgument(instance, nameof(instance), TaggingUtilities.ReserveTag(0)))
			{
				return null;
			}

			using (T hashingAlgorithm = new T())
			{
				byte[] hash = hashingAlgorithm.ComputeHash(instance);
				return ConvertToString(hash, convertToHexString);
			}
		}


		/// <summary>
		/// Converts byte array to string.
		/// </summary>
		/// <param name="input">The input byte array.</param>
		/// <param name="convertToHexString">Indicator to convert the hash to hex string if true, Base64 otherwise.</param>
		/// <returns>Converted string.</returns>
		private static string ConvertToString(byte[] input, bool convertToHexString = false)
		{
			if (!Code.ValidateArgument(input, nameof(input), TaggingUtilities.ReserveTag(0)))
			{
				return null;
			}

			return convertToHexString ? ConvertToHexString(input) : Convert.ToBase64String(input);
		}


		/// <summary>
		/// Converts byte array to hex string.
		/// </summary>
		/// <param name="input">The input byte array.</param>
		/// <returns>Hex string.</returns>
		private static string ConvertToHexString(byte[] input)
		{
			StringBuilder output = new StringBuilder();

			foreach (byte data in input)
			{
				output.Append(data.ToString("x2", CultureInfo.InvariantCulture));
			}

			return output.ToString();
		}
	}
}