// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes
{
	public interface ITimedScopeProvider
	{
		TimedScope Create(TimedScopeDefinition definition, TimedScopeResult initialResult, bool startScope = true);
	}
}