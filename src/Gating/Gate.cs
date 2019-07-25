// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Omex.Gating.Experimentation;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Base implementation of a gate
	///
	/// A gate defines a set of restrictions that a request
	/// must match in order for the code that belongs to a gate to be active. The IGateContext
	/// is used to determine which gates are applicable for a gated request (IGatedRequest).
	/// GatedCode/GatedAction/GatedFunc can be used together with the IGateContext extension
	/// methods to conditionally perform a code snippet.
	/// </summary>
	public class Gate : IGate
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Gate"/> class.
		/// </summary>
		/// <param name="gateName">name of gate</param>
		public Gate(string gateName)
		{
			Name = gateName;
			UserTypes = UserGroupTypes.Unspecified;
			IsGateEnabled = true;
		}


		#region IGate Members
		/// <summary>
		/// Name of gate
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// Fully qualified name, including parent Gate and Experiment information if applicable.
		/// </summary>
		public string FullyQualifiedName
		{
			get
			{
				// build FQN as '((ParentExperiment::)ParentName/)(Experiment::)Name' where () is optional.
				string experimentPrefix = ExperimentInfo == null ? string.Empty : string.Concat(ExperimentInfo.ExperimentName, "::");
				string parentPrefix = ParentGate == null ? string.Empty : string.Concat(ParentGate.FullyQualifiedName, "/");
				return string.Concat(parentPrefix, experimentPrefix, Name);
			}
		}


		/// <summary>
		/// Unique lookup key for this gate.
		/// </summary>
		public int Key => FullyQualifiedName.GetHashCode();


		/// <summary>
		/// Parent gate
		/// </summary>
		public IGate ParentGate { get; set; }


		/// <summary>
		/// Set of users that have access to the gate
		/// </summary>
		public HashSet<string> Users { get; set; }


		/// <summary>
		/// Set of user group types applicable to the gate
		/// </summary>
		public UserGroupTypes UserTypes { get; set; }


		/// <summary>
		/// Set of applicable markets
		/// </summary>
		public HashSet<string> Markets { get; set; }


		/// <summary>
		/// Set of environments that apply for this gate
		/// </summary>
		public HashSet<string> Environments { get; set; }


		/// <summary>
		/// Set of applicable client versions
		/// </summary>
		public IDictionary<string, RequiredClient> ClientVersions { get; set; }


		/// <summary>
		/// A secure gate is a gate that cannot be requested using
		/// the name of the gate
		/// </summary>
		public bool IsSecureGate { get; set; }


		/// <summary>
		/// A toggle that enables/disables a gate.
		/// </summary>
		public bool IsGateEnabled { get; set; }


		/// <summary>
		/// Set of query parameters that block this gate
		/// </summary>
		public IDictionary<string, HashSet<string>> BlockedQueryParameters { get; set; }


		/// <summary>
		/// Set of host environments (CommonEnvironmentName) that apply for this gate
		/// </summary>
		public HashSet<string> HostEnvironments { get; set; }


		/// <summary>
		/// Set of known IP ranges that apply for this gate
		/// </summary>
		public HashSet<string> KnownIPRanges { get; set; }


		/// <summary>
		/// Set of browser type and corresponding versions that are allowed for this gate.
		/// </summary>
		public IDictionary<string, HashSet<int>> AllowedBrowsers { get; set; }


		/// <summary>
		/// Set of browser type and corresponding versions for which this gate will be blocked.
		/// </summary>
		public IDictionary<string, HashSet<int>> BlockedBrowsers { get; set; }


		/// <summary>
		/// Set of services and corresponding service flag.
		/// </summary>
		public IDictionary<string, GatedServiceTypes> Services { get; set; }


		/// <summary>
		/// Gets all the release gates for a release plan.
		/// </summary>
		public IGate[] ReleasePlan { get; set; }


		/// <summary>
		/// Start Date of the gate
		/// </summary>
		public DateTime? StartDate { get; set; }


		/// <summary>
		/// If the gate is a release gate, name of the gate with the release plan containing this gate.
		/// </summary>
		public string RepleasePlanGateName { get; set; }


		/// <summary>
		/// End Date of the gate
		/// </summary>
		public DateTime? EndDate { get; set; }


		/// <summary>
		/// Set of cloud contexts that apply to this gate
		/// </summary>
		public HashSet<string> CloudContexts { get; set; }


		/// <summary>
		/// Information about the experiment
		/// </summary>
		/// <remarks> it is null for the non experimental gate</remarks>
		public IExperimentInfo ExperimentInfo { get; set; }


		/// <summary>
		/// Returns true if any of the markets includes a specified region.
		/// </summary>
		/// <param name="region">Region to check.</param>
		/// <returns>True if any of the markets includes a specified region; false otherwise.</returns>
		public bool IncludesRegion(string region)
		{
			if (Markets == null || Markets.Count == 0)
			{
				// there is no restriction on the market
				return true;
			}

			return Markets.Any(m => m.Split(s_marketDelimiters, StringSplitOptions.RemoveEmptyEntries).Last().Equals(
				region, StringComparison.OrdinalIgnoreCase));
		}

		#endregion


		/// <summary>
		/// Market name delimiters.
		/// </summary>
		private static readonly char[] s_marketDelimiters = new[] { '-' };


		/// <summary>
		/// The wildcard to indicate any value of a query parameter should be blocked
		/// </summary>
		public const string BlockedQueryParameterValueWildCard = "*";
	}
}
