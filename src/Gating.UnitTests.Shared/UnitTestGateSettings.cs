// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.Gating.UnitTests.Shared
{
	/// <summary>
	/// Mock implementaion of IGateSettings.
	/// </summary>
	public class UnitTestGateSettings : IGateSettings
	{
		/// <summary>
		/// GatesOverrideEnabled Setting.
		/// </summary>
		private readonly List<string> m_gatesOverrideEnabled;


		/// <summary>
		/// GatesOverrideDisabled Setting.
		/// </summary>
		private readonly List<string> m_gatesOverrideDisabled;


		/// <summary>
		/// GatesToggleEnable Setting.
		/// </summary>
		private readonly List<string> m_gatesToggleEnabled;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="gatesOverrideEnabled">GatesOverrideEnabled</param>
		/// <param name="gatesOverrideDisabled">GatesOverrideDisabled</param>
		/// <param name="gatesToggleEnabled">GatesToggleEnabled</param>
		public UnitTestGateSettings(List<string> gatesOverrideEnabled = null, List<string> gatesOverrideDisabled = null, List<string> gatesToggleEnabled = null)
		{
			m_gatesOverrideEnabled = gatesOverrideEnabled ?? new List<string>();
			m_gatesOverrideDisabled = gatesOverrideDisabled ?? new List<string>();
			m_gatesToggleEnabled = gatesToggleEnabled ?? new List<string>();
		}


		/// <summary>
		/// GatesOverrideEnabled Setting.
		/// </summary>
		/// <value>IOsiSetting as a list of gate names which are to be enabled.</value>
		public ISet<string> GatesOverrideEnabled => new HashSet<string>(m_gatesOverrideEnabled);


		/// <summary>
		/// GatesOverrideDisabled Setting.
		/// </summary>
		/// <value>IOsiSetting as a list of gate names which are to be disabled.</value>
		public ISet<string> GatesOverrideDisabled => new HashSet<string>(m_gatesOverrideDisabled);


		/// <summary>
		/// GatesToggleEnable Setting.
		/// </summary>
		/// <value>IOsiSetting as a list of gate names that will be toggled to be active.</value>
		public ISet<string> GatesToggleEnabled => new HashSet<string>(m_gatesToggleEnabled);
	}
}