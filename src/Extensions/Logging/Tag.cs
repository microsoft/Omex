// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Tagging utitilty class
	/// </summary>
	public static class Tag
	{
		/// <summary>
		/// Create a EventId using a tag using GitTagger
		/// </summary>
		public static EventId ReserveTag(int tagId) => new EventId(tagId);


		/// <summary>
		/// Create a EventId tag using CallerFilePath and CallerLineNumber
		/// </summary>
		public static EventId Create(
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
			new EventId(sourceLineNumber, sourceFilePath);
	}
}
