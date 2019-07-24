// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Omex.Gating.Experimentation;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Model.Types;
using Microsoft.Omex.System.Network;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Gate context, handles active gates for the current
	/// request and manages which gates are applicable
	/// </summary>
	public class GateContext : IGateContext
	{
		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="request">request</param>
		/// <param name="machineInformation">machine information</param>
		/// <param name="experimentContext">experiment context</param>
		/// <param name="knownIpAddresses">known ip addresses</param>
		/// <param name="settings">GateSettings</param>
		public GateContext(IGatedRequest request, IMachineInformation machineInformation, IExperimentContext experimentContext, INamedIPAddresses knownIpAddresses = null, IGateSettings settings = null)
		{
			Request = request;
			MachineInformation = machineInformation;
			ExperimentContext = experimentContext;
			KnownIpAddresses = knownIpAddresses;
			m_settings = settings;
		}
		#endregion

		#region Fields

		/// <summary>
		/// GateSettings.
		/// </summary>
		private readonly IGateSettings m_settings;

		#endregion

		#region IGateContext Members
		/// <summary>
		/// The current request
		/// </summary>
		public IGatedRequest Request { get; protected set; }


		/// <summary>
		/// The machine information
		/// </summary>
		protected IMachineInformation MachineInformation { get; set; }


		/// <summary>
		/// The experiment context
		/// </summary>
		protected IExperimentContext ExperimentContext { get; set; }


		/// <summary>
		/// The experiment context
		/// </summary>
		protected INamedIPAddresses KnownIpAddresses { get; set; }


		/// <summary>
		/// Known applicable gates
		/// </summary>
		private ConcurrentDictionary<string, byte> KnownApplicableGates { get; set; }


		/// <summary>
		/// Known blocked gates
		/// </summary>
		private ConcurrentDictionary<string, byte> KnownBlockedGates { get; set; }


		/// <summary>
		/// All gates
		/// </summary>
		private Gates AllGates { get; }


		/// <summary>
		/// Is the gate applicable for the current context
		/// </summary>
		/// <param name="gate">gate to check</param>
		/// <returns>true if applicable, false otherwise</returns>
		public bool IsGateApplicable(IGate gate)
		{
			if (!Code.ValidateArgument(gate, nameof(gate), TaggingUtilities.ReserveTag(0x2382104a /* tag_967bk */)) ||
				!Code.ValidateNotNullOrWhiteSpaceArgument(gate.Name, nameof(gate.Name), TaggingUtilities.ReserveTag(0x2382104b /* tag_967bl */)))
			{
				return false;
			}

			if (IsKnownApplicableGate(gate.Name))
			{
				return true;
			}

			if (IsKnownBlockedGate(gate.Name))
			{
				return false;
			}

			bool grantAccess = IsGateApplicableInternal(gate);
			if (grantAccess)
			{
				KnownApplicableGates.AddOrUpdate(gate.Name, _ => 0, (_, __) => 0);
			}
			else
			{
				KnownBlockedGates.AddOrUpdate(gate.Name, _ => 0, (_, __) => 0);
			}

			return grantAccess;
		}


		/// <summary>
		/// Is this a known applicable gate
		/// </summary>
		/// <param name="gateName">name of the gate</param>
		/// <returns>true if known applicable gate, false otherwise</returns>
		private bool IsKnownApplicableGate(string gateName)
		{
			if (KnownApplicableGates == null)
			{
				KnownApplicableGates = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

				ISet<string> requestedGates = Request?.RequestedGateIds;
				if (requestedGates != null)
				{
					UpdateGatesDictionary(KnownApplicableGates, requestedGates);
				}

				UpdateGatesDictionary(KnownApplicableGates, m_settings?.GatesOverrideEnabled);
			}

			if (KnownApplicableGates.ContainsKey(gateName))
			{
				ULSLogging.LogTraceTag(0x2382104c /* tag_967bm */, Categories.GateSelection, Levels.Verbose,
					"Allowing access to gate '{0}' as it has been previously allowed.", gateName);
				return true;
			}

			return false;
		}


		/// <summary>
		/// Is this a known blocked gate
		/// </summary>
		/// <param name="gateName">name of the gate</param>
		/// <returns>true if known blocked gate, false otherwise</returns>
		private bool IsKnownBlockedGate(string gateName)
		{
			if (KnownBlockedGates == null)
			{
				KnownBlockedGates = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

				ISet<string> blockedGates = Request?.BlockedGateIds;
				if (blockedGates != null)
				{
					UpdateGatesDictionary(KnownBlockedGates, blockedGates);
				}

				UpdateGatesDictionary(KnownBlockedGates, m_settings?.GatesOverrideDisabled);
			}

			if (KnownBlockedGates.ContainsKey(gateName))
			{
				ULSLogging.LogTraceTag(0x2382104d /* tag_967bn */, Categories.GateSelection, Levels.Verbose,
					"Blocking access to gate '{0}' as it has been previously blocked.", gateName);
				return true;
			}

			return false;
		}


		/// <summary>
		/// Update dictionary containing gates
		/// </summary>
		/// <param name="dictionary">Dictionary of gatyes</param>
		/// <param name="gates">Gates to add to dictionary</param>
		private static void UpdateGatesDictionary(ConcurrentDictionary<string, byte> dictionary, IEnumerable<string> gates)
		{
			if (dictionary == null || gates == null)
			{
				return;
			}

			foreach (string gate in gates.Where(gate => !string.IsNullOrWhiteSpace(gate)))
			{
				dictionary.AddOrUpdate(gate, _ => 0, (_, __) => 0);
			}
		}


		/// <summary>
		/// Is the gate applicable for the current context
		/// </summary>
		/// <param name="gate">gate to check</param>
		/// <returns>true if applicable, false otherwise</returns>
		private bool IsGateApplicableInternal(IGate gate)
		{
			bool grantAccess = gate.IsGateEnabled;

			// If the gate is disabled but has been asked to be enabled through settings.
			if (m_settings?.GatesToggleEnabled?.Contains(gate.Name) ?? false)
			{
				grantAccess = true;
				ULSLogging.LogTraceTag(0x2382104e /* tag_967bo */, Categories.GateSelection, Levels.Verbose, "Enabling gate '{0}' through BRS that was disabled using gate toggle setting",
					gate.Name);
			}

			grantAccess = grantAccess && DoesHostEnvironmentHaveAccess(gate);
			grantAccess = grantAccess && DoesServiceHaveAccess(gate);
			grantAccess = grantAccess && DoesIPAddressHaveAccess(gate);
			grantAccess = grantAccess && DoesMarketHaveAccess(gate);
			grantAccess = grantAccess && DoesEnvironmentHaveAccess(gate);
			grantAccess = grantAccess && DoesBrowserHaveAccess(gate);
			grantAccess = grantAccess && DoesClientHaveAccess(gate);
			grantAccess = grantAccess && DoesUserHaveAccess(gate);
			grantAccess = grantAccess && IsCurrentDateEnabled(gate);
			grantAccess = grantAccess && DoQueryParametersHaveAccess(gate);
			grantAccess = grantAccess && DoesCloudContextHaveAccess(gate);

			if (gate.ExperimentInfo != null)
			{
				grantAccess = grantAccess && ExperimentContext.IsExperimentalGateApplicable(gate);
			}

			return grantAccess;
		}


		/// <summary>
		/// Enter a scope of code that is gated if it is applicable
		/// </summary>
		/// <param name="gate">the gate to enter scope for</param>
		public void EnterScope(IGate gate)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(gate.Name, nameof(gate.Name), TaggingUtilities.ReserveTag(0x2382104f /* tag_967bp */)))
			{
				return;
			}

			// Add the id of the gate to the current users scopes
			InstanceCount count = m_gates.GetOrAdd(gate.Name, s => new InstanceCount(gate));
			count.Increment();
		}


		/// <summary>
		/// Exit a scope of code that is gateed
		/// </summary>
		/// <param name="gate">the gate to enter scope for</param>
		public void ExitScope(IGate gate)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(gate.Name, nameof(gate.Name), TaggingUtilities.ReserveTag(0x23821050 /* tag_967bq */)))
			{
				return;
			}

			InstanceCount count;
			if (m_gates.TryGetValue(gate.Name, out count))
			{
				int activeScopes = count.Decrement();
				if (activeScopes < 0)
				{
					ULSLogging.LogTraceTag(0x23821051 /* tag_967br */, Categories.GateSelection,
						Levels.Error, "Unbalanced scope for gate '{0}', attempting to exit scope that has already been exited.", gate.Name);
				}
			}
			else
			{
				ULSLogging.LogTraceTag(0x23821052 /* tag_967bs */, Categories.GateSelection,
					Levels.Error, "Unbalanced scope for gate '{0}', attempting to exit scope that has not been entered.", gate.Name);
			}
		}


		/// <summary>
		/// The current active gates for the context
		/// </summary>
		/// <returns>collection of all active gates</returns>
		public IEnumerable<string> CurrentGates
		{
			get
			{
				foreach (KeyValuePair<string, InstanceCount> gateScope in m_gates)
				{
					if (gateScope.Value.Value > 0)
					{
						yield return gateScope.Key;
					}
				}
			}
		}


		/// <summary>
		/// The collection of all gates that have been used for the current context
		/// </summary>
		/// <returns>collection of all gates that have been used</returns>
		public IEnumerable<IGate> ActivatedGates => m_gates.Values.Select(instance => instance.Gate);
		#endregion


		#region Determine access

		/// <summary>
		/// Are any of the current query parameters blocked by the gate critera?
		/// </summary>
		/// <param name="gate">gate</param>
		/// <returns>true if no blocked query parameters match the requests query parmeters</returns>
		private bool DoQueryParametersHaveAccess(IGate gate)
		{
			bool grantAccess = true;
			IDictionary<string, HashSet<string>> blockedQueryParameters = gate.BlockedQueryParameters;
			if (blockedQueryParameters != null && blockedQueryParameters.Count > 0)
			{
				IDictionary<string, HashSet<string>> requestsQueryParameters = Request?.QueryParameters;
				//if no params exist on the request, then we grant access by default.
				if (requestsQueryParameters == null || requestsQueryParameters.Count == 0)
				{
					ULSLogging.LogTraceTag(0x23821053 /* tag_967bt */, Categories.GateSelection, Levels.Verbose,
						"Allowing access to gate '{0}' as the GatedRequest has no query parameters.",
						gate.Name ?? "<NULL>");
					return grantAccess;
				}

				//if any request query param matches a blocked query param, we should not grant access.
				foreach (string requestParameterName in requestsQueryParameters.Keys)
				{
					if (blockedQueryParameters.ContainsKey(requestParameterName))
					{
						//if the wild card '*' is specified as a value for a blocked param, then we restrict access
						if (blockedQueryParameters[requestParameterName].Contains(Gate.BlockedQueryParameterValueWildCard))
						{
							grantAccess = false;

							ULSLogging.LogTraceTag(0x23821054 /* tag_967bu */, Categories.GateSelection, Levels.Verbose,
								"Not allowing access to gate '{0}' with the query parameter(s) '{1}'='{2}' as all parameters with name '{3}' are blocked by the wildcard '*'.",
								gate.Name ?? "<NULL>",
								requestParameterName,
								requestsQueryParameters[requestParameterName],
								requestParameterName);

							break;
						}

						//if there's corresponding parameters in the request and the list of blocked, we do not grant access
						IEnumerable<string> intersection = blockedQueryParameters[requestParameterName]
							.Intersect(requestsQueryParameters[requestParameterName], StringComparer.OrdinalIgnoreCase);

						if (intersection.Count() > 0)
						{
							grantAccess = false;

							ULSLogging.LogTraceTag(0x23821055 /* tag_967bv */, Categories.GateSelection, Levels.Verbose,
								"Not allowing access to gate '{0}' as the query parameter(s) '{1}'='{2}' is in the set of blocked query parameters '{3}'.",
								gate.Name ?? "<NULL>",
								requestParameterName,
								string.Join(",", intersection),
								string.Join(", ", blockedQueryParameters.Select(parameter => string.Format("'{0}={1}'", parameter.Key, parameter.Value))));

							break;
						}
					}
				}
			}

			return grantAccess;
		}


		/// <summary>
		/// Is the host environment part of the gate criteria
		/// </summary>
		/// <param name="gate">gate</param>
		/// <returns>true if the host environment matches the gate criteria, false otherwise</returns>
		private bool DoesHostEnvironmentHaveAccess(IGate gate)
		{
			bool grantAccess = true;
			HashSet<string> hostEnvironments = gate.HostEnvironments;
			if (hostEnvironments != null)
			{
				string environmentName = MachineInformation?.EnvironmentName ?? "None";
				grantAccess = hostEnvironments.Contains(environmentName);

				if (!grantAccess)
				{
					ULSLogging.LogTraceTag(0x23821056 /* tag_967bw */, Categories.GateSelection, Levels.Verbose,
						"Not allowing access to gate '{0}' as the host environment '{1}' is not in the set of active host environments '{2}'.",
						gate.Name ?? "<NULL>", environmentName ?? "<NULL>", string.Join(", ", hostEnvironments));
				}
			}

			return grantAccess;
		}


		/// <summary>
		/// Does the service have access.
		/// </summary>
		/// <param name="gate">The gate.</param>
		/// <returns>true if service has access, false otherwise.</returns>
		private bool DoesServiceHaveAccess(IGate gate)
		{
			bool grantAccess = true;
			IDictionary<string, GatedServiceTypes> services = gate.Services;

			// If gate doesn't expect service based gating, grant access.
			if (services != null)
			{
				GatedServiceTypes serviceFlag;
				string serviceName = MachineInformation.ServiceName;
				if (services.TryGetValue(serviceName, out serviceFlag))
				{
					switch (serviceFlag)
					{
						// GrantAccess if gate will be applicable for both Full & Canary Services.
						case GatedServiceTypes.All:
							break;

						// GrantAccess only when its a full(non-canary service).
						case GatedServiceTypes.FullService:
							grantAccess = !MachineInformation.IsCanary;
							break;

						// GrantAccess only when its a canary service.
						case GatedServiceTypes.CanaryService:
							grantAccess = MachineInformation.IsCanary;
							break;

						// Deny Access if service flag is none.
						case GatedServiceTypes.None:
							grantAccess = false;
							break;
					}
				}
				else
				{
					// If gate expects a service but current machine is not running that service or there's no service
					// either because no service was mentioned within service tag in gate or consolidation yielded zero service,
					// deny access in all cases.
					grantAccess = false;
				}

				if (!grantAccess)
				{
					ULSLogging.LogTraceTag(0x23821057 /* tag_967bx */, Categories.GateSelection, Levels.Verbose,
						"Not allowing access to gate '{0}' as '{1}' did not match the required criteria.",
						gate.Name ?? "<NULL>", serviceName ?? "<NULL>");
				}
			}

			return grantAccess;
		}


		/// <summary>
		/// Is the ip address of the request part of the gate criteria
		/// </summary>
		/// <param name="gate">gate</param>
		/// <returns>true if the ip address of the request matches the gate criteria, false otherwise</returns>
		private bool DoesIPAddressHaveAccess(IGate gate)
		{
			bool grantAccess = true;
			HashSet<string> ipRanges = gate.KnownIPRanges;
			if (ipRanges != null)
			{
				grantAccess = false;

				foreach (string ipRange in ipRanges)
				{
					if (Request != null && Request.IsPartOfKnownIPRange(KnownIpAddresses, ipRange))
					{
						grantAccess = true;
						break;
					}
				}

				if (!grantAccess)
				{
					ULSLogging.LogTraceTag(0x23821058 /* tag_967by */, Categories.GateSelection, Levels.Verbose,
						"Not allowing access to gate '{0}' as the ip address of the request is not in the set of allowed known ip ranges '{1}'.",
						gate.Name ?? "<NULL>", string.Join(", ", ipRanges));
				}
			}

			return grantAccess;
		}


		/// <summary>
		/// Is the market of the request part of the gate criteria
		/// </summary>
		/// <param name="gate">gate</param>
		/// <returns>true if the market of the request matches the gate criteria, false otherwise</returns>
		private bool DoesMarketHaveAccess(IGate gate)
		{
			bool grantAccess = true;
			HashSet<string> markets = gate.Markets;
			if (markets != null)
			{
				string requestMarket = Request?.Market;
				if (requestMarket == null || !gate.Markets.Contains(Request.Market))
				{
					grantAccess = false;

					ULSLogging.LogTraceTag(0x23821059 /* tag_967bz */, Categories.GateSelection, Levels.Verbose,
						"Not allowing access to gate '{0}' as the market of the request '{1}' is not in the set of allowed markets '{2}'.",
						gate.Name ?? "<NULL>", requestMarket ?? "<NULL>", string.Join(", ", markets));
				}
			}

			return grantAccess;
		}


		/// <summary>
		/// Is the environment of the request part of the gate criteria
		/// </summary>
		/// <param name="gate">gate</param>
		/// <returns>true if the environment of the request matches the gate criteria, false otherwise</returns>
		private bool DoesEnvironmentHaveAccess(IGate gate)
		{
			bool grantAccess = true;
			HashSet<string> environments = gate.Environments;
			if (environments != null)
			{
				string requestEnvironment = Request?.Environment;
				if (requestEnvironment == null || !gate.Environments.Contains(Request.Environment))
				{
					grantAccess = false;

					ULSLogging.LogTraceTag(0x2382105a /* tag_967b0 */, Categories.GateSelection, Levels.Verbose,
						"Not allowing access to gate '{0}' as the environment of the request '{1}' is not in the set of allowed environments '{2}'.",
						gate.Name ?? "<NULL>", requestEnvironment ?? "<NULL>", string.Join(", ", environments));
				}
			}

			return grantAccess;
		}


		/// <summary>
		/// Does the requesting client have access to the gate
		/// </summary>
		/// <param name="gate">gate</param>
		/// <returns>true if the client have access, false otherwise</returns>
		private bool DoesClientHaveAccess(IGate gate)
		{
			bool grantAccess = true;
			IDictionary<string, RequiredClient> allowedClients = gate.ClientVersions;
			if (allowedClients != null)
			{
				GatedClient client = Request?.CallingClient;
				RequiredClient requiredClient;
				if (client == null ||
					!allowedClients.TryGetValue(client.Name, out requiredClient) ||
					!IsVersionInRange(client, requiredClient) ||
					!IsInAudienceGroup(client, requiredClient))
				{
					grantAccess = false;

					ULSLogging.LogTraceTag(0x2382105b /* tag_967b1 */, Categories.GateSelection, Levels.Verbose,
						"Not allowing access to gate '{0}' as the calling client '{1}' is not one of the allowed clients '{2}'.",
						gate.Name ?? "<NULL>", GatedClientAsString(client), AllowedClientsAsString(allowedClients));
				}
			}

			return grantAccess;
		}


		/// <summary>
		/// Is the requesting client version in range?
		/// </summary>
		/// <param name="client">GatedClient</param>
		/// <param name="requiredClient">RequiredClient</param>
		/// <returns>true if the client version is in required client version range, false otherwise</returns>
		private static bool IsVersionInRange(GatedClient client, RequiredClient requiredClient)
		{
			if (client.Version == null)
			{
				return false;
			}

			RequiredApplication requiredApplication;

			if (client.AppCode != null &&
				requiredClient.Overrides.TryGetValue(client.AppCode, out requiredApplication))
			{
				if (requiredApplication.VersionRanges.Count > 0)
				{
					// VersionRanges of application
					foreach (ProductVersionRange range in requiredApplication.VersionRanges)
					{
						if (range.Min <= client.Version && range.Max >= client.Version)
						{
							return true;
						}
					}

					return false;
				}

				// Between MinVersion and MaxVersion of application
				if ((requiredApplication.MinVersion != null && requiredApplication.MinVersion > client.Version) ||
					 (requiredApplication.MaxVersion != null && requiredApplication.MaxVersion <= client.Version))
				{
					return false;
				}

				return true;
			}
			else
			{
				if (requiredClient.VersionRanges.Count > 0)
				{
					// VersionRanges of platform
					foreach (ProductVersionRange range in requiredClient.VersionRanges)
					{
						if (range.Min <= client.Version && range.Max >= client.Version)
						{
							return true;
						}
					}

					return false;
				}

				// Between MinVersion and MaxVersion of platform
				if ((requiredClient.MinVersion != null && requiredClient.MinVersion > client.Version) ||
					 (requiredClient.MaxVersion != null && requiredClient.MaxVersion <= client.Version))
				{
					return false;
				}

				return true;
			}
		}


		/// <summary>
		/// Is the requesting in the audience group?
		/// </summary>
		/// <param name="callingClient">GatedClient</param>
		/// <param name="requiredClient">RequiredClient</param>
		/// <returns>true if the calling client is in required client audience group, false otherwise</returns>
		private static bool IsInAudienceGroup(GatedClient callingClient, RequiredClient requiredClient)
		{
			if (string.IsNullOrWhiteSpace(requiredClient.AudienceGroup))
			{
				// Return true because AudienceGroup check is not needed
				return true;
			}

			if (callingClient.AudienceGroups.Count == 0)
			{
				return false;
			}

			RequiredApplication requiredApplication;

			if (callingClient.AppCode != null &&
				requiredClient.Overrides.TryGetValue(callingClient.AppCode, out requiredApplication) &&
				!string.IsNullOrWhiteSpace(requiredApplication.AudienceGroup))
			{
				// App Override
				if (callingClient.AudienceGroups.Contains(requiredApplication.AudienceGroup, StringComparer.OrdinalIgnoreCase))
				{
					return true;
				}

				return false;
			}
			else
			{
				// ClientVersion
				if (callingClient.AudienceGroups.Contains(requiredClient.AudienceGroup, StringComparer.OrdinalIgnoreCase))
				{
					return true;
				}

				return false;
			}
		}


		/// <summary>
		/// Does the browser have access.
		/// </summary>
		/// <param name="gate">The gate.</param>
		/// <returns>True if the browser type and version is compatible and hence has access, else false.</returns>
		private bool DoesBrowserHaveAccess(IGate gate)
		{
			// If gate has blocked browsers for which gate access is to be blocked,
			// browser blocking logic takes precedence and we don't check for allowed browsers.
			if (gate.BlockedBrowsers != null)
			{
				return !CheckGateApplicabilityForBrowser(gate.BlockedBrowsers);
			}

			// If gate has browsers for which access is to be granted.
			if (gate.AllowedBrowsers != null)
			{
				return CheckGateApplicabilityForBrowser(gate.AllowedBrowsers);
			}

			return true;
		}

		private bool DoesCloudContextHaveAccess(IGate gate)
		{
			bool grantAccess = true;
			HashSet<string> contexts = gate.CloudContexts;
			if (contexts != null)
			{
				HashSet<string> requestContexts = Request?.CloudContexts;
				if (requestContexts == null || gate.CloudContexts.Intersect(Request.CloudContexts).Count() == 0)
				{
					grantAccess = false;

					ULSLogging.LogTraceTag(0, Categories.GateSelection, Levels.Verbose,
						"Not allowing access to gate '{0}' as the cloud contexts of the request '{1}' are not in the set of allowed cloud contexts '{2}'.",
						gate.Name ?? "<NULL>", requestContexts == null ? "<NULL>" : string.Join(", ", requestContexts), string.Join(", ", contexts));
				}
			}

			return grantAccess;
		}

		/// <summary>
		/// Checks the gate's applicability based on the user agent browser. This is a common method which checks gate's applicability for
		/// both allowed browsers and blocked browsers.
		/// </summary>
		/// <param name="browsers">The browsers.</param>
		/// <returns>true if gate's applicability based on constraints is confirmed, false otherwise.</returns>
		private bool CheckGateApplicabilityForBrowser(IDictionary<string, HashSet<int>> browsers)
		{
			Tuple<string, int> browser = Request?.GetUserAgentBrowser();
			if (browser == null)
			{
				ULSLogging.LogTraceTag(0x2382105c /* tag_967b2 */, Categories.GateSelection, Levels.Verbose,
					"HttpRequest object doesn't have any browser, which means request not made from a browser.");

				// Request not made from a browser.
				return false;
			}

			if (!browsers.TryGetValue(browser.Item1, out HashSet<int> browserVersion))
			{
				ULSLogging.LogTraceTag(0x2382105d /* tag_967b3 */, Categories.GateSelection, Levels.Verbose,
					"Gate expects a different browser than the one from which the request was made.");

				// Gate expects different browser, hence return false.
				return false;
			}

			if (browserVersion == null)
			{
				ULSLogging.LogTraceTag(0x2382105e /* tag_967b4 */, Categories.GateSelection, Levels.Verbose,
					"Gate doesn't contain any browser version, which means all the versions apply.");

				// No constraint set on the browser's version, hence return true.
				return true;
			}

			if (browserVersion.Count == 0)
			{
				ULSLogging.LogTraceTag(0x2382105f /* tag_967b5 */, Categories.GateSelection, Levels.Verbose,
					"Gate doesn't contain any browser version, which means all the versions apply.");

				// No constraint set on the browser's version, hence return true.
				return true;
			}

			if (!browserVersion.Contains(browser.Item2))
			{
				ULSLogging.LogTraceTag(0x23821060 /* tag_967b6 */, Categories.GateSelection, Levels.Verbose,
					"Gate expects a different browser version than the one from which the request was made.");

				// Browser versions not compatible, return false.
				return false;
			}

			return true;
		}


		/// <summary>
		/// Get the gated client as a string for logging purposes
		/// </summary>
		/// <param name="client">client</param>
		/// <returns>formatted string</returns>
		private static string GatedClientAsString(GatedClient client)
		{
			if (client == null)
			{
				return "<NULL>";
			}

			return string.Format(CultureInfo.InvariantCulture, "{0},{1}", client.Name ?? "<NULL>", client.Version != null ? client.Version.ToString() : "<NONE>");
		}


		/// <summary>
		/// Get the list of allowed clients as a string for logging purposes
		/// </summary>
		/// <param name="allowedClients">dictionary of allowed clients</param>
		/// <returns>string describing the allowed clients</returns>
		public static string AllowedClientsAsString(IDictionary<string, RequiredClient> allowedClients)
		{
			if (allowedClients == null)
			{
				return string.Empty;
			}

			StringBuilder builder = new StringBuilder(100);
			foreach (RequiredClient client in allowedClients.Values)
			{
				if (builder.Length > 0)
				{
					builder.Append(";");
				}

				builder.AppendFormat("{0},Min:{1},Max:{2}",
					client.Name ?? "<NULL>",
					client.MinVersion != null ? client.MinVersion.ToString() : "<NONE>",
					client.MaxVersion != null ? client.MaxVersion.ToString() : "<NONE>");
			}

			return builder.ToString();
		}


		/// <summary>
		/// Does the user have access to the gate
		/// </summary>
		/// <param name="gate">gate</param>
		/// <returns>true if the user have access, false otherwise</returns>
		private bool DoesUserHaveAccess(IGate gate)
		{
			bool grantAccess = true;
			UserGroupTypes userType = gate.UserTypes;
			if (userType != UserGroupTypes.Unspecified)
			{
				grantAccess = false;
			}

			if ((UserGroupTypes.Dogfood & userType) == UserGroupTypes.Dogfood)
			{
				grantAccess = Request?.Users != null && Request.Users.Any(user => user != null && user.IsDogfoodUser);
			}

			if (!grantAccess && ((UserGroupTypes.CustomGroup & userType) == UserGroupTypes.CustomGroup))
			{
				HashSet<string> users = gate.Users;

				if (Request?.Users != null)
				{
					grantAccess = Request.Users.Any(user =>
					{
						if (user != null && user.UserIdentifier != null)
						{
							if (users.Contains(user.UserIdentifier))
							{
								return true;
							}

							int atIndex = user.UserIdentifier.LastIndexOf('@');
							if (atIndex >= 0)
							{
								string domain = user.UserIdentifier.Substring(atIndex);
								return users.Contains(domain);
							}
						}

						return false;
					});
				}

				if (!grantAccess)
				{
					ULSLogging.LogTraceTag(0x23821061 /* tag_967b7 */, Categories.GateSelection, Levels.Verbose,
						"Not allowing access to gate '{0}' as user is not part of the set of users that have access.",
						gate.Name);
				}
			}
			else if (!grantAccess)
			{
				ULSLogging.LogTraceTag(0x23821062 /* tag_967b8 */, Categories.GateSelection, Levels.Verbose,
					"Not allowing access to gate '{0}' as user is not of the accepted user type '{1}'.",
					gate.Name, userType);
			}

			if (!grantAccess)
			{
				return false;
			}

			// Walk the hierarchy to see if we need to check additional access (if UserTypes is not null)
			IGate parent = gate;
			while ((parent = parent.ParentGate) != null)
			{
				if ((parent.UserTypes & UserGroupTypes.Dogfood) == UserGroupTypes.Dogfood)
				{
					// Because the parent gate contains a user type restriction that cannot be denormalized,
					// check the access on the parent gate as well.
					if (!DoesUserHaveAccess(parent))
					{
						grantAccess = false;
						break;
					}
				}
			}

			return grantAccess;
		}


		/// <summary>
		/// Is the current part of the gate criteria
		/// </summary>
		/// <param name="gate">gate</param>
		/// <returns>true if the market of the request matches the gate criteria, false otherwise</returns>
		private static bool IsCurrentDateEnabled(IGate gate)
		{
			if (gate.StartDate.HasValue && gate.StartDate.Value > DateTime.UtcNow)
			{
				ULSLogging.LogTraceTag(0x23821063 /* tag_967b9 */, Categories.GateSelection, Levels.Verbose,
					"Not allowing access to gate '{0}' as the current date is before the start date '{1}'.",
					gate.Name ?? "<NULL>", gate.StartDate.Value);
				return false;
			}

			if (gate.EndDate.HasValue && gate.EndDate.Value < DateTime.UtcNow)
			{
				ULSLogging.LogTraceTag(0x23821080 /* tag_967ca */, Categories.GateSelection, Levels.Verbose,
					"Not allowing access to gate '{0}' as the current date is after the end date '{1}'.",
					gate.Name ?? "<NULL>", gate.EndDate.Value);
				return false;
			}

			return true;
		}
		#endregion


		#region Member variables
		/// <summary>
		/// Dictionary of gates that are or have been executed during the use of the gate context.
		/// Maps the number of active scopes for a specific gate.
		/// </summary>
		private readonly ConcurrentDictionary<string, InstanceCount> m_gates = new ConcurrentDictionary<string, InstanceCount>();
		#endregion


		#region GateScopes
		/// <summary>
		/// Simple class to allow for multiple threads to increment/decrement the scope count
		/// </summary>
		private class InstanceCount
		{
			/// <summary>
			/// The actual integer value
			/// </summary>
			private int m_value;


			/// <summary>
			/// The actual integer value
			/// </summary>
			public int Value => m_value;


			/// <summary>
			/// The gate.
			/// </summary>
			public IGate Gate { get; }


			/// <summary>
			/// Increment the value
			/// </summary>
			/// <returns>the incremented value</returns>
			public int Increment() => Interlocked.Increment(ref m_value);


			/// <summary>
			/// Decrement the value
			/// </summary>
			/// <returns>the decremented value</returns>
			public int Decrement() => Interlocked.Decrement(ref m_value);


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="gate">The gate.</param>
			public InstanceCount(IGate gate)
			{
				Gate = Code.ExpectsArgument(gate, nameof(gate), TaggingUtilities.ReserveTag(0x23821081 /* tag_967cb */));
			}
		}
		#endregion
	}
}
