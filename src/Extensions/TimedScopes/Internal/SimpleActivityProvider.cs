// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal class SimpleActivityProvider : IActivityProvider
	{
		public Activity Create(string operationName) => new Activity(operationName);
	}
}
