// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.Gating.Experimentation;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// A gate defines a set of restrictions that a request
	/// must match in order for the code that belongs to a gate to be active. The IGateContext
	/// is used to determine which gates are applicable for a gated request (IGatedRequest).
	/// GatedCode/GatedAction/GatedFunc can be used together with the IGateContext extension
	/// methods to conditionally perform a code snippet.
	/// </summary>
	public interface IGate
	{
		/// <summary>
		/// The name of the current gate
		/// </summary>
		string Name { get; }


		/// <summary>
		/// Fully qualified name, including parent Gate and Experiment information if applicable.
		/// </summary>
		string FullyQualifiedName { get; }


		/// <summary>
		/// Unique lookup key for this gate.
		/// </summary>
		int Key { get; }


		/// <summary>
		/// Any parent gate this gate inherits from
		/// </summary>
		/// <remarks>Each gate can have one parent gate, which in turn can have another parent gate etc.
		/// This allows for a hierarchy of gates, which in turn allows configuration of which gates are applicable
		/// for a request to be centralized in a parent gate. Note that the gates do not keep track
		/// of their "child" gates.</remarks>
		IGate ParentGate { get; }


		/// <summary>
		/// Set of applicable users for the gate
		/// </summary>
		HashSet<string> Users { get; }


		/// <summary>
		/// User group types applicable to the gate
		/// </summary>
		UserGroupTypes UserTypes { get; }


		/// <summary>
		/// Set of markets that apply for this gate
		/// </summary>
		HashSet<string> Markets { get; }


		/// <summary>
		/// Set of environments that apply for this gate
		/// </summary>
		HashSet<string> Environments { get; }


		/// <summary>
		/// Set of client versions that apply for this gate
		/// </summary>
		IDictionary<string, RequiredClient> ClientVersions { get; }


		/// <summary>
		/// A secure gate is a gate that cannot be requested using
		/// the name of the gate
		/// </summary>
		bool IsSecureGate { get; }


		/// <summary>
		/// A toggle that enables/disables a gate.
		/// </summary>
		bool IsGateEnabled { get; }


		/// <summary>
		/// Set of query parameters that make this gate not applicable
		/// </summary>
		IDictionary<string, HashSet<string>> BlockedQueryParameters { get; }


		/// <summary>
		/// Set of host environments that apply for this gate
		/// </summary>
		HashSet<string> HostEnvironments { get; }


		/// <summary>
		/// Set of known IP ranges that apply for this gate
		/// </summary>
		HashSet<string> KnownIPRanges { get; }


		/// <summary>
		/// Set of browser type and corresponding versions that are allowed for this gate.
		/// </summary>
		IDictionary<string, HashSet<int>> AllowedBrowsers { get; }


		/// <summary>
		/// Set of browser type and corresponding versions for which this gate will be blocked.
		/// </summary>
		IDictionary<string, HashSet<int>> BlockedBrowsers { get; }


		/// <summary>
		/// Set of services and corresponding service flag.
		/// </summary>
		IDictionary<string, GatedServiceTypes> Services { get; }


		/// <summary>
		/// Gets all the release gates for a release plan.
		/// </summary>
		IGate[] ReleasePlan { get; }


		/// <summary>
		/// If the gate is a release gate, name of the gate with the release plan containing this gate.
		/// </summary>
		string RepleasePlanGateName { get; }


		/// <summary>
		/// Start Date of the gate
		/// </summary>
		DateTime? StartDate { get; }


		/// <summary>
		/// End Date of the gate
		/// </summary>
		DateTime? EndDate { get; }


		/// <summary>
		/// Set of cloud contexts that apply to this gate
		/// </summary>
		HashSet<string> CloudContexts { get; }


		/// <summary>
		/// Information about the experiment
		/// </summary>
		/// <remarks> it is null for the non experimental gate</remarks>
		IExperimentInfo ExperimentInfo { get; }


		/// <summary>
		/// Returns true if any of the markets includes a specified region.
		/// </summary>
		/// <param name="region">Region to check.</param>
		/// <returns>True if any of the markets includes a specified region; false otherwise.</returns>
		bool IncludesRegion(string region);
	}
}
