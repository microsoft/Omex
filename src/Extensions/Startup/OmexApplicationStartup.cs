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
using Microsoft.Omex.Extensions.TimedScopes;

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
		/// <param name="applicationName">Name of the application used for logging</param>
		public OmexApplicationStartup(string? applicationName = null) =>
			ServiceTypeName = applicationName ?? GetDefaultName();


		/// <summary>
		/// Run function to execute
		/// This method should be used as an entry point of the application
		/// </summary>
		/// <param name="function">Function to execute</param>
		/// <param name="typeRegistration">Action to register types in DI</param>
		/// <param name="typeResolution">Action to resolve types from DI</param>
		/// <returns></returns>
		protected async Task RunAsync(
			Func<Task> function,
			Action<IServiceCollection>? typeRegistration = null,
			Action<IServiceProvider>? typeResolution = null)
		{
			try
			{
				m_typeRegistration = typeRegistration;
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
		protected virtual void Register(IServiceCollection collection) { }


		/// <summary>Resolve dependencies from DI</summary>
		protected virtual void Resolve(IServiceProvider provider) { }


		/// <summary>
		/// Method will create DI container register types and resolve spesified type from it
		/// </summary>
		/// <typeparam name="T">Type of startup objest </typeparam>
		/// <param name="objctsToRegister"></param>
		/// <returns>Instance of the object returned from DI</returns>
		protected T GetStartupObject<T>(params object[] objctsToRegister)
			where T : class
		{
			IServiceCollection collection = new ServiceCollection();

			collection.AddTransient<T>();

			foreach (object obj in objctsToRegister)
			{
				collection.AddSingleton(obj.GetType(), obj);
			}

			AggregatedRegister(collection);

			IServiceProvider provider = collection
				.BuildServiceProvider(new ServiceProviderOptions
				{
					ValidateOnBuild = true,
					ValidateScopes = true
				});

			AggregatedResolve(provider);

			return provider.GetService<T>();
		}


		/// <summary>Log successful start</summary>
		protected virtual void LogSuccess(int processId, string serviceName) { }


		/// <summary>Log failure</summary>
		protected virtual void LogFailure(Exception exception) { }


		/// <summary>Name of the service used for logging</summary>
		protected string ServiceTypeName { get; }


		private void AggregatedRegister(IServiceCollection collection)
		{
			m_typeRegistration?.Invoke(collection);

			Register(collection);

			collection
				.AddOmexLogging<TContext>()
				.AddTimedScopes();
		}


		private void AggregatedResolve(IServiceProvider provider)
		{
			provider.InitializeOmexCompatabilityClasses();

			Resolve(provider);

			m_typeResolution?.Invoke(provider);
		}


		private Action<IServiceCollection>? m_typeRegistration;


		private Action<IServiceProvider>? m_typeResolution;


		private string GetDefaultName() => Assembly.GetEntryAssembly()?.GetName().Name ?? GetType().Name;
	}
}
