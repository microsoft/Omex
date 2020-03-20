// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Gated code snippet
	/// </summary>
	public class GatedAction : GatedCode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GatedAction"/> class.
		/// </summary>
		/// <param name="action">the action to perform</param>
		/// <remarks>Creates a baseline gated action</remarks>
		public GatedAction(Action action) => Action = action;

		/// <summary>
		/// Initializes a new instance of the <see cref="GatedAction"/> class.
		/// </summary>
		/// <param name="gate">Gate the code snippet belongs to</param>
		/// <param name="action">the action to perform</param>
		public GatedAction(IGate gate, Action action)
			: base(gate) => Action = action;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="combination">Gate combination the code snippet belongs to.</param>
		/// <param name="action">The action to perform.</param>
		public GatedAction(GateCombination combination, Action action)
			: base(combination) => Action = action;

		/// <summary>
		/// The action to perform
		/// </summary>
		public Action Action { get; }
	}
}
