// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Tagging utitilty class
	/// </summary>
	public static class Tag
	{
		/// <summary>
		/// Create an EventId using a tag using GitTagger
		/// </summary>
		public static EventId ReserveTag(int tagId) => new EventId(tagId);

		/// <summary>
		/// Create an EventId tag using CallerFilePath and CallerLineNumber
		/// </summary>
		public static EventId Create(
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0) =>
				new EventId(sourceLineNumber, sourceFilePath);
	}
}
