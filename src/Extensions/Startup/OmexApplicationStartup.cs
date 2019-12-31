// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Compatability;
using Microsoft.Omex.Extensions.Logging;

namespace Microsoft.Omex.Extensions.ServiceFabric
{
	/// <summary>
	/// Class to start application with initialized DI and Logging
	/// </summary>
	/// <typeparam name="TContext">Service context implentation type used for logging, use NullServiceContext if it's not required</typeparam>
	public class OmexApplicationStartup<TContext>
		where TContext : class, IServiceContext
	{
		/// <summary>Create an instance of OmexApplicationStartup</summary>
		/// <param name="serviceTypeName">Name of the service used for logging</param>
		public OmexApplicationStartup(string? serviceTypeName = null) =>
			ServiceTypeName = serviceTypeName ?? GetDefaultName();


		/// <summary>Run function to execute</summary>
		/// <param name="function">Function to execute</param>
		/// <param name="typeRegestration">Action to register types in DI</param>
		/// <param name="typeResolution">Action to resolve types from DI</param>
		/// <returns></returns>
		protected async Task RunAsync(
			Func<Task> function,
			Action<IServiceCollection>? typeRegestration = null,
			Action<IServiceProvider>? typeResolution = null)
		{
			try
			{
				m_typeRegestration = typeRegestration;
				m_typeResolution = typeResolution;
				Task task = function();
				LogSuccess(Process.GetCurrentProcess().Id, ServiceTypeName);
				await task;
			}
			catch (Exception exception)
			{
				LogFailure(exception);
				throw;
			}
		}


		/// <summary>Register dependencies in DI</summary>
		protected virtual IServiceCollection Register(IServiceCollection collection)
		{
			collection
				.AddOmexLogging<TContext>()
				.AddTimedScopes();
			m_typeRegestration?.Invoke(collection);
			return collection;
		}


		/// <summary>Resolve dependencies from DI</summary>
		protected virtual IServiceProvider Resolve(IServiceProvider provider)
		{
			provider.InitializeOmexCompatabilityClasses();
			m_typeResolution?.Invoke(provider);
			return provider;
		}


		/// <summary></summary>
		/// <typeparam name="T">Type of startup objest </typeparam>
		/// <param name="objctsToRegister"></param>
		/// <returns></returns>
		protected T GetStartupObject<T>(params object[] objctsToRegister)
			where T : class
		{
			IServiceCollection collection = new ServiceCollection();

			collection.AddSingleton<T>();

			foreach (object obj in objctsToRegister)
			{
				collection.AddSingleton(obj.GetType(), obj);
			}

			Register(collection);

			IServiceProvider provider = collection
				.BuildServiceProvider(new ServiceProviderOptions
				{
					ValidateOnBuild = true,
					ValidateScopes = true
				});

			Resolve(provider);

			return provider.GetService<T>();
		}


		/// <summary>Log successful start</summary>
		protected virtual void LogSuccess(int processId, string serviceName) { }


		/// <summary>Log failure</summary>
		protected virtual void LogFailure(Exception exception) { }


		/// <summary>Name of the service used for logging</summary>
		protected string ServiceTypeName { get; }


		private Action<IServiceCollection>? m_typeRegestration;


		private Action<IServiceProvider>? m_typeResolution;


		private string GetDefaultName()
		{
			Assembly? assembly = Assembly.GetEntryAssembly() 
				?? Assembly.GetCallingAssembly()
				?? Assembly.GetExecutingAssembly();

			return assembly.GetName().Name ?? GetType().Name;
		}
	}
}
