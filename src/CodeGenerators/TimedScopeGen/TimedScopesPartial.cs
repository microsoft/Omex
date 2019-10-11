// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.CodeGenerators.TimedScopeGen
{
	/// <summary>
	/// Partial definitions for the TimedScope class
	/// </summary>
	public partial class TimedScope
	{
		/// <summary>
		/// Gets the description
		/// </summary>
		/// <returns>Description</returns>
		public string GetDescription() => Description ?? string.Join(string.Empty, Text);
	}
}