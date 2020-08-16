// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.EventSources;
using Microsoft.Omex.Extensions.Hosting.Certificates;
using Microsoft.Omex.Extensions.Hosting.EventSources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Hosting.UnitTests
{
	[TestClass]
	public class HostBuilderExtensionsTests
	{
		[DataTestMethod]
		[DataRow(typeof(ILogger<HostBuilderExtensionsTests>))]
		[DataRow(typeof(ITimedScopeProvider))]
		public void AddOmexServices_TypesRegistered(Type type)
		{
			object collectionObj = new ServiceCollection()
				.AddSingleton<IHostEnvironment>(new HostingEnvironment())
				.AddOmexServices()
				.BuildServiceProvider(new ServiceProviderOptions
				{
					ValidateOnBuild = true,
					ValidateScopes = true
				}).GetService(type);

			Assert.IsNotNull(collectionObj, FormattableString.Invariant($"Type {type} was not resolved after AddOmexServices to ServiceCollection"));

			object hostObj = new HostBuilder()
				.AddOmexServices()
				.UseDefaultServiceProvider(options =>
				{
					options.ValidateOnBuild = true;
					options.ValidateScopes = true;
				})
				.Build().Services.GetService(type);

			Assert.IsNotNull(hostObj, FormattableString.Invariant($"Type {type} was not resolved after AddOmexServices to HostBuilder"));
		}

		[TestMethod]
		public void AddCertificateReader_TypesRegistered()
		{
			ICertificateReader collectionObj = new ServiceCollection()
				.AddCertificateReader()
				.AddLogging()
				.BuildServiceProvider(new ServiceProviderOptions
				{
					ValidateOnBuild = true,
					ValidateScopes = true
				})
				.GetService<ICertificateReader>();

			Assert.IsNotNull(collectionObj, FormattableString.Invariant($"Type {nameof(ICertificateReader)} was not resolved after AddCertificateReader to ServiceCollection"));
		}

		[TestMethod]
		public void BuildWithErrorReporting_LogsExceptions()
		{
			string serviceType = ServiceInitializationEventSource.GetHostName();
			CustomEventListener listener = new CustomEventListener();
			listener.EnableEvents(ServiceInitializationEventSource.Instance, EventLevel.Error);

			Assert.ThrowsException<AggregateException>(() => new HostBuilder()
				.ConfigureServices(c => c.AddTransient<TypeThatShouldNotBeResolvable>())
				.BuildWithErrorReporting(),
				"BuildWithErrorReporting should fail in case of unresolvable dependencies");

			bool hasErrorEvent = listener.EventsInformation.Any(e =>
				serviceType == GetPayloadValue<string>(e, ServiceTypePayloadName)
				&& e.EventId == (int)EventSourcesEventIds.GenericHostFailed);

			Assert.IsTrue(hasErrorEvent, "BuildWithErrorReporting error should be logged");
		}

		public class TypeThatShouldNotBeResolvable
		{
			public TypeThatShouldNotBeResolvable(TypeThatIsNotRegistered value) { }

			public class TypeThatIsNotRegistered { }
		}

		private class CustomEventListener : EventListener
		{
			public List<EventWrittenEventArgs> EventsInformation { get; } = new List<EventWrittenEventArgs>();

			protected override void OnEventSourceCreated(EventSource eventSource)
			{
				base.OnEventSourceCreated(eventSource);
			}

			protected override void OnEventWritten(EventWrittenEventArgs eventData) => EventsInformation.Add(eventData);
		}

		private static TPayloadType? GetPayloadValue<TPayloadType>(EventWrittenEventArgs info, string name)
			where TPayloadType : class
		{
			int index = info.PayloadNames?.IndexOf(name) ?? -1;
			return (TPayloadType?)(index < 0 ? null : info.Payload?[index]);
		}

		private const string ServiceTypePayloadName = "serviceType";
	}
}
