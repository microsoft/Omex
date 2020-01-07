// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Compatability;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.ServiceFabric;
using Microsoft.Omex.Extensions.ServiceFabric.Abstractions;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Class to start application with initialized DI and Logging
	/// </summary>
	/// <typeparam name="TContext">Service context implentation type used for logging, use NullServiceContext if it's not required</typeparam>
	public class OmexServiceFabricApplicationStartup<TContext> : OmexApplicationStartup<TContext>
		where TContext : class, IServiceContext
	{
		/// <summary>Create an instance of OmexApplicationStartup</summary>
		/// <param name="applicationName">Name of the application used for logging</param>
		public OmexServiceFabricApplicationStartup(string? applicationName = null) :
			base(applicationName)
		{
		}


		/// <summary>Register dependencies in DI</summary>
		protected override void Register(IServiceCollection collection) => collection.AddServiceFabricDependencies();
	}
}
