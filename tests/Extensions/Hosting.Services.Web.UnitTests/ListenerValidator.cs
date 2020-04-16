// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Hosting.Services.Web;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hosting.Services.Web.UnitTests
{
	internal class ListenerValidator<TService, TContext>
		where TService : IServiceFabricService<TContext>
		where TContext : ServiceContext
	{
		public ListenerValidator()
		{
			m_logsEventSourceMock = new Mock<ILogEventSender>();
			ListenerName = "TestListener";
			IntegrationOptions = ServiceFabricIntegrationOptions.None;
			BuilderAction = builder =>
			{
				builder.ConfigureServices(collection =>
				{
					collection
						.AddOmexServiceFabricDependencies<TContext>()
						.AddTransient<TypeRegisteredInListenerExtension>()
						.AddSingleton(m_logsEventSourceMock.Object);
				});
			};
		}

		public string ListenerName { get; }

		public ServiceFabricIntegrationOptions IntegrationOptions { get; }

		public Action<IWebHostBuilder> BuilderAction { get; }

		public IWebHost ValidateListenerBuilder(TContext context, KestrelListenerBuilder<MockStartup, TService, TContext> builder)
		{
			Assert.AreEqual(ListenerName, builder.Name);

			IWebHost host = builder.BuildWebHost(
					string.Empty,
					new MockListener(context, (s, l) => new Mock<IWebHost>().Object));

			Assert.AreEqual(context, ResolveType<IAccessor<TContext>>(host).Value);

			bool isStatefulService = typeof(StatefulServiceContext).IsAssignableFrom(typeof(TContext));
			if (isStatefulService)
			{
				Assert.IsNotNull(ResolveType<IAccessor<IReliableStateManager>>(host).Value);
			}

			ResolveType<TypeRegisteredInListenerExtension>(host);
			ResolveType<MockStartup.TypeRegisteredInStartup>(host);

			return host;
		}

		public void ValidateOmexTypesRegistered(IWebHost host)
		{
			ResolveType<ITimedScopeProvider>(host);
			ILogger logger = ResolveType<ILogger<ListenerValidator<TService, TContext>>>(host);
			m_logsEventSourceMock.Invocations.Clear();
			logger.LogError("TestMessage");
			Assert.AreNotEqual(0, m_logsEventSourceMock.Invocations.Count, "Omex logger should be registred in WebHost");

			ResolveType<ActivityEnrichmentMiddleware>(host);
			ResolveType<ResponseHeadersMiddleware>(host);

#pragma warning disable CS0618 // Obsolete middlewares should also be resolvable
			ResolveType<ObsoleteCorrelationHeadersMiddleware>(host);
#pragma warning restore CS0618
		}

		public ICommunicationListener ValidateBuildFunction(TService service, KestrelListenerBuilder<MockStartup, TService, TContext> builder)
		{
			ICommunicationListener listener = builder.Build(service);
			Assert.IsNotNull(listener, "Listener should not be null");
			return listener;
		}

		private Mock<ILogEventSender> m_logsEventSourceMock;

		private T ResolveType<T>(IWebHost host) where T : class
		{
			T obj = host.Services.GetService<T>();
			Assert.IsNotNull(obj, "Failed to resolve {0}", typeof(T));
			return obj;
		}

		private class TypeRegisteredInListenerExtension { }

		private class MockListener : AspNetCoreCommunicationListener
		{
			public MockListener(ServiceContext serviceContext, Func<string, AspNetCoreCommunicationListener, IWebHost> build)
				: base(serviceContext, build) { }

			protected override string GetListenerUrl() => string.Empty;
		}
	}
}
