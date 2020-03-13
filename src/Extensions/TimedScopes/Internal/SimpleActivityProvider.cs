// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.ReplayableLogs;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal sealed class SimpleActivityProvider : IActivityProvider
	{
		public Activity Create(TimedScopeDefinition definition) => new Activity(definition.Name);
	}
}
