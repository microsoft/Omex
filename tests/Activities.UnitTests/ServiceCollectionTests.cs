// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.Testing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Activities.UnitTests
{
	[TestClass]
	public class ServiceCollectionTests
	{
		[TestMethod]
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
		public void AddOmexActivitySource_HostedServicesRegistered()
		{
			Type[] types = GetRegisteredServices<IHostedService>();

			CollectionAssert.Contains(types, typeof(ActivityListenerInitializerService));
			CollectionAssert.Contains(types, typeof(DiagnosticsObserversInitializer));
		}

		[TestMethod]
		public void AddOmexActivitySource_ActivityCreationEnabled()
		{
			Task task = CreateHost().RunAsync();

			using Activity? activity = new ActivitySource("Source")
				.StartActivity(nameof(AddOmexActivitySource_HostedServicesRegistered));

			NullableAssert.IsNotNull(activity, "Activity creation enabled after host started");
		}

		private static IHost CreateHost() =>
			new HostBuilder().ConfigureServices(collection =>
			{
				collection.AddOmexActivitySource();
			})
			.Build();

		private static Type[] GetRegisteredServices<T>()
			where T : class =>
				[.. CreateHost()
					.Services
					.GetRequiredService<IEnumerable<T>>()
					.Select(s => s.GetType())];
	}
}
