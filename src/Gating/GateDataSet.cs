// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Omex.Gating.Authentication.Groups;
using Microsoft.Omex.Gating.Data;
using Microsoft.Omex.Gating.Experimentation;
using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Extensions;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Model.Types;
using Microsoft.Omex.System.Validation;
using GatingConfiguration = Gating.Configuration;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Gate DataSet handles reading from the gates file and manages reloading of
	/// gates as needed.
	/// </summary>
	public class GateDataSet<T> : ConfigurationDataSet<T>, IGateDataSet where T : ConfigurationDataSetLogging, new()
	{
		/// <summary>
		/// The Gate resource name
		/// </summary>
		private readonly string m_gatesResourceName;


		/// <summary>
		/// The Test Groups resource name
		/// </summary>
		private readonly string m_testGroupsResourceName;


		/// <summary>
		/// Delimiter to split version range from string
		/// </summary>
		private static readonly char[] s_versionRangeDelimiters = new char[] { ';', ',' };


		/// <summary>
		/// Delimiter to split version from version range
		/// </summary>
		private static readonly char[] s_versionDelimiters = new char[] { '-' };


		/// <summary>
		/// Initializes a new instance of the <see cref="GateDataSet"/> class.
		/// </summary>
		public GateDataSet()
			: this("OmexGates.xml", "OmexTip.xml")
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="GateDataSet"/> class.
		/// </summary>
		/// <param name="gatesResourceName">Name of the gates resource.</param>
		/// <param name="testGroupsResourceName">Name of the test groups resource.</param>
		public GateDataSet(string gatesResourceName, string testGroupsResourceName)
		{
			m_gatesResourceName = Code.ExpectsNotNullOrWhiteSpaceArgument(gatesResourceName, "gatesResourceName", TaggingUtilities.ReserveTag(0x2384d542 /* tag_97nvc */));
			m_testGroupsResourceName = Code.ExpectsNotNullOrWhiteSpaceArgument(testGroupsResourceName, "testGroupsResourceName", TaggingUtilities.ReserveTag(0x2384d543 /* tag_97nvd */));

			Experiments = new Experiments();
		}


		#region IGateDataSet Members

		/// <summary>
		/// Get a gate using the name
		/// </summary>
		/// <param name="gateName">name of gate to get</param>
		/// <returns>gate. null if a matching gate could not be found</returns>
		public virtual IGate GetGate(string gateName)
		{
			if (string.IsNullOrWhiteSpace(gateName))
			{
				ULSLogging.LogTraceTag(0x2384d544 /* tag_97nve */, Categories.GateDataSet,
					Levels.Error, true, "Parameter 'gateName' must not be null or whitespace.");
				return null;
			}

			if (m_gates != null && m_gates.TryGetValue(gateName, out IGate gate))
			{
				return gate;
			}

			return null;
		}


		/// <summary>
		/// Lookup a gate based on its key.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <returns>Gate instance if found, otherwise null.</returns>
		public virtual IGate GetGateByKey(int key)
		{
			if (m_gatesByKey != null && m_gatesByKey.TryGetValue(key, out IGate gate))
			{
				return gate;
			}

			return null;
		}


		/// <summary>
		/// Get all gate names that are part of the DataSet
		/// </summary>
		public virtual IEnumerable<string> GateNames => m_gates?.Keys ?? Enumerable.Empty<string>();


		/// <summary>
		/// Get all gates that are part of the DataSet
		/// </summary>
		public virtual IEnumerable<IGate> Gates => m_gates?.Values ?? Enumerable.Empty<IGate>();
		#endregion


		#region Member variables
		/// <summary>
		/// All gates in the DataSet
		/// </summary>
		private Dictionary<string, IGate> m_gates;


		/// <summary>
		/// All gates in the DataSet, indexed by key.
		/// </summary>
		private Dictionary<int, IGate> m_gatesByKey;


		/// <summary>
		/// List of gates which have release plan.
		/// </summary>
		private readonly HashSet<string> m_gatesWithReleasePlan = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


		/// <summary>
		/// All experiments in the DataSet
		/// </summary>
		private Dictionary<string, IExperiment> m_experiments;


		/// <summary>
		/// Gating configuration schema
		/// </summary>
		private const string GatingSchema = ResourceNames.GatingConfiguration;
		#endregion


		#region Load from xml

		/// <summary>
		/// Load the DataSet
		/// </summary>
		/// <param name="resources">Resources to load the DataSet from</param>
		/// <returns>true if load was successful, false otherwise</returns>
		public override bool Load(IDictionary<string, IResourceDetails> resources)
		{
			if (!base.Load(resources))
			{
				return false;
			}

			m_testGroupsDataSet = LoadTestGroupsDataSet(resources);

			byte[] gateContent;
			bool gateResourceStatus = LoadResource(resources, m_gatesResourceName, out gateContent);
			if (!gateResourceStatus)
			{
				ULSLogging.LogCodeErrorTag(0x2384d545 /* tag_97nvf */, Categories.GateDataSet, false,
					true, "Failed to retrieve Gates resource content for GateDataSet.");
			}
			else if (gateContent == null || gateContent.Length == 0)
			{
				ULSLogging.LogCodeErrorTag(0x2384d546 /* tag_97nvg */, Categories.GateDataSet, false,
					true, "Null or empty Gate resource data encountered in GateDataSet loading method");
			}
			else
			{
				Load(gateContent);
			}

			LastReload = DateTime.UtcNow;
			return IsHealthy;
		}


		/// <summary>
		/// Load the DataSet from an array of bytes
		/// </summary>
		/// <param name="file">array of bytes</param>
		private void Load(byte[] file)
		{
			try
			{
				using (MemoryStream stream = new MemoryStream(file, false))
				{
					GatingConfiguration.Gates gateObject = stream.Read<GatingConfiguration.Gates>(GatingSchema, null);
					if (gateObject == null)
					{
						ULSLogging.LogCodeErrorTag(0x2384d547 /* tag_97nvh */, Categories.GateDataSet, false,
							true, "Null object returned after deserializing Gating Configuration xml data.");
						return;
					}

					m_gates = LoadGates(gateObject);

					// builds a key->gate lookup.
					m_gatesByKey = new Dictionary<int, IGate>();

					foreach (IGate gate in m_gates.Values)
					{
						if (m_gatesByKey.TryGetValue(gate.Key, out IGate duplicateKeyGate))
						{
							ULSLogging.LogCodeErrorTag(0x2384d548 /* tag_97nvi */, Categories.GateDataSet, false, true,
								"Duplicate gate key '{0}' for '{1}' and '{2}'", gate.Key, gate.FullyQualifiedName,
								duplicateKeyGate.FullyQualifiedName);
						}
						else
						{
							m_gatesByKey.Add(gate.Key, gate);
						}
					}

					Experiments?.Load(m_experiments);
				}
			}
			catch (InvalidOperationException exception)
			{
				ULSLogging.ReportExceptionTag(0x2384d549 /* tag_97nvj */, Categories.GateDataSet, exception,
					true, "Failed to load gate DataSet due to exception.");
			}
		}


		/// <summary>
		/// Load the gates from the configuration
		/// </summary>
		/// <param name="configurationGates">configuration gates</param>
		/// <returns>dictionary of loaded gates</returns>
		private Dictionary<string, IGate> LoadGates(GatingConfiguration.Gates configurationGates)
		{
			if (configurationGates == null)
			{
				ULSLogging.LogCodeErrorTag(0x2384d54a /* tag_97nvk */, Categories.GateDataSet, false,
					true, "The gates configuration object is empty.");
				return null;
			}

			Dictionary<string, Gate> gates = new Dictionary<string, Gate>();
			Dictionary<string, IExperiment> experiments = new Dictionary<string, IExperiment>();
			if (configurationGates.Gate != null || configurationGates.Experiment != null)
			{
				HashSet<string> duplicateGates = null;

				// Create the gates
				if (configurationGates.Gate != null)
				{
					foreach (GatingConfiguration.GateType configurationGate in configurationGates.Gate)
					{
						if (string.IsNullOrWhiteSpace(configurationGate.Name))
						{
							ULSLogging.LogTraceTag(0x2384d54b /* tag_97nvl */, Categories.GateDataSet, Levels.Error,
								true, "The name of a gate cannot be null or whitespace, ignoring gate.");
							continue;
						}

						if (gates.ContainsKey(configurationGate.Name))
						{
							// This is a duplicated gate, should remove all gates with this name
							if (duplicateGates == null)
							{
								duplicateGates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
							}

							if (duplicateGates.Add(configurationGate.Name))
							{
								ULSLogging.LogTraceTag(0x2384d54c /* tag_97nvm */, Categories.GateDataSet, Levels.Error,
									true, "Gate '{0}' appears multiple times. Each gate must be unique.",
									configurationGate.Name);
							}
							continue;
						}

						Gate gate = ReadGate(configurationGate);

						ReadReleasePlan(configurationGate, gate);

						gates[gate.Name] = gate;
					}
				}

				//Create the experimental gates
				if (configurationGates.Experiment != null)
				{
					foreach (GatingConfiguration.GatesExperiment experiment in configurationGates.Experiment)
					{
						if (string.IsNullOrWhiteSpace(experiment.Name))
						{
							ULSLogging.LogTraceTag(0x2384d54d /* tag_97nvn */, Categories.GateDataSet, Levels.Error,
								true, "The name of an experiment cannot be null or whitespace, ignoring experiment.");
							continue;
						}

						foreach (GatingConfiguration.ExperimentGateType configurationExperimentGate in experiment.ExperimentGate)
						{
							if (gates.ContainsKey(configurationExperimentGate.Name))
							{
								// This is a duplicated gate, should remove all gates with this name
								if (duplicateGates == null)
								{
									duplicateGates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
								}

								if (duplicateGates.Add(configurationExperimentGate.Name))
								{
									ULSLogging.LogTraceTag(0x2384d54e /* tag_97nvo */, Categories.GateDataSet, Levels.Error,
										true, "Gate '{0}' appears multiple times. Each gate must be unique.",
										configurationExperimentGate.Name);
								}
								continue;
							}

							Gate gate = ReadExperimentalGate(configurationExperimentGate, experiment.Name);

							ReadReleasePlan(configurationExperimentGate, gate);

							gates[gate.Name] = gate;
						}
					}
				}

				// Remove any duplicate gates
				if (duplicateGates != null)
				{
					foreach (string duplicate in duplicateGates)
					{
						gates.Remove(duplicate);
					}
				}

				BuildParentHierarchy(configurationGates, gates);

				// Consolidate requirements throughout all gates
				foreach (Gate gate in gates.Values)
				{
					ConsolidateGates(gate, gate.ParentGate);
					ConsolidateExperimentalGates(gate, experiments);

					// Consolidate release gates contained inside a gate.
					if (m_gatesWithReleasePlan.Contains(gate.Name))
					{
						foreach (Gate releaseGate in Array.ConvertAll(gate.ReleasePlan, releaseIGate => releaseIGate as Gate))
						{
							ConsolidateGates(releaseGate, gate, true);
						}
					}
				}
			}

			Dictionary<string, IGate> loadedGates = new Dictionary<string, IGate>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, Gate> gate in gates)
			{
				loadedGates[gate.Key] = gate.Value;
			}

			m_experiments = experiments;
			return loadedGates;
		}


		/// <summary>
		/// Consolidates the gates.
		/// </summary>
		/// <param name="gateToConsolidate">The gate to consolidate.</param>
		/// <param name="gateToConsolidateFrom">The gate to consolidate from.</param>
		/// <param name="consolidateJustOneLevel">if set to true consolidate just one level, else consolidate till parent gate is null.</param>
		private void ConsolidateGates(Gate gateToConsolidate, IGate gateToConsolidateFrom, bool consolidateJustOneLevel = false)
		{
			if (gateToConsolidate == null)
			{
				return;
			}

			while (gateToConsolidateFrom != null)
			{
				ConsolidateHostEnvironments(gateToConsolidate, gateToConsolidateFrom);

				ConsolidateKnownIPRanges(gateToConsolidate, gateToConsolidateFrom);

				ConsolidateMarkets(gateToConsolidate, gateToConsolidateFrom);

				ConsolidateEnvironments(gateToConsolidate, gateToConsolidateFrom);

				ConsolidateClients(gateToConsolidate, gateToConsolidateFrom);

				ConsolidateUsers(gateToConsolidate, gateToConsolidateFrom);

				ConsolidateBrowsers(gateToConsolidate, gateToConsolidateFrom, true);

				ConsolidateBrowsers(gateToConsolidate, gateToConsolidateFrom, false);

				ConsolidateServices(gateToConsolidate, gateToConsolidateFrom);

				ConsolidateStartDate(gateToConsolidate, gateToConsolidateFrom);

				ConsolidateEndDate(gateToConsolidate, gateToConsolidateFrom);

				if (gateToConsolidateFrom.IsSecureGate)
				{
					gateToConsolidate.IsSecureGate = true;
				}

				if (!gateToConsolidateFrom.IsGateEnabled)
				{
					gateToConsolidate.IsGateEnabled = false;
				}

				gateToConsolidateFrom = gateToConsolidateFrom.ParentGate;

				if (consolidateJustOneLevel)
				{
					break;
				}
			}
		}


		/// <summary>
		/// Consolidate the host environments of a gate and the host environments of a parent gate
		/// </summary>
		/// <param name="gate">gate</param>
		/// <param name="parent">parent gate</param>
		private static void ConsolidateHostEnvironments(Gate gate, IGate parent)
		{
			if (parent.HostEnvironments == null)
			{
				// No host environment restrictions on parent, nothing to consolidate
				return;
			}

			if (gate.HostEnvironments == null)
			{
				// Promote the parent host environment restrictions
				gate.HostEnvironments = parent.HostEnvironments;
			}
			else
			{
				// Only host environments that exist both on the parent gate and this gate are applicable
				gate.HostEnvironments.IntersectWith(parent.HostEnvironments);
			}
		}


		/// <summary>
		/// Consolidate the known IP ranges of a gate and the known IP ranges of a parent gate
		/// </summary>
		/// <param name="gate">gate</param>
		/// <param name="parent">parent gate</param>
		private static void ConsolidateKnownIPRanges(Gate gate, IGate parent)
		{
			if (parent.KnownIPRanges == null)
			{
				// No IP range restrictions on parent, nothing to consolidate
				return;
			}

			if (gate.KnownIPRanges == null)
			{
				// Promote the parent IP range restrictions
				gate.KnownIPRanges = parent.KnownIPRanges;
			}
			else
			{
				// Only IP ranges that exist both on the parent gate and this gate are applicable
				gate.KnownIPRanges.IntersectWith(parent.KnownIPRanges);
			}
		}


		/// <summary>
		/// Consolidate the users of a gate and the users of a parent gate
		/// </summary>
		/// <param name="gate">gate</param>
		/// <param name="parent">parent gate</param>
		private void ConsolidateUsers(Gate gate, IGate parent)
		{
			if (parent.UserTypes == UserGroupTypes.None)
			{
				// No user access, this gate should have no user access as well
				ULSLogging.LogTraceTag(0x2384d54f /* tag_97nvp */, Categories.GateDataSet, Levels.Verbose, true,
					"Parent gate '{0}' has UserGroupTypes set to 'None', setting to 'None' on gate '{1}' as well.",
					parent.Name ?? "<NULL>", gate.Name ?? "<NULL>");

				gate.UserTypes = UserGroupTypes.None;
				gate.Users = null;
				return;
			}
			else if (parent.UserTypes == UserGroupTypes.Unspecified)
			{
				// No user restrictions from the parent gate, nothing to consolidate
				return;
			}

			if ((parent.UserTypes & UserGroupTypes.CustomGroup) == UserGroupTypes.CustomGroup)
			{
				gate.UserTypes |= UserGroupTypes.CustomGroup;
				if (gate.Users == null)
				{
					// Promote the parent users as that will be required for access to the current gate
					gate.Users = parent.Users;
				}
				else
				{
					// Only users that are both in the parent gate and the current gate will have access
					gate.Users.IntersectWith(parent.Users);
				}
				// No users in the parent gate, nothing to consolidate
				return;
			}
		}


		/// <summary>
		/// Consolidate the clients of a gate and the clients of a parent gate
		/// </summary>
		/// <param name="gate">gate</param>
		/// <param name="parent">parent gate</param>
		private void ConsolidateClients(Gate gate, IGate parent)
		{
			if (parent.ClientVersions == null)
			{
				// No client versions in the parent gate, nothing to consolidate
				return;
			}

			if (gate.ClientVersions == null)
			{
				// There are no restrictions on the current gate, so promote the parents client restrictions
				gate.ClientVersions = parent.ClientVersions;
			}
			else
			{
				// Should only leave the minimum set of clients that have access
				HashSet<string> clientsToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (string clientName in gate.ClientVersions.Keys)
				{
					if (!parent.ClientVersions.TryGetValue(clientName, out RequiredClient parentClient))
					{
						// Doesn't exist so remove it from our DataSet
						ULSLogging.LogTraceTag(0x2384d550 /* tag_97nvq */, Categories.GateDataSet, Levels.Error,
							true, "Client '{0}' of gate '{1}' does not exist on parent gate '{2}', removing client.",
							clientName, gate.Name, parent.Name);
						clientsToRemove.Add(clientName);
						continue;
					}

					RequiredClient client = gate.ClientVersions[clientName];
					if (parentClient.MinVersion != null)
					{
						if (client.MinVersion == null || client.MinVersion < parentClient.MinVersion)
						{
							client.MinVersion = parentClient.MinVersion;
						}
					}
					if (parentClient.MaxVersion != null)
					{
						if (client.MaxVersion == null || client.MaxVersion > parentClient.MaxVersion)
						{
							client.MaxVersion = parentClient.MaxVersion;
						}
					}
					if (parentClient.VersionRanges.Count > 0 && client.VersionRanges.Count == 0)
					{
						// Inheriting only. If child client has VersionRange, keep existing.
						foreach (ProductVersionRange range in parentClient.VersionRanges)
						{
							client.AddVersionRange(range);
						}
					}

					if (parentClient.Overrides.Count > 0 && client.Overrides.Count == 0)
					{
						// Inheriting only. Consolidating each override makes unnecessary complex
						foreach (RequiredApplication requiredApp in parentClient.Overrides.Values)
						{
							client.AddOverride(requiredApp);
						}
					}
				}

				foreach (string incorrectClient in clientsToRemove)
				{
					gate.ClientVersions.Remove(incorrectClient);
				}
			}
		}


		/// <summary>
		/// Consolidate the markets of a gate and the markets of a parent gate
		/// </summary>
		/// <param name="gate">gate</param>
		/// <param name="parent">parent gate</param>
		private static void ConsolidateMarkets(Gate gate, IGate parent)
		{
			if (parent.Markets == null)
			{
				// No market restrictions on parent, nothing to consolidate
				return;
			}

			if (gate.Markets == null)
			{
				// Promote the parent market restrictions
				gate.Markets = parent.Markets;
			}
			else
			{
				// Only markets that exist both on the parent gate and this gate are applicable
				gate.Markets.IntersectWith(parent.Markets);
			}
		}


		/// <summary>
		/// Consolidate the environments of a gate and the environments of a parent gate
		/// </summary>
		/// <param name="gate">gate</param>
		/// <param name="parent">parent gate</param>
		private static void ConsolidateEnvironments(Gate gate, IGate parent)
		{
			if (parent.Environments == null)
			{
				// No environment restrictions on parent, nothing to consolidate
				return;
			}

			if (gate.Environments == null)
			{
				// Promote the parent environment restrictions
				gate.Environments = parent.Environments;
			}
			else
			{
				// Only environments that exist both on the parent gate and this gate are applicable
				gate.Environments.IntersectWith(parent.Environments);
			}
		}


		/// <summary>
		/// Consolidates the browsers and corresponding versions of a gate with that of the parent gate.
		/// </summary>
		/// <param name="gate">The gate.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="isForAllowedBrowsers">flag to check if this consolidation is for AllowerBrowses or BlockedBrowsers</param>
		private static void ConsolidateBrowsers(Gate gate, IGate parent, bool isForAllowedBrowsers)
		{
			IDictionary<string, HashSet<int>> parentGateBrowsers;
			IDictionary<string, HashSet<int>> childGateBrowsers;

			if (isForAllowedBrowsers)
			{
				parentGateBrowsers = parent.BlockedBrowsers;
				childGateBrowsers = gate.BlockedBrowsers;
			}
			else
			{
				parentGateBrowsers = parent.AllowedBrowsers;
				childGateBrowsers = gate.AllowedBrowsers;
			}

			if (parentGateBrowsers == null)
			{
				// No browser restrictions on parent, nothing to consolidate.
				return;
			}

			if (childGateBrowsers == null)
			{
				// Set the parent browser restrictions for the child gate.
				childGateBrowsers = parentGateBrowsers;
			}
			else
			{
				IDictionary<string, HashSet<int>> browsers = new Dictionary<string, HashSet<int>>();

				foreach (KeyValuePair<string, HashSet<int>> childGateBrowser in childGateBrowsers
					.Where(t => parentGateBrowsers.ContainsKey(t.Key)))
				{
					// Set of empty browser version means the gate expects all the versions for the browser.
					HashSet<int> childGateBrowserVersions = childGateBrowser.Value;
					HashSet<int> parentGateBrowserVersions = parentGateBrowsers[childGateBrowser.Key];

					if (childGateBrowserVersions.Count > 0 && parentGateBrowserVersions.Count > 0)
					{
						// If set of browser versions for both child gate and parent gate is not empty,
						// intersect the data set.
						childGateBrowserVersions.IntersectWith(parentGateBrowserVersions);
					}
					else if (parentGateBrowserVersions.Count > 0 && childGateBrowserVersions.Count == 0)
					{
						// If set of browser versions for child gate is empty and set of browser versions
						// for parent gate is not empty, set the parent browser versions as child browser versions.
						childGateBrowserVersions = parentGateBrowserVersions;
					}

					// If set of browser versions for child gate is not empty and set of browser versions for parent gate is empty,
					// or both the sets are empty, set of child browser versions remains as it is.
					browsers[childGateBrowser.Key] = childGateBrowserVersions;
				}

				childGateBrowsers = browsers;
			}

			if (isForAllowedBrowsers)
			{
				gate.BlockedBrowsers = childGateBrowsers;
			}
			else
			{
				gate.AllowedBrowsers = childGateBrowsers;
			}
		}


		/// <summary>
		/// Consolidates the services.
		/// </summary>
		/// <param name="gate">The gate.</param>
		/// <param name="parent">The parent.</param>
		private static void ConsolidateServices(Gate gate, IGate parent)
		{
			if (parent.Services == null)
			{
				// No services restriction on parent, nothing to consolidate.
				return;
			}

			if (gate.Services == null)
			{
				// No services restriction on gate, setting parents restriction to it.
				gate.Services = parent.Services;
			}

			IDictionary<string, GatedServiceTypes> mergedServices = new Dictionary<string, GatedServiceTypes>(StringComparer.OrdinalIgnoreCase);

			// Intersection of services and corresponding service flags between parent gate services and child gate services.
			foreach (KeyValuePair<string, GatedServiceTypes> parentService in
				parent.Services.Where(service => gate.Services.ContainsKey(service.Key)))
			{
				GatedServiceTypes childServceFlag = gate.Services[parentService.Key];
				mergedServices[parentService.Key] = parentService.Value & childServceFlag;
			}

			gate.Services = mergedServices;
		}


		/// <summary>
		/// Consolidate the start date of a gate and the start date of a parent gate
		/// </summary>
		/// <param name="gate">gate</param>
		/// <param name="parent">parent gate</param>
		private static void ConsolidateStartDate(Gate gate, IGate parent)
		{
			if (parent.StartDate == null)
			{
				// No start date restrictions on parent, nothing to consolidate
				return;
			}

			if (gate.StartDate == null)
			{
				// Promote the parent StartDate restrictions
				gate.StartDate = parent.StartDate;
			}
			else
			{
				// Take the later start date
				gate.StartDate = gate.StartDate > parent.StartDate ? gate.StartDate : parent.StartDate;
			}
		}


		/// <summary>
		/// Consolidate the start date of a gate and the End date of a parent gate
		/// </summary>
		/// <param name="gate">gate</param>
		/// <param name="parent">parent gate</param>
		private static void ConsolidateEndDate(Gate gate, IGate parent)
		{
			if (parent.EndDate == null)
			{
				// No End date restrictions on parent, nothing to consolidate
				return;
			}

			if (gate.EndDate == null)
			{
				// Promote the parent StartDate restrictions
				gate.EndDate = parent.EndDate;
			}
			else
			{
				// Take the earlier End date
				gate.EndDate = gate.EndDate < parent.EndDate ? gate.EndDate : parent.EndDate;
			}
		}


		/// <summary>
		/// Consolidate an experimental gate and if the gate contains a release plan, sets the experiment info in the release gates.
		/// </summary>
		/// <param name="gate">gate</param>
		/// <param name="experiments">list of experiments</param>
		private static void ConsolidateExperimentalGates(Gate gate, Dictionary<string, IExperiment> experiments)
		{
			if (gate.ExperimentInfo == null)
			{
				return;
			}

			if (!experiments.ContainsKey(gate.ExperimentInfo.ExperimentName))
			{
				experiments[gate.ExperimentInfo.ExperimentName] = new Experiment();
			}

			// If an experimental gate has a release plan, set expriment info of the gate to all the release gate.
			if (gate.ReleasePlan != null)
			{
				foreach (Gate releaseGate in Array.ConvertAll(gate.ReleasePlan, releaseIGate => releaseIGate as Gate))
				{
					if (releaseGate != null)
					{
						releaseGate.ExperimentInfo = gate.ExperimentInfo;
					}
				}
			}

			experiments[gate.ExperimentInfo.ExperimentName].Add(gate);
		}


		/// <summary>
		/// Build the hierarchy of the gates, by resolving the name of each parent gate to the actual
		/// gate in the DataSet. Any gate with missing parent gate or with a circular chain in the
		/// hierarchy is removed from the DataSet.
		/// </summary>
		/// <param name="configurationGates">gates in configuration</param>
		/// <param name="gates">loaded gates</param>
		/// <remarks>Each gate can have one parent gate, which in turn can have another parent gate etc.
		/// This allows for a hierarchy of gates, which in turn allows configuration of which gates are applicable
		/// for a request to be centralized in a parent gate.</remarks>
		private void BuildParentHierarchy(GatingConfiguration.Gates configurationGates, Dictionary<string, Gate> gates)
		{
			HashSet<string> gatesToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			// Build up the hierarchy
			if (configurationGates.Gate != null)
			{
				foreach (GatingConfiguration.GateType gate in configurationGates.Gate)
				{
					ReadParentGates(gate, gates, gatesToRemove);
				}
			}

			if (configurationGates.Experiment != null)
			{
				foreach (GatingConfiguration.GatesExperiment experiment in configurationGates.Experiment)
				{
					foreach (GatingConfiguration.ExperimentGateType gate in experiment.ExperimentGate)
					{
						ReadParentGates(gate, gates, gatesToRemove);
					}
				}
			}

			// If a gate exists in the list of missing gates, add all of the child gates
			// into the missing gates. If we have added anything into the gates to remove, then loop again
			bool moreMissingGates = true;
			while (moreMissingGates)
			{
				moreMissingGates = false;
				foreach (IGate gate in gates.Values)
				{
					if (gatesToRemove.Contains(gate.Name))
					{
						// This gate is already flagged to be removed so skip it
						continue;
					}

					HashSet<string> ancestors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					ancestors.Add(gate.Name);

					IGate ancestor = gate.ParentGate;
					while (ancestor != null)
					{
						if (gatesToRemove.Contains(ancestor.Name) ||
							!gates.ContainsKey(ancestor.Name))
						{
							// The ancestor is missing, so all gates should be removed
							foreach (string removeGate in ancestors)
							{
								moreMissingGates = gatesToRemove.Add(removeGate) || moreMissingGates;
							}
							break;
						}

						ancestors.Add(ancestor.Name);
						ancestor = ancestor.ParentGate;
					}
				}
			}

			if (gatesToRemove.Count > 0)
			{
				ULSLogging.LogTraceTag(0x2384d551 /* tag_97nvr */, Categories.GateDataSet, Levels.Error,
					true, "The following gates have missing parent gates or circular dependencies in the hierarchy, removing all: {0}",
					string.Join(", ", gatesToRemove));

				foreach (string removeGate in gatesToRemove)
				{
					gates.Remove(removeGate);
				}
			}
		}


		/// <summary>
		/// Reads the parent gate and checks if it exists and it is not experimental. Also ensures no circular dependency
		/// </summary>
		/// <param name="gate">gate whose parent to read</param>
		/// <param name="gates">loaded gates</param>
		/// <param name="gatesToRemove">gates to be removed</param>
		private void ReadParentGates(GatingConfiguration.GateType gate, Dictionary<string, Gate> gates,
			HashSet<string> gatesToRemove)
		{
			if (string.IsNullOrWhiteSpace(gate.Name))
			{
				// This is not a valid gate, so continue
				return;
			}

			if (gate.ParentGate == null)
			{
				// This gate does not have a parent gate, so continue
				return;
			}

			if (!gates.TryGetValue(gate.Name, out Gate existingGate))
			{
				// Unable to find the gate to process, it has already been discarded
				return;
			}

			if (string.IsNullOrWhiteSpace(gate.ParentGate.Name))
			{
				ULSLogging.LogTraceTag(0x2384d552 /* tag_97nvs */, Categories.GateDataSet, Levels.Error,
					true, "Gate '{0}' has a null or whitespace only parent gate, ignoring parent gate.",
					gate.Name);

				gate.ParentGate = null;
				return;
			}

			if (!gates.TryGetValue(gate.ParentGate.Name, out Gate parent))
			{
				ULSLogging.LogTraceTag(0x2384d553 /* tag_97nvt */, Categories.GateDataSet, Levels.Error,
					true, "Gate '{0}' has a parent gate '{1}' that does not exist, removing gate '{0}'.",
					gate.Name, gate.ParentGate.Name);

				// Parent gate doesn't exist, should remove this gate
				gatesToRemove.Add(gate.Name);
				return;
			}

			if (parent.ExperimentInfo != null)
			{
				ULSLogging.LogTraceTag(0x2384d554 /* tag_97nvu */, Categories.GateDataSet, Levels.Error,
					true, "Gate '{0}' has a parent gate '{1}' that is experimental, removing gate '{0}'.",
					gate.Name, gate.ParentGate.Name);

				// Parent gate is experimental, should remove this gate
				gatesToRemove.Add(gate.Name);
				return;
			}

			HashSet<string> ancestors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			ancestors.Add(existingGate.Name);

			IGate ancestor = parent;
			while (ancestor != null)
			{
				if (ancestors.Contains(ancestor.Name))
				{
					// The hierarchy contains circular dependencies, removing the gates
					ULSLogging.LogTraceTag(0x2384d555 /* tag_97nvv */, Categories.GateDataSet, Levels.Error,
						true, "The ancestor chain '{0}' for gate '{0}' contains a circular dependency, removing all gates in chain.",
						string.Join(", ", ancestors), gate.Name);

					gatesToRemove.UnionWith(ancestors);
					break;
				}
				else
				{
					ancestors.Add(ancestor.Name);
					ancestor = ancestor.ParentGate;
				}
			}
			existingGate.ParentGate = parent;
		}


		/// <summary>
		/// Reads the gate from a configuration gate
		/// </summary>
		/// <param name="configurationGate">configuration to read from</param>
		/// <returns>the gate</returns>
		private Gate ReadGate(GatingConfiguration.BaseGateType configurationGate)
		{
			Gate gate = new Gate(configurationGate.Name);

			ReadHostEnvironments(configurationGate, gate);

			ReadKnownIPRanges(configurationGate, gate);

			ReadMarkets(configurationGate, gate);

			ReadEnvironments(configurationGate, gate);

			ReadBlockedQueryParameters(configurationGate, gate);

			ReadUsers(configurationGate, gate);

			ReadClients(configurationGate, gate);

			ReadBrowsers(configurationGate, gate);

			ReadServices(configurationGate, gate);

			ReadStartDate(configurationGate, gate);

			ReadEndDate(configurationGate, gate);

			gate.IsSecureGate = configurationGate.Secure;

			gate.IsGateEnabled = configurationGate.Enabled;

			return gate;
		}


		/// <summary>
		/// Reads the experiment gate from a configuration gate
		/// </summary>
		/// <param name="configurationExperimentalGate">configuration to read from</param>
		/// <param name="experimentName">Name of the experiment.</param>
		/// <returns>the gate</returns>
		private Gate ReadExperimentalGate(GatingConfiguration.ExperimentGateType configurationExperimentalGate, string experimentName)
		{
			Gate gate = ReadGate(configurationExperimentalGate);

			uint? experimentWeight = ReadExperimentalWeight(configurationExperimentalGate, gate.Name);
			if (experimentWeight != null)
			{
				gate.ExperimentInfo = new ExperimentInfo(experimentName, experimentWeight.Value);
			}

			return gate;
		}


		/// <summary>
		/// Read the host environments from a configuration gate
		/// </summary>
		/// <param name="configurationGate">configuration to read from</param>
		/// <param name="gate">gate to add information to</param>
		private void ReadHostEnvironments(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.HostEnvironments == null)
			{
				return;
			}

			gate.HostEnvironments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (GatingConfiguration.BaseGateTypeHostEnvironment environment in configurationGate.HostEnvironments)
			{
				if (environment.Name == "None")
				{
					gate.HostEnvironments.Clear();
					break;
				}

				string environmentName = environment.Name.ToString();
				if (!gate.HostEnvironments.Add(environmentName))
				{
					ULSLogging.LogTraceTag(0x2384d556 /* tag_97nvw */, Categories.GateDataSet, Levels.Error,
						true, "Host environment '{0}' is repeated multiple times for gate '{1}'.",
						environmentName, gate.Name);
				}
			}
		}


		/// <summary>
		/// Read the known IP ranges from a configuration gate
		/// </summary>
		/// <param name="configurationGate">configuration to read from</param>
		/// <param name="gate">gate to add information to</param>
		private void ReadKnownIPRanges(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.KnownIPRanges == null)
			{
				return;
			}

			gate.KnownIPRanges = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (GatingConfiguration.BaseGateTypeKnownIPRange ipRange in configurationGate.KnownIPRanges)
			{
				if (ipRange.Name == "None")
				{
					gate.KnownIPRanges.Clear();
					break;
				}

				string ipRangeName = ipRange.Name;
				if (!gate.KnownIPRanges.Add(ipRangeName))
				{
					ULSLogging.LogTraceTag(0x2384d557 /* tag_97nvx */, Categories.GateDataSet, Levels.Error,
						true, "Known IP range '{0}' is repeated multiple times for gate '{1}'.",
						ipRangeName, gate.Name);
				}
			}
		}


		/// <summary>
		/// Reads the browsers for a gate from configuration gate.
		/// </summary>
		/// <param name="configurationGate">The configuration gate.</param>
		/// <param name="gate">The gate.</param>
		private void ReadBrowsers(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.Browsers == null)
			{
				gate.AllowedBrowsers = null;
				gate.BlockedBrowsers = null;

				return;
			}

			IDictionary<string, HashSet<int>> readBrowsers = new Dictionary<string, HashSet<int>>();

			if (configurationGate.Browsers.Item == null || configurationGate.Browsers.Item.Browser == null)
			{
				gate.AllowedBrowsers = null;
				gate.BlockedBrowsers = null;

				return;
			}

			foreach (GatingConfiguration.BrowserFormatBrowser browser in configurationGate.Browsers.Item.Browser)
			{
				string userAgentBrowser = browser.Name;
				if (!readBrowsers.ContainsKey(userAgentBrowser))
				{
					readBrowsers[userAgentBrowser] = new HashSet<int>();
				}

				if (browser.Version == null)
				{
					continue;
				}

				foreach (GatingConfiguration.BrowserFormatBrowserVersion browserVersion in browser.Version)
				{
					if (!int.TryParse(browserVersion.Value, out int version))
					{
						ULSLogging.LogTraceTag(0x2384d559 /* tag_97nvz */, Categories.GateDataSet, Levels.Error,
							false, "Unable to parse version '{0}' as integer of the browser '{1}'", browserVersion.Value, browser);

						continue;
					}

					readBrowsers[userAgentBrowser].Add(version);
				}
			}

			if (configurationGate.Browsers.ItemElementName == GatingConfiguration.ItemChoiceType.AllowedBrowsers)
			{
				gate.AllowedBrowsers = readBrowsers;
				gate.BlockedBrowsers = null;
			}
			else
			{
				gate.BlockedBrowsers = readBrowsers;
				gate.AllowedBrowsers = null;
			}
		}


		/// <summary>
		/// Reads the services for a gate from loaded configuration gate.
		/// </summary>
		/// <param name="configurationGate">The configuration gate.</param>
		/// <param name="gate">The gate.</param>
		private static void ReadServices(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.Services == null)
			{
				gate.Services = null;
				return;
			}

			gate.Services = new Dictionary<string, GatedServiceTypes>(StringComparer.OrdinalIgnoreCase);
			foreach (GatingConfiguration.BaseGateTypeService service in configurationGate.Services)
			{
				if (Enum.TryParse(service.ActiveFor.ToString(), true, out GatedServiceTypes serviceFlag))
				{
					gate.Services[service.Type.ToString()] = serviceFlag;
				}
				else
				{
					gate.Services[service.Type.ToString()] = GatedServiceTypes.None;
				}
			}
		}


		/// <summary>
		/// Reads the release plan for a gate from configuration file.
		/// </summary>
		/// <param name="configurationGate">The configuration gate.</param>
		/// <param name="gate">The gate.</param>
		private void ReadReleasePlan(GatingConfiguration.GateType configurationGate, Gate gate)
		{
			if (configurationGate.ReleasePlan == null)
			{
				gate.ReleasePlan = null;
				return;
			}

			IDictionary<string, IGate> releasePlan = new Dictionary<string, IGate>(StringComparer.OrdinalIgnoreCase);

			foreach (GatingConfiguration.BaseGateType configurationReleaseGate in configurationGate.ReleasePlan)
			{
				IGate releaseGate = ReadGate(configurationReleaseGate);

				if (releasePlan.ContainsKey(releaseGate.Name))
				{
					ULSLogging.LogTraceTag(0x2384d55a /* tag_97nv0 */, Categories.GateDataSet, Levels.Error,
						false, "ReleaseGate '{0}' is repeated multiple times in ReleasePlan for gate '{1}'",
						releaseGate.Name, gate.Name);

					continue;
				}

				((Gate)releaseGate).RepleasePlanGateName = gate.Name;
				releasePlan[releaseGate.Name] = releaseGate;
			}

			gate.ReleasePlan = new List<IGate>(releasePlan.Values).ToArray();
			m_gatesWithReleasePlan.Add(gate.Name);
		}


		/// <summary>
		/// Read the markets from a configuration gate
		/// </summary>
		/// <param name="configurationGate">configuration to read from</param>
		/// <param name="gate">gate to add information to</param>
		private void ReadMarkets(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.Markets == null)
			{
				return;
			}

			gate.Markets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (GatingConfiguration.BaseGateTypeMarket market in configurationGate.Markets)
			{
				if (string.IsNullOrWhiteSpace(market.Name))
				{
					ULSLogging.LogTraceTag(0x2384d55b /* tag_97nv1 */, Categories.GateDataSet, Levels.Error,
						true, "Market name for gate '{0}' is null or only whitespace, ignoring.",
						gate.Name);

					continue;
				}

				if (!gate.Markets.Add(market.Name))
				{
					ULSLogging.LogTraceTag(0x2384d55c /* tag_97nv2 */, Categories.GateDataSet, Levels.Error,
						true, "Market '{0}' is repeated multiple times for gate '{1}'.",
						market.Name, gate.Name);
				}
			}
		}


		/// <summary>
		/// Read the query parameters from a configuration gate
		/// </summary>
		/// <param name="configurationGate">configuration to read from</param>
		/// <param name="gate">gate to add information to</param>
		private void ReadBlockedQueryParameters(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.BlockedQueryParameters == null)
			{
				return;
			}

			gate.BlockedQueryParameters = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (GatingConfiguration.BaseGateTypeBlockedQueryParameter blockedQueryParameter in configurationGate.BlockedQueryParameters)
			{
				//Empty names are not valid
				if (string.IsNullOrWhiteSpace(blockedQueryParameter.Name))
				{
					ULSLogging.LogTraceTag(0x23849815 /* tag_97j6v */, Categories.GateDataSet, Levels.Error, true,
						"Blocked Query Parameter name for gate '{0}' is null or only whitespace, ignoring.",
						gate.Name);

					continue;
				}

				//Empty values are not valid
				if (string.IsNullOrWhiteSpace(blockedQueryParameter.Value))
				{
					ULSLogging.LogTraceTag(0x23849816 /* tag_97j6w */, Categories.GateDataSet, Levels.Error, true,
						"Blocked Query Parameter value for gate '{0}' is null or only whitespace, ignoring.",
						gate.Name);

					continue;
				}

				//If name is new, create an entry in the dictionary
				if (!gate.BlockedQueryParameters.ContainsKey(blockedQueryParameter.Name))
				{
					gate.BlockedQueryParameters.Add(blockedQueryParameter.Name, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
				}

				//Add the new name and value to the dictionary and hashset
				gate.BlockedQueryParameters[blockedQueryParameter.Name].Add(blockedQueryParameter.Value);
			}
		}


		/// <summary>
		/// Read the environments from a configuration gate
		/// </summary>
		/// <param name="configurationGate">configuration to read from</param>
		/// <param name="gate">gate to add information to</param>
		private void ReadEnvironments(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.Environments == null)
			{
				return;
			}

			gate.Environments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (GatingConfiguration.BaseGateTypeEnvironment environment in configurationGate.Environments)
			{
				if (string.IsNullOrWhiteSpace(environment.Name))
				{
					ULSLogging.LogTraceTag(0x2384d55d /* tag_97nv3 */, Categories.GateDataSet, Levels.Error, true,
						"Environment name for gate '{0}' is null or only whitespace, ignoring.",
						gate.Name);

					continue;
				}

				if (!gate.Environments.Add(environment.Name))
				{
					ULSLogging.LogTraceTag(0x2384d55e /* tag_97nv4 */, Categories.GateDataSet, Levels.Error, true,
						"Environment '{0}' is repeated multiple times for gate '{1}'.",
						environment.Name, gate.Name);
				}
			}
		}


		/// <summary>
		/// Read the clients from a configuration gate
		/// </summary>
		/// <param name="configurationGate">configuration gate</param>
		/// <param name="gate">gate to update</param>
		private void ReadClients(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.ClientVersions == null)
			{
				return;
			}

			gate.ClientVersions = new SortedDictionary<string, RequiredClient>(StringComparer.OrdinalIgnoreCase);
			foreach (GatingConfiguration.BaseGateTypeClientVersion clientVersion in configurationGate.ClientVersions)
			{
				if (string.IsNullOrWhiteSpace(clientVersion.Name))
				{
					ULSLogging.LogTraceTag(0x2384d55f /* tag_97nv5 */, Categories.GateDataSet, Levels.Error,
						true, "Gate {0} contains a client version with a null or whitespace name.",
						gate.Name);
					continue;
				}

				RequiredClient client = new RequiredClient();
				client.Name = clientVersion.Name;
				client.AudienceGroup = clientVersion.AudienceGroup;
				if (!string.IsNullOrWhiteSpace(clientVersion.MaxVersion))
				{
					if (ProductVersion.TryParse(clientVersion.MaxVersion, out ProductVersion version))
					{
						client.MaxVersion = version;
					}
					else
					{
						ULSLogging.LogTraceTag(0x2384d560 /* tag_97nv6 */, Categories.GateDataSet, Levels.Error,
							true, "Gate '{0}' contains an incorrect MaxVersion value '{1}' for client '{2}'.",
							gate.Name, clientVersion.MaxVersion, client.Name);

						client.MaxVersion = ProductVersion.MinVersion;
					}
				}

				if (!string.IsNullOrWhiteSpace(clientVersion.MinVersion))
				{
					if (ProductVersion.TryParse(clientVersion.MinVersion, out ProductVersion version))
					{
						client.MinVersion = version;
					}
					else
					{
						ULSLogging.LogTraceTag(0x2384d561 /* tag_97nv7 */, Categories.GateDataSet, Levels.Error,
							true, "Gate '{0}' contains an incorrect MinVersion value '{1}' for client '{2}'.",
							gate.Name, clientVersion.MinVersion, client.Name);

						client.MinVersion = ProductVersion.MaxVersion;
					}
				}

				if (!string.IsNullOrWhiteSpace(clientVersion.VersionRange))
				{
					// String to list by ;. This makes "A-B;C-D" to [A-B][C-D]
					foreach (string range in clientVersion.VersionRange.Split(s_versionRangeDelimiters, StringSplitOptions.RemoveEmptyEntries))
					{
						// This makes "A-B" to [A][B]. A is min, B is max
						string[] versions = range.Split(s_versionDelimiters, StringSplitOptions.RemoveEmptyEntries);
						if (versions.Length == 2)
						{
							if (ProductVersion.TryParse(versions[0], out ProductVersion minver)
								&& ProductVersion.TryParse(versions[1], out ProductVersion maxver))
							{
								ProductVersionRange versionRange = new ProductVersionRange(minver, maxver);
								client.AddVersionRange(versionRange);
							}
							else
							{
								ULSLogging.LogTraceTag(0x2384d562 /* tag_97nv8 */, Categories.GateDataSet, Levels.Error,
									true, "Gate '{0}' contains an incorrect VersionRage value '{1}' for client '{2}'.",
									gate.Name, clientVersion.VersionRange, client.Name);
							}
						}
					}
				}

				if (clientVersion.ApplicationOverride != null)
				{
					foreach (GatingConfiguration.BaseGateTypeClientVersionApplicationOverride clientVersionOverride in clientVersion.ApplicationOverride)
					{
						RequiredApplication requiredApp = ReadOverride(clientVersionOverride);

						if (requiredApp != null)
						{
							client.AddOverride(requiredApp);
						}
					}
				}

				gate.ClientVersions[client.Name] = MergeExistingClient(gate, client);
			}
		}


		/// <summary>
		/// Read the override from a configuration gate
		/// </summary>
		/// <param name="clientVersionOverride">BaseGateTypeClientVersionApplicationOverride</param>
		/// <returns>RequiredApplication</returns>
		private RequiredApplication ReadOverride(GatingConfiguration.BaseGateTypeClientVersionApplicationOverride clientVersionOverride)
		{
			if (clientVersionOverride == null)
			{
				ULSLogging.LogTraceTag(0x2384d563 /* tag_97nv9 */, Categories.GateDataSet, Levels.Error,
					true, "Override is null.");
				return null;
			}

			if (string.IsNullOrWhiteSpace(clientVersionOverride.AppCode))
			{
				ULSLogging.LogTraceTag(0x2384d580 /* tag_97nwa */, Categories.GateDataSet, Levels.Error,
					true, "Override contains AppCode with a null or whitespace.");
				return null;
			}

			RequiredApplication requiredApp = new RequiredApplication
			{
				Name = clientVersionOverride.AppCode
			};

			if (!string.IsNullOrWhiteSpace(clientVersionOverride.MaxVersion))
			{
				if (ProductVersion.TryParse(clientVersionOverride.MaxVersion, out ProductVersion version))
				{
					requiredApp.MaxVersion = version;
				}
				else
				{
					ULSLogging.LogTraceTag(0x2384d581 /* tag_97nwb */, Categories.GateDataSet, Levels.Error,
						true, "Override '{0}' contains an incorrect MaxVersion value '{1}'.",
						requiredApp.Name, clientVersionOverride.MaxVersion);

					requiredApp.MaxVersion = ProductVersion.MinVersion;
				}
			}

			if (!string.IsNullOrWhiteSpace(clientVersionOverride.MinVersion))
			{
				if (ProductVersion.TryParse(clientVersionOverride.MinVersion, out ProductVersion version))
				{
					requiredApp.MinVersion = version;
				}
				else
				{
					ULSLogging.LogTraceTag(0x2384d582 /* tag_97nwc */, Categories.GateDataSet, Levels.Error,
						true, "Override '{0}' contains an incorrect MinVersion value '{1}'.",
						requiredApp.Name, clientVersionOverride.MinVersion);

					requiredApp.MinVersion = ProductVersion.MaxVersion;
				}
			}

			if (!string.IsNullOrWhiteSpace(clientVersionOverride.VersionRange))
			{
				// String to list by ;. This makes "A-B;C-D" to [A-B][C-D]
				foreach (string range in clientVersionOverride.VersionRange.Split(s_versionRangeDelimiters, StringSplitOptions.RemoveEmptyEntries))
				{
					// This makes "A-B" to [A][B]. A is min, B is max
					string[] versions = range.Split(s_versionDelimiters, StringSplitOptions.RemoveEmptyEntries);
					if (versions.Length == 2)
					{
						if (ProductVersion.TryParse(versions[0], out ProductVersion minver)
							&& ProductVersion.TryParse(versions[1], out ProductVersion maxver))
						{
							ProductVersionRange versionRange = new ProductVersionRange(minver, maxver);
							requiredApp.AddVersionRange(versionRange);
						}
						else
						{
							ULSLogging.LogTraceTag(0x2384d583 /* tag_97nwd */, Categories.GateDataSet, Levels.Error,
								true, "Override '{0}' contains an incorrect VersionRage value '{1}'.",
								requiredApp.Name, clientVersionOverride.VersionRange);
						}
					}
				}
			}

			if (!string.IsNullOrWhiteSpace(clientVersionOverride.AudienceGroup))
			{
				requiredApp.AudienceGroup = clientVersionOverride.AudienceGroup;
			}

			return requiredApp;
		}


		/// <summary>
		/// Read the existing client from gate and merge with new client
		/// </summary>
		/// <param name="gate">Gate</param>
		/// <param name="client">RequiredClient</param>
		/// <returns>RequiredClient</returns>
		private RequiredClient MergeExistingClient(Gate gate, RequiredClient client)
		{
			if (gate.ClientVersions.TryGetValue(client.Name, out RequiredClient existingClient))
			{
				ULSLogging.LogTraceTag(0x2384d584 /* tag_97nwe */, Categories.GateDataSet, Levels.Error,
					true, "Gate '{0}' contains multiple entries for the same client '{1}'.",
					gate.Name, client.Name);

				if (existingClient.MinVersion != null)
				{
					if (client.MinVersion == null || client.MinVersion < existingClient.MinVersion)
					{
						client.MinVersion = existingClient.MinVersion;
					}
				}

				if (existingClient.MaxVersion != null)
				{
					if (client.MaxVersion == null || client.MaxVersion > existingClient.MaxVersion)
					{
						client.MaxVersion = existingClient.MaxVersion;
					}
				}

				if (existingClient.VersionRanges.Count > 0 && client.VersionRanges.Count == 0)
				{
					foreach (ProductVersionRange range in existingClient.VersionRanges)
					{
						client.AddVersionRange(range);
					}
				}

				if (existingClient.Overrides.Count > 0 && client.Overrides.Count == 0)
				{
					foreach (RequiredApplication app in existingClient.Overrides.Values)
					{
						client.AddOverride(app);
					}
				}
			}

			return client;
		}


		/// <summary>
		/// Read the users that have access to the gate from a configuration gate
		/// </summary>
		/// <param name="configurationGate">configuration gate</param>
		/// <param name="gate">gate to update</param>
		private void ReadUsers(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.UserGroups == null)
			{
				return;
			}

			HashSet<string> users = null;
			UserGroupTypes userTypes = UserGroupTypes.Unspecified;
			foreach (GatingConfiguration.BaseGateTypeUserGroup group in configurationGate.UserGroups)
			{
				switch (group.Type)
				{
					case GatingConfiguration.BaseGateTypeUserGroupType.None:
						userTypes = UserGroupTypes.None;
						break;
					case GatingConfiguration.BaseGateTypeUserGroupType.Dogfood:
						if (userTypes != UserGroupTypes.None)
						{
							userTypes = UserGroupTypes.Dogfood | (userTypes & ~UserGroupTypes.Unspecified);
						}
						break;
					case GatingConfiguration.BaseGateTypeUserGroupType.CustomGroup:
						if (userTypes == UserGroupTypes.None)
						{
							break;
						}
						userTypes = UserGroupTypes.CustomGroup | (userTypes & ~UserGroupTypes.Unspecified);

						if (users == null)
						{
							users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						}

						// Lookup in the test groups and add it to the set of users
						if (string.IsNullOrWhiteSpace(group.Name))
						{
							ULSLogging.LogTraceTag(0x2384d585 /* tag_97nwf */, Categories.GateDataSet, Levels.Error,
								true, "Gate '{0}' contains a user group that is null or only whitespace.",
								gate.Name);
						}
						else
						{
							if (m_testGroupsDataSet == null)
							{
								ULSLogging.LogTraceTag(0x2384d586 /* tag_97nwg */, Categories.GateDataSet, Levels.Error,
									true, "Unable to retrieve the test groups DataSet to resolve test group '{0}' for gate '{1}'.",
									group.Name, gate.Name);
							}
							else
							{
								users.UnionWith(m_testGroupsDataSet.GetGroupUsers(group.Name));
							}
						}
						break;
					case GatingConfiguration.BaseGateTypeUserGroupType.FeatureTeam:
						if (userTypes == UserGroupTypes.None)
						{
							break;
						}
						userTypes = UserGroupTypes.CustomGroup | (userTypes & ~UserGroupTypes.Unspecified);

						if (users == null)
						{
							users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						}

						if (string.IsNullOrEmpty(group.Members))
						{
							ULSLogging.LogTraceTag(0x2384d587 /* tag_97nwh */, Categories.GateDataSet, Levels.Error,
								true, "Gate '{0}' contains a user group that is null or only whitespace",
								gate.Name);
						}
						else
						{
							Array.ForEach(group.Members.Split(' '),
								(alias) =>
								{
									alias = alias.Trim();
									users.Add(alias);
								}
							);
						}
						break;
					default:
						break;
				}
			}

			if (userTypes == UserGroupTypes.None)
			{
				if (users != null)
				{
					ULSLogging.LogTraceTag(0x2384d588 /* tag_97nwi */, Categories.GateDataSet, Levels.Error,
						true, "Gate '{0}' contains user group type 'None' and additional user groups. Ignoring all other user groups.",
						gate.Name);
				}

				gate.Users = null;
			}
			else
			{
				gate.Users = users;
			}
			gate.UserTypes = userTypes;
		}


		/// <summary>
		/// Read the start date from a configuration gate
		/// </summary>
		/// <param name="configurationGate">configuration to read from</param>
		/// <param name="gate">gate to add information to</param>
		private static void ReadStartDate(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.StartDate == null)
			{
				return;
			}

			gate.StartDate = configurationGate.StartDate.Value;
		}


		/// <summary>
		/// Read the end date from a configuration gate
		/// </summary>
		/// <param name="configurationGate">configuration to read from</param>
		/// <param name="gate">gate to add information to</param>
		private static void ReadEndDate(GatingConfiguration.BaseGateType configurationGate, Gate gate)
		{
			if (configurationGate.EndDate == null)
			{
				return;
			}

			gate.EndDate = configurationGate.EndDate.Value;
		}


		/// <summary>
		/// Read the end date from a configuration gate
		/// </summary>
		/// <param name="configurationExperimentGate">configuration to read from</param>
		/// <param name="gateName">gate to add information to</param>
		/// <returns>Experimental weight, null if unable to read</returns>
		private uint? ReadExperimentalWeight(GatingConfiguration.ExperimentGateType configurationExperimentGate, string gateName)
		{
			uint experimentWeight;
			try
			{
				experimentWeight = Convert.ToUInt32(configurationExperimentGate.Weight, CultureInfo.InvariantCulture);
			}
			catch (Exception)
			{
				ULSLogging.LogTraceTag(0x2384d589 /* tag_97nwj */, Categories.GateDataSet, Levels.Error, true,
					"Experiment weight for gate '{0}' is not a proper non negative integer, ignoring.",
					gateName);
				return null;
			}

			return experimentWeight;
		}


		/// <summary>
		/// Loads private copy
		/// </summary>
		/// <param name="resources">resources list</param>
		/// <returns>Loaded TestGroupsDataSet</returns>
		private TestGroupsDataSet<T> LoadTestGroupsDataSet(IDictionary<string, IResourceDetails> resources)
		{
			TestGroupsDataSet<T> testGroupsDataSet = new TestGroupsDataSet<T>(m_testGroupsResourceName);
			if (!resources.TryGetValue(m_testGroupsResourceName, out IResourceDetails resource))
			{
				ULSLogging.LogCodeErrorTag(0x2384d58a /* tag_97nwk */, Categories.GateDataSet, false, true,
					"Resources passed to GateDataSet do not include TestGroups resource '{0}'.", m_testGroupsResourceName);
				return null;
			}

			Dictionary<string, IResourceDetails> reducedDictionary =
				new Dictionary<string, IResourceDetails>(1, StringComparer.OrdinalIgnoreCase)
			{
				{ m_testGroupsResourceName, resource }
			};

			if (!testGroupsDataSet.Load(reducedDictionary))
			{
				ULSLogging.LogCodeErrorTag(0x2384d58b /* tag_97nwl */, Categories.GateDataSet, false,
					true, "Failed to load TestGroupsDataSet.");
			}

			return testGroupsDataSet;
		}


		/// <summary>
		/// TestGroupsDataSet
		/// </summary>
		private TestGroupsDataSet<T> m_testGroupsDataSet;


		/// <summary>
		/// Experiments
		/// </summary>
		public IExperiments Experiments { get; protected set; }

		#endregion
	}


	/// <summary>
	/// Gate DataSet handles reading from the gates file and manages reloading of
	/// gates as needed.
	/// </summary>
	public class GateDataSet : GateDataSet<ConfigurationDataSetLogging>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GateDataSet"/> class.
		/// </summary>
		public GateDataSet()
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="GateDataSet"/> class.
		/// </summary>
		/// <param name="gatesResourceName">Name of the gates resource.</param>
		/// <param name="testGroupsResourceName">Name of the test groups resource.</param>
		public GateDataSet(string gatesResourceName, string testGroupsResourceName)
			: base(gatesResourceName, testGroupsResourceName)
		{
		}
	}
}
