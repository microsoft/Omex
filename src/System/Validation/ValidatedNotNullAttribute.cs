// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.System.Validation
{
    /// <summary>
    /// Indicates to Code analysis tools that a method validates a particular parameter to not be null
    /// </summary>
    internal sealed class ValidatedNotNullAttribute : Attribute {}
}