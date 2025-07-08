// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.ServiceFabric.Data;

namespace Microsoft.Omex.Extensions.Hosting.Services;

/// <summary>
/// Manages the state of the Omex service.
/// </summary>
public sealed class OmexStateManager
{
	private readonly IReliableStateManager m_stateManager;
	private readonly ReplicaRole m_role;

	/// <summary>
	/// Initializes a new instance of the <see cref="OmexStateManager"/> class.
	/// </summary>
	/// <param name="stateManager">The reliable state manager.</param>
	/// <param name="role">The replica role.</param>
	public OmexStateManager(IReliableStateManager stateManager, ReplicaRole role)
	{
		m_stateManager = stateManager;
		m_role = role;
	}

	/// <summary>
	/// Gets the reliable state manager.
	/// </summary>
	public IReliableStateManager State => m_stateManager;

	/// <summary>
	/// Gets a value indicating whether the state is readable.
	/// </summary>
	public bool IsReadable => m_role == ReplicaRole.Primary || m_role == ReplicaRole.ActiveSecondary;

	/// <summary>
	/// Gets a value indicating whether the state is writable.
	/// </summary>
	public bool IsWritable => m_role == ReplicaRole.Primary;
}
