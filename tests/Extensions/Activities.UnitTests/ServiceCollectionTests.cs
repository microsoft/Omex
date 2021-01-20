// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System;
using Microsoft.Omex.Extensions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Moq;
using Microsoft.Extensions.Options;

namespace Microsoft.Omex.Extensions.TimedScopes.UnitTests
{
	[TestClass]
	public class ServiceCollectionTests
	{
		[DataTestMethod]
		[DataRow(typeof(ActivitySource), typeof(ActivitySource))]
		[DataRow(typeof(IActivityStopObserver), typeof(ActivityStopObserver))]
		public void AddOmexActivitySource_TypesRegistered(Type typeToResolve, Type implementinType)
		{
			object obj = new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection.AddOmexActivitySource();
				})
				.Build()
				.Services
				.GetRequiredService(typeToResolve);

			Assert.IsNotNull(obj);
			Assert.IsInstanceOfType(obj, implementinType);
		}

		[TestMethod]
		public void AddOmexActivitySource_HostedServicesRegiestered()
		{
			Type[] types = new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection.AddOmexActivitySource();
				})
				.Build()
				.Services
				.GetRequiredService<IEnumerable<IHostedService>>()
				.Select(s => s.GetType())
				.ToArray();

			Assert.AreEqual(2, types.Length);
			CollectionAssert.Contains(types, typeof(ActivityListenerInitializerService));
			CollectionAssert.Contains(types, typeof(DiagnosticsObserversInitializer));
		}

		[TestMethod]
		public void AddOmexActivitySource_ActivityCreationEnabled()
		{
			Task task = new HostBuilder()
				.ConfigureServices(collection =>
				{
					collection.AddOmexActivitySource();
				})
				.Build()
				.RunAsync();

			Activity? activity = new ActivitySource("Source")
				.StartActivity(nameof(AddOmexActivitySource_HostedServicesRegiestered));

			NullableAssert.IsNotNull(activity, "Activity creation enabled after host started");
		}
	}
}
