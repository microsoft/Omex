// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Activities
{
	/// <summary>
	/// An internal interface to allow dependency injection to select between <see cref="ActivityEventSender"/> and
	/// <see cref="ScrubbedActivityEventSender"/>.
	/// </summary>
	internal interface IActivityEventSender : IActivitiesEventSender
	{
	}
}
