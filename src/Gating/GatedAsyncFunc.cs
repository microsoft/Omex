// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// A gated function, allows for conditionally executing code depending on the gate it
	/// belongs to.
	/// </summary>
	/// <typeparam name="TOut">the return type of the function</typeparam>
	public class GatedAsyncFunc<TOut> : GatedCode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GatedAsyncFunc{TOut}"/> class.
		/// </summary>
		/// <param name="func">the function to perform</param>
		/// <remarks>Creates a baseline gated function</remarks>
		public GatedAsyncFunc(Func<Task<TOut>> func) => Func = func;


		/// <summary>
		/// Initializes a new instance of the <see cref="GatedAsyncFunc{TOut}"/> class.
		/// </summary>
		/// <param name="gate">Gate the code snippet belongs to</param>
		/// <param name="func">the function to perform</param>
		public GatedAsyncFunc(IGate gate, Func<Task<TOut>> func)
			: base(gate) => Func = func;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="combination">gate combination the code snippet belongs to</param>
		/// <param name="func">the function to perform</param>
		public GatedAsyncFunc(GateCombination combination, Func<Task<TOut>> func)
			: base(combination) => Func = func;


		/// <summary>
		/// The function to perform
		/// </summary>
		public Func<Task<TOut>> Func { get; }
	}


	/// <summary>
	/// A gated function, allows for conditionally executing code depending on the gate it
	/// belongs to.
	/// </summary>
	/// <typeparam name="TIn">input type of function</typeparam>
	/// <typeparam name="TOut">the return type of the function</typeparam>
	public class GatedAsyncFunc<TIn, TOut> : GatedCode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GatedAsyncFunc{TIn, TOut}"/> class.
		/// </summary>
		/// <param name="func">the function to perform</param>
		/// <remarks>Creates a baseline gated function</remarks>
		public GatedAsyncFunc(Func<TIn, Task<TOut>> func) => Func = func;


		/// <summary>
		/// Initializes a new instance of the <see cref="GatedAsyncFunc{TIn, TOut}"/> class.
		/// </summary>
		/// <param name="gate">Gate the code snippet belongs to</param>
		/// <param name="func">The function to perform</param>
		public GatedAsyncFunc(IGate gate, Func<TIn, Task<TOut>> func)
			: base(gate) => Func = func;


		/// <summary>
		/// The function to perform
		/// </summary>
		public Func<TIn, Task<TOut>> Func { get; }
	}
}