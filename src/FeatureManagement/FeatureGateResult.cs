// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.Extensions.FeatureManagement;

/// <summary>
/// The result of checking a customer against a specific experiment feature gate.
/// </summary>
/// <param name="InTreatment">A value indicating whether the customer has been assigned to a treatment flight.</param>
/// <param name="Value">The optional feature gate value associated with the treatment flight.</param>
public readonly record struct FeatureGateResult(bool InTreatment, string? Value = null);
