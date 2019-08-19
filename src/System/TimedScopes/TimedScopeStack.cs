// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Linked stack of active timed scopes
	/// </summary>
	[Serializable]
	public class TimedScopeStack
	{
		/// <summary>
		/// Root item for all stacks
		/// </summary>
		public static TimedScopeStack Root { get; } = new TimedScopeStack();


		/// <summary>
		/// Checks if node is root
		/// </summary>
		public bool IsRoot => ReferenceEquals(this, Parent);


		/// <summary>
		/// Adds a new item to the stack
		/// </summary>
		/// <param name="item">Data item to store</param>
		/// <returns>Stack with the new item on it</returns>
		public TimedScopeStack Push(TimedScope item) => new TimedScopeStack(item, this);


		/// <summary>
		/// Remove item from the stack and return the new stack
		/// </summary>
		/// <param name="scope">TimedScope stored at the top od the stack</param>
		/// <returns>New stack with the top item removed</returns>
		public TimedScopeStack Pop(out TimedScope scope)
		{
			scope = Item;
			return Parent;
		}


		/// <summary>
		/// Retrieve item from the top of the stack
		/// </summary>
		/// <returns></returns>
		public TimedScope Peek() => Item;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="item">Item stored in the stack</param>
		/// <param name="parent">Parent of this stack</param>
		private TimedScopeStack(TimedScope item, TimedScopeStack parent)
		{
			Code.ExpectsArgument(parent, nameof(parent), TaggingUtilities.ReserveTag(0x23817061 /* tag_96xb7 */));

			Item = item;
			Parent = parent;
		}


		/// <summary>
		/// Constructor
		/// </summary>
		private TimedScopeStack()
		{
			Parent = this;
		}


		/// <summary>
		/// Parent of this node
		/// </summary>
		private TimedScopeStack Parent { get; }


		/// <summary>
		/// Data item stored in this node
		/// </summary>
		private TimedScope Item { get; }
	}
}
