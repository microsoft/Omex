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
	}
}
