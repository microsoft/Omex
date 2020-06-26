// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Extensions for dealing with legacy Tag IDs.
	/// </summary>
	[Obsolete("Added for backward compatability, we are in a process of changing tagging system, please avoid using it if posible", false)]
	public static class TagsExtensions
	{
		/// <summary>
		/// Get the Tag id as a string, from an <see cref="EventId"/>.
		/// Please avoid using this method if posible, it's added to support old tag scenario and made internal for unit tests
		/// </summary>
		/// <param name="eventId">The event ID.</param>
		/// <returns>the tag as string</returns>
		/// <remarks>
		/// In terms of the conversion from integer tag value to equivalent string reprsentation, the following scheme is used:
		/// 1. If the integer tag &lt;= 0x0000FFFF, treat the tag as special tag called numeric only tag.
		/// Hence the string representation is direct conversion i.e. tag id 6700 == 6700
		/// 2. Else, if it's an alphanumeric tag, there are 2 different schemes to pack those. viz. 4 letter and 5 letter representations.
		/// 2.1 four letter tags are converted by transforming each byte into it's equivalent ASCII. e.g. 0x61626364 => abcd
		/// 2.2 five letter tags are converted by transforming lower 30 bits of the integer value into the symbol space a-z,0-9.
		/// The conversion is done by treating each group of 6 bits as an index into the symbol space a,b,c,d, ... z, 0, 1, 2, ....9
		/// eg. 0x000101D0 = 00 000000 000000 010000 000111 010000 2 = aaqhq
		/// </remarks>
		public static string ToTagId(this EventId eventId)
		{
			int tagId = eventId.Id;

			if (tagId <= 0xFFFF)
			{
				// Use straight numeric values
				return tagId.ToString("x4", CultureInfo.InvariantCulture);
			}
			else if (tagId <= 0x3FFFFFFF)
			{
				// Use the lower 30 bits, grouped into 6 bits each, index into
				// valuespace 'a'-'z','0'-'9' (reverse order)
				char[] chars = new char[5];
				for (int i = 4; i >= 0; i--)
				{
					int charVal = tagId & 0x3F;
					tagId = tagId >> 6;
					if (charVal > 25)
					{
						if (charVal > 35)
						{
							chars[i] = '?';
						}
						else
						{
							chars[i] = (char)(charVal + 22);
						}
					}
					else
					{
						chars[i] = (char)(charVal + 'a');
					}
				}

				return new string(chars);
			}
			else
			{
				// Each byte represented as ASCII (reverse order)
				byte[] bytes = BitConverter.GetBytes(tagId);
				char[] characters = Encoding.ASCII.GetChars(bytes);
				if (characters.Length > 0)
				{
					Array.Reverse(characters);
					return new string(characters);
				}
			}

			return "0000";
		}
	}
}
