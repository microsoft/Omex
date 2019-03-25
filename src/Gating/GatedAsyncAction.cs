// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Gated code snippet
	/// </summary>
	public class GatedAsyncAction : GatedCode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GatedAsyncAction"/> class.
		/// </summary>
		/// <param name="action">the action to perform</param>
		/// <remarks>Creates a baseline gated action</remarks>
		public GatedAsyncAction(Func<Task> action) => Action = action;


		/// <summary>
		/// Initializes a new instance of the <see cref="GatedAsyncAction"/> class.
		/// </summary>
		/// <param name="gate">Gate the code snippet belongs to</param>
		/// <param name="action">the action to perform</param>
		public GatedAsyncAction(IGate gate, Func<Task> action)
			: base(gate) => Action = action;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="combination">Gate combination the code snippet belongs to.</param>
		/// <param name="action">The action to perform.</param>
		public GatedAsyncAction(GateCombination combination, Func<Task> action)
			: base(combination) => Action = action;


		/// <summary>
		/// The action to perform
		/// </summary>
		public Func<Task> Action { get; }
	}
}