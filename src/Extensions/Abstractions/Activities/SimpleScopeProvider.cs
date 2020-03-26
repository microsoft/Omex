// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Simple activity provider that could be used for unit tests
	/// </summary>
	/// <remarks>It does not support custom activity providers (log replay won't work)</remarks>
	public class SimpleScopeProvider : ITimedScopeProvider
	{
		/// <inheritdoc />
		public TimedScope Create(TimedScopeDefinition name, TimedScopeResult result = TimedScopeResult.SystemError) =>
			new TimedScope(new System.Diagnostics.Activity(name.Name), result);

		/// <inheritdoc />
		public TimedScope CreateAndStart(TimedScopeDefinition name, TimedScopeResult result = TimedScopeResult.SystemError) =>
			Create(name, result).Start();
	}
}
