/**************************************************************************************************
	GateExtensions.cs

	Extension methods for IGate and IGateContext to select and perform gated code.
**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Extension methods for IGateContext to select and perform gated code
	/// and extension methods related to gating
	/// </summary>
	public static class GateExtensions
	{
		/// <summary>
		/// Perform an action that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="gate">single target gate</param>
		/// <param name="action">action delegate</param>
		public static void PerformAction(this IGateContext context, IGate gate, Action action) => SelectAction(context, new GatedAction[] { new GatedAction(gate, action) })();


		/// <summary>
		/// Perform an action that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="actions">array of gated actions</param>
		public static void PerformAction(this IGateContext context, params GatedAction[] actions) => SelectAction(context, actions)();


		/// <summary>
		/// Perform an action that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="actions">array of gated actions</param>
		public static void PerformAction(this IGateContext context, GatedCode.Modes mode, params GatedAction[] actions) => SelectAction(context, mode, null, actions)();


		/// <summary>
		/// Perform an action based on a condition that is specified using predicate.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="actions">The actions.</param>
		public static void PerformConditionalAction(this IGateContext context, Func<bool> conditionalPredicate,
			params GatedAction[] actions) => SelectAction(context, GatedCode.Modes.Scoped, conditionalPredicate, actions)();


		/// <summary>
		/// Perform an action that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="gate">single target gate</param>
		/// <param name="action">action delegate</param>
		public static Task PerformAction(this IGateContext context, IGate gate, Func<Task> action) => SelectAction(context, new GatedAsyncAction(gate, action))();


		/// <summary>
		/// Perform an action that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="actions">array of gated actions</param>
		public static Task PerformAction(this IGateContext context, params GatedAsyncAction[] actions) => SelectAction(context, actions)();


		/// <summary>
		/// Perform an action that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="actions">array of gated actions</param>
		public static Task PerformAction(this IGateContext context, GatedCode.Modes mode, params GatedAsyncAction[] actions) => SelectAction(context, mode, null, actions)();


		/// <summary>
		/// Perform an action based on a condition that is specified using predicate.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="actions">The actions.</param>
		public static Task PerformConditionalAction(this IGateContext context, Func<bool> conditionalPredicate,
			params GatedAsyncAction[] actions) => SelectAction(context, GatedCode.Modes.Scoped, conditionalPredicate, actions)();


		/// <summary>
		/// Perform a function that is applicable in the current gate context based on the passed predicate.
		/// </summary>
		/// <typeparam name="T">Generic type.</typeparam>
		/// <param name="context">The context.</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="functions">The functions.</param>
		/// <returns>Func to perform, (default if no applicable action found)</returns>
		public static T PerformConditionalFunction<T>(this IGateContext context, Func<bool> conditionalPredicate,
			params GatedFunc<T>[] functions) => SelectFunction(context, GatedCode.Modes.Scoped, conditionalPredicate, functions)();


		/// <summary>
		/// Perform a function that is applicable in the current gate context
		/// </summary>
		/// <typeparam name="T">the return type of the function</typeparam>
		/// <param name="context">gate context</param>
		/// <param name="gate">target gate</param>
		/// <param name="function">function delegate</param>
		/// <returns>instance of T (default if no applicable action found)</returns>
		public static T PerformFunction<T>(this IGateContext context, IGate gate, Func<T> function) => SelectFunction(context, new GatedFunc<T>[] { new GatedFunc<T>(gate, function) })();


		/// <summary>
		/// Perform a function that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="functions">array of gated functions</param>
		/// <typeparam name="T">the return type of the function</typeparam>
		/// <returns>instance of T (default if no applicable action found)</returns>
		public static T PerformFunction<T>(this IGateContext context, params GatedFunc<T>[] functions) => SelectFunction(context, functions)();


		/// <summary>
		/// Perform a function that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="functions">array of gated functions</param>
		/// <typeparam name="T">the return type of the function</typeparam>
		/// <returns>instance of T (default if no applicable action found)</returns>
		public static T PerformFunction<T>(this IGateContext context, GatedCode.Modes mode, params GatedFunc<T>[] functions) => SelectFunction(context, mode, null, functions)();


		/// <summary>
		/// Perform a function that is applicable in the current gate context based on the passed predicate.
		/// </summary>
		/// <typeparam name="T">Generic type.</typeparam>
		/// <param name="context">The context.</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="functions">The functions.</param>
		/// <returns>Func to perform, (default if no applicable action found)</returns>
		public static Task<T> PerformConditionalFunction<T>(this IGateContext context, Func<bool> conditionalPredicate,
			params GatedAsyncFunc<T>[] functions) => SelectFunction(context, GatedCode.Modes.Scoped, conditionalPredicate, functions)();


		/// <summary>
		/// Perform a function that is applicable in the current gate context
		/// </summary>
		/// <typeparam name="T">the return type of the function</typeparam>
		/// <param name="context">gate context</param>
		/// <param name="gate">target gate</param>
		/// <param name="function">function delegate</param>
		/// <returns>instance of T (default if no applicable action found)</returns>
		public static Task<T> PerformFunction<T>(this IGateContext context, IGate gate, Func<Task<T>> function) => SelectFunction(context, new GatedAsyncFunc<T>(gate, function))();


		/// <summary>
		/// Perform a function that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="functions">array of gated functions</param>
		/// <typeparam name="T">the return type of the function</typeparam>
		/// <returns>instance of T (default if no applicable action found)</returns>
		public static Task<T> PerformFunction<T>(this IGateContext context, params GatedAsyncFunc<T>[] functions) => SelectFunction(context, functions)();


		/// <summary>
		/// Perform a function that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="functions">array of gated functions</param>
		/// <typeparam name="T">the return type of the function</typeparam>
		/// <returns>instance of T (default if no applicable action found)</returns>
		public static Task<T> PerformFunction<T>(this IGateContext context, GatedCode.Modes mode, params GatedAsyncFunc<T>[] functions) => SelectFunction(context, mode, null, functions)();


		/// <summary>
		/// Perform each action that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gated context</param>
		/// <param name="actions">array of gated actions</param>
		public static void PerformEachAction(this IGateContext context, params GatedAction[] actions) => PerformEachAction(context, GatedCode.Modes.Scoped, actions);


		/// <summary>
		/// Perform each action that is applicable in the current gate context
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="actions">array of gated actions</param>
		public static void PerformEachAction(this IGateContext context, GatedCode.Modes mode, params GatedAction[] actions)
		{
			Array.ForEach(actions, (action) => PerformAction(context, mode, action));
		}


		/// <summary>
		/// Perform each function that is applicable in the current gate context. The input parameter
		/// is passed to the first applicable function, and the result of that function is used as the input
		/// for the next applicable function, etc. This allows for chaining multiple functions, each having
		/// an opportunity to modify the input.
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="input">the input object</param>
		/// <param name="functions">array of gated functions</param>
		/// <typeparam name="T">the return type of the function</typeparam>
		/// <returns>instance of T (default if no applicable function found)</returns>
		public static T PerformEachFunction<T>(this IGateContext context, T input, params GatedFunc<T, T>[] functions)
		{
			return PerformEachFunction(context, GatedCode.Modes.Scoped, input, functions);
		}


		/// <summary>
		/// Perform each function that is applicable in the current gate context. The input parameter
		/// is passed to the first applicable function, and the result of that function is used as the input
		/// for the next applicable function, etc. This allows for chaining multiple functions, each having
		/// an opportunity to modify the input.
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="input">the input object</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="functions">array of gated functions</param>
		/// <typeparam name="T">the return type of the function</typeparam>
		/// <returns>instance of T (default if no applicable action found)</returns>
		public static T PerformEachFunction<T>(this IGateContext context, GatedCode.Modes mode, T input, params GatedFunc<T, T>[] functions)
		{
			Array.ForEach(functions,
				(function) =>
				{
					input = SelectFunction(context, mode, function)(input);
				}
			);
			return input;
		}


		/// <summary>
		/// Runs the given action in the scope of the gates associated with it
		/// </summary>
		/// <remarks>
		/// All code invoked by the action will consider the gates active.
		/// </remarks>
		/// <param name="gateContext">gate context</param>
		/// <param name="gatedAction">gated action to run</param>
		public static void PerformGatedActionInScope(this IGateContext gateContext, GatedAction gatedAction)
		{
			foreach (IGate gate in gatedAction.Gates.Gates)
			{
				gateContext.EnterScope(gate);
			}

			try
			{
				gatedAction.Action?.Invoke();
			}
			finally
			{
				foreach (IGate gate in gatedAction.Gates.Gates)
				{
					gateContext.ExitScope(gate);
				}
			}
		}


		/// <summary>
		/// Executes the given function in the scope of the gates associated with it
		/// </summary>
		/// <remarks>
		/// All code invoked by the function will consider the gates active.
		/// </remarks>
		/// <typeparam name="T">type of the function's result</typeparam>
		/// <param name="gateContext">gate context</param>
		/// <param name="gatedFunction">gated function to execute</param>
		/// <returns>result returned by the function</returns>
		public static T PerformGatedFunctionInScope<T>(this IGateContext gateContext, GatedFunc<T> gatedFunction)
		{
			foreach (IGate gate in gatedFunction.Gates.Gates)
			{
				gateContext.EnterScope(gate);
			}

			try
			{
				return gatedFunction.Func == null ? default(T) : gatedFunction.Func();
			}
			finally
			{
				foreach (IGate gate in gatedFunction.Gates.Gates)
				{
					gateContext.ExitScope(gate);
				}
			}
		}


		/// <summary>
		/// Select an applicable action without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="actions">array of gated actions</param>
		/// <returns>an action to perform, never null</returns>
		public static Action SelectAction(this IGateContext context, params GatedAction[] actions) => SelectAction(context, GatedCode.Modes.Scoped, null, actions);


		/// <summary>
		/// Select an applicable action without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="actions">array of gated actions</param>
		/// <returns>an action to perform, never null</returns>
		public static Action SelectAction(this IGateContext context, GatedCode.Modes mode, Func<bool> conditionalPredicate, params GatedAction[] actions)
		{
			Tuple<GatedAction, IGate[]> action = SelectCode(context, conditionalPredicate, actions);
			if (action != null && action.Item1 != null)
			{
				return new Action(
					() =>
					{
						context.EnterScopes(mode, action.Item2);

						try
						{
							action.Item1.Action?.Invoke();
						}
						finally
						{
							context.ExitScopes(mode, action.Item2);
						}
					});
			}

			return new Action(() => { });
		}


		/// <summary>
		/// Select an applicable action without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="actions">array of gated actions</param>
		/// <returns>an action to perform, never null</returns>
		public static Func<Task> SelectAction(this IGateContext context, params GatedAsyncAction[] actions) => SelectAction(context, GatedCode.Modes.Scoped, null, actions);


		/// <summary>
		/// Select an applicable action without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="actions">array of gated actions</param>
		/// <returns>an action to perform, never null</returns>
		public static Func<Task> SelectAction(this IGateContext context, GatedCode.Modes mode, Func<bool> conditionalPredicate, params GatedAsyncAction[] actions)
		{
			return SelectAction(context, mode, conditionalPredicate, actions, false);
		}


		/// <summary>
		/// Select an applicable action without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="actions">array of gated actions</param>
		/// <param name="keepContext">controls whether async operation should keep the synchronization context</param>
		/// <remarks>
		/// keep context should be set to true in code which is async end to end,
		/// and false in other cases to avoid deadlocking
		/// </remarks>
		/// <returns>an action to perform, never null</returns>
		public static Func<Task> SelectAction(this IGateContext context, GatedCode.Modes mode, Func<bool> conditionalPredicate, GatedAsyncAction[] actions, bool keepContext = false)
		{
			Tuple<GatedAsyncAction, IGate[]> action = SelectCode(context, conditionalPredicate, actions);
			if (action?.Item1 == null)
			{
				return s_noOpAction;
			}

			return async () =>
			{
				context.EnterScopes(mode, action.Item2);

				try
				{
					if (action.Item1?.Action != null)
					{
						await action.Item1.Action().ConfigureAwait(keepContext);
					}
				}
				finally
				{
					context.ExitScopes(mode, action.Item2);
				}
			};
		}


		/// <summary>
		/// Empty awaitable action
		/// </summary>
		private static readonly Func<Task> s_noOpAction = () => Task.FromResult(true);


		/// <summary>
		/// Select an applicable function without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="functions">array of gated functions</param>
		/// <returns>a function to perform, never null</returns>
		public static Func<T> SelectFunction<T>(this IGateContext context, params GatedFunc<T>[] functions)
		{
			return SelectFunction(context, GatedCode.Modes.Scoped, null, functions);
		}


		/// <summary>
		/// Select an applicable function without executing it
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="functions">array of gated functions</param>
		/// <returns>a function to perform, never null</returns>
		public static Func<T> SelectFunction<T>(this IGateContext context, GatedCode.Modes mode, Func<bool> conditionalPredicate,
			params GatedFunc<T>[] functions)
		{
			Tuple<GatedFunc<T>, IGate[]> action = SelectCode(context, conditionalPredicate, functions);
			if (action != null && action.Item1 != null)
			{
				return new Func<T>(
					() =>
					{
						context.EnterScopes(mode, action.Item2);

						try
						{
							if (action.Item1.Func != null)
							{
								return action.Item1.Func();
							}
						}
						finally
						{
							context.ExitScopes(mode, action.Item2);
						}
						return default(T);
					});
			}

			return new Func<T>(() => default(T));
		}


		/// <summary>
		/// Select an applicable function without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="functions">array of gated functions</param>
		/// <returns>a function to perform, never null</returns>
		public static Func<T, T> SelectFunction<T>(this IGateContext context, params GatedFunc<T, T>[] functions)
		{
			return SelectFunction(context, GatedCode.Modes.Scoped, functions);
		}


		/// <summary>
		/// Select an applicable function without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="functions">array of gated functions</param>
		/// <returns>a function to perform, never null</returns>
		public static Func<T, T> SelectFunction<T>(this IGateContext context, GatedCode.Modes mode, params GatedFunc<T, T>[] functions)
		{
			Tuple<GatedFunc<T, T>, IGate[]> action = SelectCode(context, null, functions);
			if (action != null && action.Item1 != null)
			{
				return new Func<T, T>(
					(t) =>
					{
						context.EnterScopes(mode, action.Item2);

						try
						{
							if (action.Item1.Func != null)
							{
								return action.Item1.Func(t);
							}
						}
						finally
						{
							context.ExitScopes(mode, action.Item2);
						}

						return t;
					});
			}

			return new Func<T, T>((t) => t);
		}


		/// <summary>
		/// Select an applicable function without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="functions">array of gated functions</param>
		/// <returns>a function to perform, never null</returns>
		public static Func<Task<T>> SelectFunction<T>(this IGateContext context, params GatedAsyncFunc<T>[] functions)
		{
			return SelectFunction(context, GatedCode.Modes.Scoped, null, functions);
		}


		/// <summary>
		/// Select an applicable function without executing it
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="functions">array of gated functions</param>
		/// <returns>a function to perform, never null</returns>
		public static Func<Task<T>> SelectFunction<T>(this IGateContext context, GatedCode.Modes mode, Func<bool> conditionalPredicate,
			params GatedAsyncFunc<T>[] functions)
		{
			return SelectFunction(context, mode, conditionalPredicate, functions, false);
		}


		/// <summary>
		/// Select an applicable function without executing it
		/// </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="functions">array of gated functions</param>
		/// <param name="keepContext">controls whether async operation should keep the synchronization context</param>
		/// <remarks>
		/// keep context should be set to true in code which is async end to end,
		/// and false in other cases to avoid deadlocking
		/// </remarks>
		/// <returns>a function to perform, never null</returns>
		public static Func<Task<T>> SelectFunction<T>(this IGateContext context, GatedCode.Modes mode, Func<bool> conditionalPredicate,
			GatedAsyncFunc<T>[] functions, bool keepContext)
		{
			Tuple<GatedAsyncFunc<T>, IGate[]> action = SelectCode(context, conditionalPredicate, functions);
			if (action?.Item1 == null)
			{
				return () => Task.FromResult(default(T));
			}

			return async () =>
			{
				context.EnterScopes(mode, action.Item2);

				try
				{
					if (action.Item1.Func != null)
					{
						return await action.Item1.Func().ConfigureAwait(keepContext);
					}
				}
				finally
				{
					context.ExitScopes(mode, action.Item2);
				}

				return default(T);
			};
		}


		/// <summary>
		/// Select an applicable function without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="functions">array of gated functions</param>
		/// <returns>a function to perform, never null</returns>
		public static Func<T, Task<T>> SelectFunction<T>(this IGateContext context, params GatedAsyncFunc<T, T>[] functions)
		{
			return SelectFunction(context, GatedCode.Modes.Scoped, functions);
		}


		/// <summary>
		/// Select an applicable function without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="functions">array of gated functions</param>
		/// <returns>a function to perform, never null</returns>
		public static Func<T, Task<T>> SelectFunction<T>(this IGateContext context, GatedCode.Modes mode, params GatedAsyncFunc<T, T>[] functions)
		{
			return SelectFunction(context, mode, functions, false);
		}


		/// <summary>
		/// Select an applicable function without executing it
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">mode to run the gated code in</param>
		/// <param name="functions">array of gated functions</param>
		/// <param name="keepContext">controls whether async operation should keep the synchronization context</param>
		/// <remarks>
		/// keep context should be set to true in code which is async end to end,
		/// and false in other cases to avoid deadlocking
		/// </remarks>
		/// <returns>a function to perform, never null</returns>
		public static Func<T, Task<T>> SelectFunction<T>(this IGateContext context, GatedCode.Modes mode, GatedAsyncFunc<T, T>[] functions, bool keepContext)
		{
			Tuple<GatedAsyncFunc<T, T>, IGate[]> action = SelectCode(context, null, functions);
			if (action != null && action.Item1 != null)
			{
				return async t =>
				{
					context.EnterScopes(mode, action.Item2);

					try
					{
						if (action.Item1.Func != null)
						{
							return await action.Item1.Func(t).ConfigureAwait(keepContext);
						}
					}
					finally
					{
						context.ExitScopes(mode, action.Item2);
					}

					return t;
				};
			}

			return Task.FromResult;
		}


		/// <summary>
		/// Select a code
		/// </summary>
		/// <typeparam name="T">type of gated code</typeparam>
		/// <param name="context">gate context</param>
		/// <param name="conditionalPredicate">The conditional predicate.</param>
		/// <param name="code">array of gated code</param>
		/// <returns>Tuple of gated code and applicaple gate.</returns>
		public static Tuple<T, IGate[]> SelectCode<T>(this IGateContext context, Func<bool> conditionalPredicate, params T[] code) where T : GatedCode
		{
			if (context == null || code == null)
			{
				return null;
			}

			Lazy<bool> conditionalPredicateIsFalse = new Lazy<bool>(
				() => conditionalPredicate != null && !conditionalPredicate());
			IGate[] gates = null;
			T gatedCode = Array.Find(code,
				t =>
				{
					if (t != null)
					{
						if (t.IsBaselineCode)
						{
							return true;
						}

						if (!conditionalPredicateIsFalse.Value)
						{
							IGate[] applicableGates;
							if (t.Gates.IsApplicable(context, out applicableGates))
							{
								gates = applicableGates;
								return true;
							}
						}
					}

					return false;
				});

			if (gatedCode == null)
			{
				return null;
			}

			return new Tuple<T, IGate[]>(gatedCode, gates);
		}


		/// <summary>
		/// Enter a scope for gated code
		/// </summary>
		/// <param name="context">the gate context</param>
		/// <param name="mode">mode that code should be performed (e.g. scoped or not)</param>
		/// <param name="gate">the gate to enter scope for</param>
		public static void EnterScope(this IGateContext context, GatedCode.Modes mode, IGate gate)
		{
			if (context.ShouldBeScoped((mode & GatedCode.Modes.EnterScope), gate))
			{
				context.EnterScope(gate);
			}
		}


		/// <summary>
		/// Enters a collection of scopes in a nested way.
		/// </summary>
		/// <param name="context">The gate context.</param>
		/// <param name="mode">The mode that code should be performed.</param>
		/// <param name="gates">The array of gates for the scope.</param>
		public static void EnterScopes(this IGateContext context, GatedCode.Modes mode, IGate[] gates)
		{
			if (gates == null)
			{
				return;
			}

			foreach (IGate gate in gates)
			{
				EnterScope(context, mode, gate);
			}
		}


		/// <summary>
		/// Exits a collection of scopes in a reverse order.
		/// </summary>
		/// <param name="context">The gate context.</param>
		/// <param name="mode">The mode that code should be performed.</param>
		/// <param name="gates">The array of gates for exiting the scope.</param>
		public static void ExitScopes(this IGateContext context, GatedCode.Modes mode, IGate[] gates)
		{
			if (gates == null)
			{
				return;
			}

			for (int i = gates.Length - 1; i >= 0; i--)
			{
				ExitScope(context, mode, gates[i]);
			}
		}


		/// <summary>
		/// Exit a scope for gated code
		/// </summary>
		/// <param name="context">the gate context</param>
		/// <param name="mode">mode that code should be performed (e.g. scoped or not)</param>
		/// <param name="gate">the gate to exit scope for</param>
		public static void ExitScope(this IGateContext context, GatedCode.Modes mode, IGate gate)
		{
			if (context.ShouldBeScoped((mode & GatedCode.Modes.ExitScope), gate))
			{
				context.ExitScope(gate);
			}
		}


		/// <summary>
		/// Should the context scope the code
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="mode">preferred mode for code</param>
		/// <param name="gate">gate to run</param>
		/// <returns>true if the context should scope the code, false otherwise</returns>
		private static bool ShouldBeScoped(this IGateContext context, GatedCode.Modes mode, IGate gate)
		{
			if (gate == null)
			{
				return false;
			}

			if (context == null)
			{
				return false;
			}

			if ((mode & GatedCode.Modes.EnterScope) == GatedCode.Modes.EnterScope ||
				(mode & GatedCode.Modes.ExitScope) == GatedCode.Modes.ExitScope)
			{
				return true;
			}

			if (context.Request != null && context.Request.RequestedGateIds != null &&
				context.Request.RequestedGateIds.Contains(gate.Name))
			{
				return true;
			}

			return false;
		}


		/// <summary>
		/// Retrieve the current active gates as a string
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="separator">optional separator</param>
		/// <returns>string representation</returns>
		public static string CurrentGatesAsString(this IGateContext context, string separator = null)
		{
			return context != null ?
				GatesAsString(context.CurrentGates, separator) : null;
		}


		/// <summary>
		/// Retrieve the activated gates as a string
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="separator">optional separator</param>
		/// <returns>string representation</returns>
		public static string ActivatedGatesAsString(this IGateContext context, string separator = null)
		{
			return context != null && context.ActivatedGates != null ?
				GatesAsString(context.ActivatedGates.Select(gate => gate.Name), separator) : null;
		}


		/// <summary>
		/// Retrieve the requested gates as a string
		/// </summary>
		/// <param name="context">gate context</param>
		/// <param name="separator">optional separator</param>
		/// <returns>string representation</returns>
		public static string RequestedGatesAsString(this IGateContext context, string separator = null)
		{
			return context != null && context.Request != null ?
				GatesAsString(context.Request.RequestedGateIds, separator) : null;
		}


		/// <summary>
		/// Join a set of gates to a string
		/// </summary>
		/// <param name="gates">gates</param>
		/// <param name="separator">optional separator</param>
		/// <returns>string representation</returns>
		private static string GatesAsString(IEnumerable<string> gates, string separator)
		{
			if (gates == null || !gates.Any())
			{
				return null;
			}
			return string.Join(separator ?? string.Empty, gates);
		}


		/// <summary>
		/// Creates gate entry for JSON object.
		/// </summary>
		/// <param name="gate">The gate that will be mapped on frontend.</param>
		/// <param name="context">The gate context</param>
		/// <returns>JSON gate entry.</returns>
		public static KeyValuePair<string, string> CreateGateEntry(this IGate gate, IGateContext context)
		{
			// work around the issue where having a '.' character in gate name always returns gate not applicable
			string key = gate.Name.Replace('.', '_');
			bool isGateApplicable = context.IsGateApplicable(gate);
			string value = new JavaScriptSerializer().Serialize(isGateApplicable.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
			return new KeyValuePair<string, string>(key, value);
		}
	}
}
