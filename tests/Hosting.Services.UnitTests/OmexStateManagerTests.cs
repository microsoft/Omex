// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Fabric;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.ServiceFabric.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Omex.Extensions.Hosting.Services.UnitTests
{
	[TestClass]
	public class OmexStateManagerTests
	{
		[TestMethod]
		public void Constructor_InitializesPropertiesCorrectly()
		{
			// Arrange
			Mock<IReliableStateManager> mockStateManager = new Mock<IReliableStateManager>();
			ReplicaRole role = ReplicaRole.Primary;

			// Act
			OmexStateManager stateManager = new OmexStateManager(mockStateManager.Object, role);

			// Assert
			Assert.AreEqual(mockStateManager.Object, stateManager.State);
			Assert.IsTrue(stateManager.IsReadable);
			Assert.IsTrue(stateManager.IsWritable);
		}

		[TestMethod]
		public void OmexStateManager_ReadableState_BehavesAsExpected()
		{
			// Arrange
			Mock<IReliableStateManager> mockStateManager = new Mock<IReliableStateManager>();

			OmexStateManager primaryStateManager = new OmexStateManager(mockStateManager.Object, ReplicaRole.Primary);
			OmexStateManager secondaryStateManager = new OmexStateManager(mockStateManager.Object, ReplicaRole.ActiveSecondary);
			OmexStateManager idleStateManager = new OmexStateManager(mockStateManager.Object, ReplicaRole.IdleSecondary);

			// Assert
			Assert.IsTrue(primaryStateManager.IsReadable);
			Assert.IsTrue(secondaryStateManager.IsReadable);
			Assert.IsFalse(idleStateManager.IsReadable);
		}

		[TestMethod]
		public void OmexStateManager_WritableState_BehavesAsExpected()
		{
			// Arrange
			Mock<IReliableStateManager> mockStateManager = new Mock<IReliableStateManager>();

			OmexStateManager primaryStateManager = new OmexStateManager(mockStateManager.Object, ReplicaRole.Primary);
			OmexStateManager secondaryStateManager = new OmexStateManager(mockStateManager.Object, ReplicaRole.ActiveSecondary);

			// Assert
			Assert.IsTrue(primaryStateManager.IsWritable);
			Assert.IsFalse(secondaryStateManager.IsWritable);
		}
	}
}
