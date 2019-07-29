// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.Omex.System.Diagnostics;

namespace Microsoft.Omex.System.AspNetCore
{
	/// <summary>
	/// Context data stored on LogicalCallContext
	/// </summary>
	public class LogicalCallContext : LogicalCallContextBase
	{
		private static readonly AsyncLocal<ConcurrentDictionary<string, object>> s_dataAsyncLocal = new AsyncLocal<ConcurrentDictionary<string, object>>();


		private static readonly AsyncLocal<IImmutableDictionary<string, object>> s_dictionaryAsyncLocal = new AsyncLocal<IImmutableDictionary<string, object>>();


		private static readonly AsyncLocal<IImmutableStack<IImmutableDictionary<string, object>>> s_stackAsyncLocal = new AsyncLocal<IImmutableStack<IImmutableDictionary<string, object>>>();


		public LogicalCallContext(IMachineInformation machineInformation) : base(machineInformation)
		{
		}


		/// <summary> Gets or sets the context data </summary>
		protected override ConcurrentDictionary<string, object> ContextData
		{
			get => s_dataAsyncLocal.Value;
			set => s_dataAsyncLocal.Value = value;
		}


		/// <summary> Gets or sets the context dictionary </summary>
		protected override IImmutableDictionary<string, object> ContextDictionary
		{
			get => s_dictionaryAsyncLocal.Value;
			set => s_dictionaryAsyncLocal.Value = value;
		}


		/// <summary> Gets or sets the context stack  </summary>
		protected override IImmutableStack<IImmutableDictionary<string, object>> ContextStack
		{
			get => s_stackAsyncLocal.Value;
			set => s_stackAsyncLocal.Value = value;
		}
	}
}
