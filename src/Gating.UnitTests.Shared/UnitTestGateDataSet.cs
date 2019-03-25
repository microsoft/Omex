// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.Gating.UnitTests.Shared
{
	/// <summary>
	/// Gate data set used in unit tests
	/// </summary>
	public class UnitTestGateDataSet : GateDataSet
	{
		/// <summary>
		/// Create the unit test gate dataSet
		/// </summary>
		/// <remarks>Loads the gate dataSet from an embedded resource in the unit test assembly</remarks>
		public UnitTestGateDataSet(string gatesResourceName, string testGroupsResourceName)
			: base(gatesResourceName, testGroupsResourceName) => GateDictionary = new Dictionary<string, IGate>();


		/// <summary>
		/// Dictionary containing overridden gates
		/// </summary>
		private Dictionary<string, IGate> GateDictionary { get; set; }


		/// <summary>
		/// Override a gate. If a gate with the specified name
		/// is requested, it will be returned instead of retrieving it from the
		/// gate dataSet
		/// </summary>
		/// <param name="name">name of gate</param>
		/// <param name="gate">gate to return, use 'null' to effectively hide a gate</param>
		public void AddGateOverride(string name, IGate gate) => GateDictionary[name] = gate;


		/// <summary>
		/// Remove an overridden gate
		/// </summary>
		/// <param name="name">name of gate</param>
		public void RemoveGateOverride(string name) => GateDictionary.Remove(name);


		/// <summary>
		/// Get a gate by name
		/// </summary>
		/// <param name="gateName">name of gate</param>
		/// <returns>found gate, should return null if a gate matching the name could not be found</returns>
		public override IGate GetGate(string gateName)
		{
			if (!GateDictionary.TryGetValue(gateName, out IGate gate))
			{
				gate = base.GetGate(gateName);
			}

			return gate;
		}


		/// <summary>
		/// Get the gate names that are part of the dataSet
		/// </summary>
		public override IEnumerable<string> GateNames
		{
			get
			{
				foreach (string gate in GateDictionary.Keys)
				{
					if (GateDictionary[gate] != null)
					{
						yield return gate;
					}
				}
				foreach (string gate in base.GateNames)
				{
					if (!GateDictionary.ContainsKey(gate))
					{
						yield return gate;
					}
				}
			}
		}


		/// <summary>
		/// Get the gates that are part of the dataSet
		/// </summary>
		public override IEnumerable<IGate> Gates
		{
			get
			{
				foreach (string key in GateNames)
				{
					yield return GetGate(key);
				}
			}
		}
	}
}