// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.FeatureManagement.Tests.Extensions;

using System;
using System.Security.Claims;
using Microsoft.Omex.FeatureManagement.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class ClaimsPrincipalExtensionsTests
{
	#region IsAuthenticated

	[TestMethod]
	public void IsAuthenticated_WhenIdentityIsNull_ReturnsFalse()
	{
		// ARRANGE
		ClaimsPrincipal principal = new();

		// ACT
		bool result = principal.IsAuthenticated();

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsAuthenticated_WhenIdentityIsNotAuthenticated_ReturnsFalse()
	{
		// ARRANGE
		ClaimsIdentity identity = new();
		ClaimsPrincipal principal = new(identity);

		// ACT
		bool result = principal.IsAuthenticated();

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsAuthenticated_WhenIdentityIsAuthenticated_ReturnsTrue()
	{
		// ARRANGE
		ClaimsIdentity identity = new("Bearer");
		ClaimsPrincipal principal = new(identity);

		// ACT
		bool result = principal.IsAuthenticated();

		// ASSERT
		Assert.IsTrue(result);
	}

	#endregion

	#region TryGetEntraId

	[TestMethod]
	public void TryGetEntraId_WhenOidClaimIsValidGuid_ReturnsGuid()
	{
		// ARRANGE
		const string validGuid = "b1a7e8e2-1c2d-4e3f-9a4b-5c6d7e8f9a0b";
		ClaimsPrincipal principal = new(
		[
			new(
			[
				new("oid", validGuid),
			]),
		]);

		// ACT
		bool result = principal.TryGetEntraId(out Guid entraId);

		// ASSERT
		Assert.IsTrue(result);
		Assert.AreEqual(Guid.Parse(validGuid), entraId);
	}

	[TestMethod]
	[DataRow("OID")]
	[DataRow("Oid")]
	[DataRow("oId")]
	public void TryGetEntraId_WhenOidClaimIsDifferentCasing_ReturnsGuid(string claimType)
	{
		// ARRANGE
		const string validGuid = "b1a7e8e2-1c2d-4e3f-9a4b-5c6d7e8f9a0b";
		ClaimsPrincipal principal = new(
		[
			new(
			[
				new(claimType, validGuid),
			]),
		]);

		// ACT
		bool result = principal.TryGetEntraId(out Guid entraId);

		// ASSERT
		Assert.IsTrue(result);
		Assert.AreEqual(Guid.Parse(validGuid), entraId);
	}

	[TestMethod]
	public void TryGetEntraId_WhenOidClaimIsMissing_ReturnsFalse()
	{
		// ARRANGE
		ClaimsPrincipal principal = new(
		[
			new(),
		]);

		// ACT
		bool result = principal.TryGetEntraId(out Guid _);

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryGetEntraId_WhenOidClaimIsNotAGuid_ReturnsFalse()
	{
		// ARRANGE
		ClaimsPrincipal principal = new(
		[
			new(
			[
				new("oid", "not-a-guid"),
			]),
		]);

		// ACT
		bool result = principal.TryGetEntraId(out Guid _);

		// ASSERT
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryGetEntraId_WhenMultipleClaimsWithOid_ReturnsFirstValidGuid()
	{
		// ARRANGE
		const string validGuid = "b1a7e8e2-1c2d-4e3f-9a4b-5c6d7e8f9a0b";
		ClaimsPrincipal principal = new(
		[
			new(
			[
				new("oid", validGuid),
				new("oid", "not-a-guid"),
			]),
		]);

		// ACT
		bool result = principal.TryGetEntraId(out Guid entraId);

		// ASSERT
		Assert.IsTrue(result);
		Assert.AreEqual(Guid.Parse(validGuid), entraId);
	}

	#endregion
}
