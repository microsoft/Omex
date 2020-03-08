// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

//Old namespace used to be compatible with TimedScopeGen
namespace Microsoft.Omex.Extensions.Compatibility.TimedScopes
{
	/// <summary>
	/// Store Timed Scope name and its description
	/// </summary>
	[Obsolete(TimedScopeDefinitionExtensions.ObsoleteMessage, TimedScopeDefinitionExtensions.IsObsoleteError)]
	public class TimedScopeDefinition
	{
		/// <summary>
		/// Name
		/// </summary>
		public string Name { get; }


		/// <summary>
		/// Description
		/// </summary>
		public string Description { get; }


		/// <summary>
		/// Description
		/// </summary>
		public string LinkToOnCallEngineerHandbook { get; }


		/// <summary>
		/// Should the scope be logged only when explicitly demanded
		/// </summary>
		public bool OnDemand { get; }


		/// <summary>
		/// Does the scope capture user hashes that are suitable for unique user-based alerting?
		/// </summary>
		public bool CapturesUniqueUserHashes { get; }


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="description">Description</param>
		/// <param name="linkToOnCallEngineerHandbook">Link to On Call Engineer Handbook</param>
		/// <param name="onDemand">Should the scope be logged only when explicitly demanded</param>
		/// <param name="capturesUniqueUserHashes">Does the scope capture user hashes that are suitable for unique user-based alerting?</param>
		public TimedScopeDefinition(string name, string? description = null, string? linkToOnCallEngineerHandbook = null, bool onDemand = false, bool capturesUniqueUserHashes = false)
		{
			Name = name;
			Description = description ?? string.Empty;
			LinkToOnCallEngineerHandbook = linkToOnCallEngineerHandbook ?? string.Empty;
			OnDemand = onDemand;
			CapturesUniqueUserHashes = capturesUniqueUserHashes;
		}
	}
}
