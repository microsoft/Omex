/**************************************************************************************************
	GatedFunc.cs

	A gated function, allows for conditionally executing code depending on the gate it
	belongs to.
**************************************************************************************************/

#region Using Directives

using System;

#endregion

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// A gated function, allows for conditionally executing code depending on the gate it
	/// belongs to.
	/// </summary>
	/// <typeparam name="T">the return type of the function</typeparam>
	public class GatedFunc<T> : GatedCode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GatedFunc{T}"/> class.
		/// </summary>
		/// <param name="func">the function to perform</param>
		/// <remarks>Creates a baseline gated function</remarks>
		public GatedFunc(Func<T> func) => Func = func;


		/// <summary>
		/// Initializes a new instance of the <see cref="GatedFunc{T}"/> class.
		/// </summary>
		/// <param name="gate">Gate the code snippet belongs to</param>
		/// <param name="func">the function to perform</param>
		public GatedFunc(IGate gate, Func<T> func)
			: base(gate) => Func = func;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="combination">gate combination the code snippet belongs to</param>
		/// <param name="func">the function to perform</param>
		public GatedFunc(GateCombination combination, Func<T> func)
			: base(combination) => Func = func;


		/// <summary>
		/// The function to perform
		/// </summary>
		public Func<T> Func { get; }
	}


	/// <summary>
	/// A gated function, allows for conditionally executing code depending on the gate it
	/// belongs to.
	/// </summary>
	/// <typeparam name="T1">input type of function</typeparam>
	/// <typeparam name="T2">the return type of the function</typeparam>
	public class GatedFunc<T1, T2> : GatedCode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GatedFunc{T1, T2}"/> class.
		/// </summary>
		/// <param name="func">the function to perform</param>
		public GatedFunc(Func<T1, T2> func) => Func = func;


		/// <summary>
		/// Initializes a new instance of the <see cref="GatedFunc{T1, T2}"/> class.
		/// </summary>
		/// <param name="gate">Gate the code snippet belongs to</param>
		/// <param name="func">The function to perform</param>
		public GatedFunc(IGate gate, Func<T1, T2> func)
			: base(gate) => Func = func;


		/// <summary>
		/// The function to perform
		/// </summary>
		public Func<T1, T2> Func { get; }
	}
}
