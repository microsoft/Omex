// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Compatibility
{
	/// <summary>
	/// Simple activity provider that could be used for unit tests
	/// </summary>
	/// <remarks>It does not support custom activity providers (log replay won't work)</remarks>
	internal class SimpleScopeProvider : ITimedScopeProvider
	{
		public TimedScope Create(TimedScopeDefinition name, TimedScopeResult result = TimedScopeResult.SystemError) =>
			new TimedScope(new System.Diagnostics.Activity(name.Name), result);

		public TimedScope CreateAndStart(TimedScopeDefinition name, TimedScopeResult result = TimedScopeResult.SystemError) =>
			Create(name, result).Start();
	}
}
