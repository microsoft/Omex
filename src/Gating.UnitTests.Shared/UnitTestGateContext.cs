// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Omex.Gating.UnitTests.Shared
{
	/// <summary>
	/// The unit test gate context.
	/// </summary>
	public class UnitTestGateContext : IGateContext
	{
		/// <summary>
		/// The gated request.
		/// </summary>
		public IGatedRequest Request { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether <see cref="IsGateApplicable"/> should always
		/// return <c>true</c>.
		/// </summary>
		public bool AlwaysReturnApplicable { get; set; }

		/// <summary>
		/// A flag when set to true returns all gates to be applicable for the mock gate context.
		/// </summary>
		/// <param name="gate">The gate.</param>
		/// <returns><c>true</c> if the gate is applicable; <c>false</c> otherwise.</returns>
		public bool IsGateApplicable(IGate gate)
		{
			if (AlwaysReturnApplicable || gate == null)
			{
				return true;
			}

			if (ApplicableGates == null)
			{
				return false;
			}

			return ApplicableGates.Contains(gate.Name, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Enter the gate scope.
		/// </summary>
		/// <param name="gate">The gate.</param>
		public void EnterScope(IGate gate)
		{
			if (ActivatedGates == null)
			{
				ActivatedGates = new List<IGate>();
			}

			((List<IGate>)ActivatedGates).Add(gate);
		}

		/// <summary>
		/// Exit the gate scope.
		/// </summary>
		/// <param name="gate">The gate.</param>
		public void ExitScope(IGate gate)
		{
		}

		/// <summary>
		/// The list of current gates.
		/// </summary>
		public IEnumerable<string> CurrentGates { get; set; }

		/// <summary>
		/// The list of activated gates.
		/// </summary>
		public IEnumerable<IGate> ActivatedGates { get; set; }

		/// <summary>
		/// The list of applicable gates.
		/// </summary>
		public IEnumerable<string> ApplicableGates { get; set; }
	}
}
