// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Testing.Helpers;

namespace Microsoft.Omex.Extensions.Activities.UnitTests
{
	[TestClass]
	public class ServiceCollectionTests
	{
		[DataTestMethod]
		[DataRow(typeof(ActivitySource), typeof(ActivitySource))]
		[DataRow(typeof(IActivityStartObserver), typeof(ActivityObserver))]
		[DataRow(typeof(IActivityStopObserver), typeof(ActivityObserver))]
		public void AddOmexActivitySource_TypesRegistered(Type typeToResolve, Type implementingType)
		{
			object obj = CreateHost()
				.Services
				.GetRequiredService(typeToResolve);

			Assert.IsNotNull(obj);
			Assert.IsInstanceOfType(obj, implementingType);
		}

		[TestMethod]
		public void AddOmexActivitySource_HostedServicesRegiestered()
		{
			Type[] types = GetRegisteredServices<IHostedService>();

			Assert.AreEqual(2, types.Length);
			CollectionAssert.Contains(types, typeof(ActivityListenerInitializerService));
			CollectionAssert.Contains(types, typeof(DiagnosticsObserversInitializer));
		}

		[TestMethod]
		public void AddOmexActivitySource_ActivityCreationEnabled()
		{
			Task task = CreateHost().RunAsync();

			Activity? activity = new ActivitySource("Source")
				.StartActivity(nameof(AddOmexActivitySource_HostedServicesRegiestered));

			NullableAssert.IsNotNull(activity, "Activity creation enabled after host started");
		}

		private IHost CreateHost() =>
			new HostBuilder().ConfigureServices(collection =>
			{
				collection.AddOmexActivitySource();
			})
			.Build();

		private Type[] GetRegisteredServices<T>()
			where T : class =>
				CreateHost()
					.Services
					.GetRequiredService<IEnumerable<T>>()
					.Select(s => s.GetType())
					.ToArray();
	}
}
