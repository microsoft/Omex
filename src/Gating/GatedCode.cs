// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Gated code snippet
	/// </summary>
	public class GatedCode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GatedCode"/> class.
		/// </summary>
		/// <remarks>Creates a baseline gated code</remarks>
		public GatedCode() => IsBaselineCode = true;


		/// <summary>
		/// Initializes a new instance of the <see cref="GatedCode"/> class.
		/// </summary>
		/// <param name="gate">Gate the code snippet belongs to</param>
		public GatedCode(IGate gate)
		{
			IsBaselineCode = gate == null;
			if (gate != null)
			{
				if (gate.ReleasePlan != null)
				{
					if (gate.ReleasePlan.Length == 0)
					{
						Gates = new GatesNone();
					}
					else
					{
						Gates = new GatesAny(gate.ReleasePlan);
					}
				}
				else
				{
					Gates = new GatesAny(gate);
				}
			}
			else
			{
				Gates = new GatesNone();
			}
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="combination">gate combination the code snippet belongs to</param>
		public GatedCode(GateCombination combination)
		{
			IsBaselineCode = combination == null;
			Gates = combination ?? new GatesNone();
		}


		/// <summary>
		/// Does this code snippet belong outside gates, i.e. is baseline code
		/// </summary>
		public bool IsBaselineCode { get; }


		/// <summary>
		/// Gate(s) the code snippet belongs to
		/// </summary>
		public GateCombination Gates { get; }


		/// <summary>
		/// Mode that the gated code should be ran in
		/// </summary>
		[Flags]
		public enum Modes
		{
			/// <summary>
			/// Run the gated code outside a gate context scope
			/// </summary>
			/// <remarks>This runs the gated code without alerting the
			/// gate context about the code being run</remarks>
			None = 0,


			/// <summary>
			/// Enter a gated context scope before running the code
			/// </summary>
			EnterScope,


			/// <summary>
			/// Exit a gated context scope after running the code
			/// </summary>
			ExitScope,


			/// <summary>
			/// Run the gated code inside a gate context scope
			/// </summary>
			/// <remarks>This is the default mode and adds the name of
			/// the gate to the list of active gates</remarks>
			Scoped = EnterScope | ExitScope,
		}
	}
}