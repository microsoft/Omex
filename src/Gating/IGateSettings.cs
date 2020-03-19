// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Configuration settings for Gates.
	/// </summary>
	public interface IGateSettings
	{
		/// <summary>
		/// GatesOverrideEnabled Setting.
		/// </summary>
		/// <value>Setting as a set of gate names which are to be enabled.</value>
		ISet<string> GatesOverrideEnabled { get; }

		/// <summary>
		/// GatesOverrideDisabled Setting.
		/// </summary>
		/// <value>Setting as a set of gate names which are to be disabled.</value>
		ISet<string> GatesOverrideDisabled { get; }

		/// <summary>
		/// GatesToggleEnable Setting.
		/// </summary>
		/// <value>Setting as a set of gate names that will be toggled to be active.</value>
		ISet<string> GatesToggleEnabled { get; }
	}
}
