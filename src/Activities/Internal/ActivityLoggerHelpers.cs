// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Text;

namespace Microsoft.Omex.Extensions.Activities
{
	internal static class ActivityLoggerHelpers
	{
		public static StringBuilder AppendObjStart(this StringBuilder builder) => builder.Append('{');

		public static StringBuilder AppendObjEnd(this StringBuilder builder) => builder.Append('}');

		public static StringBuilder AppendArrayStart(this StringBuilder builder) => builder.Append('[');

		public static StringBuilder AppendArrayEnd(this StringBuilder builder) => builder.Append(']');

		public static StringBuilder AppendSeparator(this StringBuilder builder) => builder.Append(',');

		public static StringBuilder AppendParamName(this StringBuilder builder, string name) =>
			builder.Append(name).Append(':');

		/// <summary>
		/// Appends collection of key value pairs to StringBuilder, by calling ToString on key and value
		/// </summary>
		/// <remarks>
		/// TODO: should be replaced by AppendJoin when we remove netstandard target, since KeyValuePair also overrides ToString
		/// </remarks>
		public static StringBuilder AppendPairs<T>(this StringBuilder builder, IEnumerable<KeyValuePair<string, T>> pairs)
		{
			builder.AppendArrayStart();

			bool isFirst = true;
			foreach (KeyValuePair<string, T> pair in pairs)
			{
				if (!isFirst)
				{
					builder.AppendSeparator();
				}

				builder
					.AppendObjStart()
					.AppendParamName(pair.Key)
					.Append(pair.Value)
					.AppendObjEnd();
			}

			builder.AppendArrayEnd();

			return builder;
		}
	}
}
