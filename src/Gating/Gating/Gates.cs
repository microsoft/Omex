/**************************************************************************************************
	Gates.cs

	Class containing all known gates
**************************************************************************************************/

#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Omex.Gating.Experimentation;
using Microsoft.Omex.Gating.Extensions;
using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Logging;

#endregion

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// All known gates
	/// </summary>
	public class Gates
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Gates"/> class.
		/// </summary>
		/// <param name="loader">GateDataSet loader</param>
		public Gates(IConfigurationDataSetLoader<GateDataSet> loader) => m_gateDataSetLoader = loader;


		#region Gates retrieval

		/// <summary>
		/// All gate names
		/// </summary>
		public IEnumerable<string> GateNames => GateDataSet?.GateNames ?? Enumerable.Empty<string>();


		/// <summary>
		/// Get a gate by name
		/// </summary>
		/// <param name="gateName">name of the gate</param>
		/// <returns>found gate, null if a gate could not be found</returns>
		public IGate GetGate(string gateName)
		{
			if (string.IsNullOrWhiteSpace(gateName))
			{
				ULSLogging.LogTraceTag(0x2384e50d /* tag_97oun */, Categories.GateSelection,
					Levels.Error, "Parameter 'gateName' must not be null or only whitespace.");
				return null;
			}

			return GateDataSet?.GetGate(gateName);
		}


		/// <summary>
		/// Lookup a gate based on its key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>Gate instance if found, otherwise null.</returns>
		public IGate GetGateByKey(int key) => GateDataSet?.GetGateByKey(key);


		/// <summary>
		/// Gate DataSet loader
		/// </summary>
		private readonly IConfigurationDataSetLoader<GateDataSet> m_gateDataSetLoader;


		/// <summary>
		/// Instance of the gate DataSet
		/// </summary>
		public IGateDataSet GateDataSet => m_gateDataSetLoader?.LoadedDataSet;


		/// <summary>
		/// Sets the data set retriever for cached gate
		/// </summary>
		public static void SetDataSetRetrieverForCachedGate(Func<IGateDataSet> retriever) => CachedGate.DataSetRetriever = retriever;
		#endregion


		#region Protected classes

		/// <summary>
		/// Protected class for gates that are derived from configuration file
		/// </summary>
		protected class CachedGate : IGate
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="CachedGate"/> class.
			/// </summary>
			/// <param name="name">name of the gate</param>
			public CachedGate(string name) => Name = name;


			/// <summary>
			/// Data set retriever, used to update the gate when the data set is re-loaded
			/// </summary>
			public static Func<IGateDataSet> DataSetRetriever { get; set; }


			/// <summary>
			/// Get the gate from the DataSet
			/// </summary>
			private IGate Gate
			{
				get
				{
					IGateDataSet dataSet = DataSetRetriever?.Invoke();

					IGate gate = m_dataSetGate;
					DateTime lastReload = m_lastReload;
					if (dataSet != null && (gate == null || lastReload < dataSet.LastReload))
					{
						// Should load the gate from the DataSet
						gate = dataSet.GetGate(Name);
						lastReload = dataSet.LastReload;
					}
					if (gate == null)
					{
						// Return an Unknown gate here, which will never be applicable by setting the user group as none
						ULSLogging.LogTraceTag(0x2384e50e /* tag_97ouo */, Categories.GateSelection, Levels.Verbose,
							"Gate '{0}' is not a known gate, creating a new disabled gate.", Name);

						Gate unknownGate = new Gate(Name);
						unknownGate.UserTypes = UserGroupTypes.None;
						gate = unknownGate;
						lastReload = dataSet != null ? dataSet.LastReload : DateTime.UtcNow;
					}
					m_dataSetGate = gate;
					m_lastReload = lastReload;

					return gate;
				}
			}


			/// <summary>
			/// The instance of the gate from the DataSet
			/// </summary>
			private IGate m_dataSetGate;


			/// <summary>
			/// The last time the gate was loaded into the DataSet
			/// </summary>
			private DateTime m_lastReload = DateTime.MinValue;


			#region IGate Members
			/// <summary>
			/// Set of client versions that apply for this gate
			/// </summary>
			public IDictionary<string, RequiredClient> ClientVersions => Gate.ClientVersions;


			/// <summary>
			/// Set of markets that apply for this gate
			/// </summary>
			public HashSet<string> Markets => Gate.Markets;


			/// <summary>
			/// Set of environments that apply for this gate
			/// </summary>
			public HashSet<string> Environments => Gate.Environments;


			/// <summary>
			/// The name of the current gate
			/// </summary>
			public string Name { get; }


			/// <summary>
			/// Fully qualified name, including parent Gate and Experiment information if applicable.
			/// </summary>
			public string FullyQualifiedName => Gate.FullyQualifiedName;


			/// <summary>
			/// Unique lookup key for this gate.
			/// </summary>
			public int Key => Gate.Key;


			/// <summary>
			/// Any parent gate this gate inherits from
			/// </summary>
			public IGate ParentGate => Gate.ParentGate;


			/// <summary>
			/// Set of applicable users for the gate
			/// </summary>
			public HashSet<string> Users => Gate.Users;


			/// <summary>
			/// Set of user group types applicable to the gate
			/// </summary>
			public UserGroupTypes UserTypes => Gate.UserTypes;


			/// <summary>
			/// A secure gate is a gate that cannot be requested using
			/// the name of the gate
			/// </summary>
			public bool IsSecureGate => Gate.IsSecureGate;


			/// <summary>
			/// A toggle that enables/disables a gate.
			/// </summary>
			public bool IsGateEnabled => Gate.IsGateEnabled;


			/// <summary>
			/// Set of host environments (CommonEnvironmentName) that apply for this gate
			/// </summary>
			public HashSet<string> HostEnvironments => Gate.HostEnvironments;


			/// <summary>
			/// Set of services and corresponding service flag.
			/// </summary>
			public IDictionary<string, GatedServiceTypes> Services => Gate.Services;


			/// <summary>
			/// Set of known IP ranges that apply for this gate
			/// </summary>
			public HashSet<string> KnownIPRanges => Gate.KnownIPRanges;


			/// <summary>
			/// Set of browsers and corresponding versions that are allowed for this gate.
			/// </summary>
			public IDictionary<UserAgentBrowser, HashSet<int>> AllowedBrowsers => Gate.AllowedBrowsers;


			/// <summary>
			/// Set of browsers and corresponding versions for which this gate will be blocked.
			/// </summary>
			public IDictionary<UserAgentBrowser, HashSet<int>> BlockedBrowsers => Gate.BlockedBrowsers;


			/// <summary>
			/// Gets all the release gates for a release plan.
			/// </summary>
			public IGate[] ReleasePlan => Gate.ReleasePlan;


			/// <summary>
			/// If the gate is a release gate, name of the gate with the release plan containing this gate.
			/// </summary>
			public string RepleasePlanGateName => Gate.RepleasePlanGateName;


			/// <summary>
			/// Returns true if any of the markets includes a specified region.
			/// </summary>
			/// <param name="region">Region to check.</param>
			/// <returns>True if any of the markets includes a specified region; false otherwise.</returns>
			public bool IncludesRegion(string region) => Gate.IncludesRegion(region);


			/// <summary>
			/// Start Date of the gate
			/// </summary>
			public DateTime? StartDate => Gate.StartDate;


			/// <summary>
			/// End Date of the gate
			/// </summary>
			public DateTime? EndDate => Gate.EndDate;


			/// <summary>
			/// Information about the experiment
			/// </summary>
			/// <remarks> it is null for the non experimental gate</remarks>
			public IExperimentInfo ExperimentInfo => Gate.ExperimentInfo;

			#endregion
		}

		#endregion
	}
}
