// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Fabric;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Hosting.Services.Web;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hosting.Services.Web.UnitTests
{
	internal class ListenerValidator<TContext>
		where TContext : ServiceContext
	{
		public ListenerValidator()
		{
			m_logsEventSourcMock = new Mock<ILogEventSender>();
			ListenerName = "TestListener";
			IntegrationOptions = ServiceFabricIntegrationOptions.None;
			BuilderAction = builder =>
			{
				builder.ConfigureServices(collection =>
				{
					collection
						.AddOmexServiceFabricDependencies<TContext>()
						.AddTransient<TypeRegistredInListenerExtension>()
						.AddSingleton(m_logsEventSourcMock.Object);
				});
			};
		}


		public string ListenerName { get; }


		public ServiceFabricIntegrationOptions IntegrationOptions { get; }


		public Action<IWebHostBuilder> BuilderAction { get; }


		public IWebHost ValidateListenerBuilder(TContext context, KestrelListenerBuilder<MockStartup, TContext> builder)
		{
			Assert.AreEqual(ListenerName, builder.Name);

			IWebHost host = builder.BuildWebHost(
					context,
					string.Empty,
					new MockListener(context, (s, l) => new Mock<IWebHost>().Object));

			Assert.ReferenceEquals(context, ResolveType<ServiceContext>(host));
			Assert.ReferenceEquals(context, ResolveType<TContext>(host));
			Assert.ReferenceEquals(context, ResolveType<IServiceContextAccessor<TContext>>(host).ServiceContext);

			ResolveType<TypeRegistredInListenerExtension>(host);
			ResolveType<MockStartup.TypeRegistredInStartup>(host);

			return host;
		}


		public void ValidateOmexTypesRegistred(IWebHost host)
		{
			ResolveType<ITimedScopeProvider>(host);
			ILogger logger = ResolveType<ILogger<ListenerValidator<TContext>>>(host);
			m_logsEventSourcMock.Invocations.Clear();
			logger.LogError("TestMessage");
			Assert.AreNotEqual(0, m_logsEventSourcMock.Invocations.Count, "Omex logger should be registred in WebHost");
		}


		public ICommunicationListener ValidateBuildFunction(TContext context, KestrelListenerBuilder<MockStartup, TContext> builder)
		{
			ICommunicationListener listener = builder.Build(context);
			Assert.IsNotNull(listener, "Listener should not be null");
			return listener;
		}


		private Mock<ILogEventSender> m_logsEventSourcMock;


		private T ResolveType<T>(IWebHost host) where T : class
		{
			T obj = host.Services.GetService<T>();
			Assert.IsNotNull(obj, "Failed to resolve {0}", typeof(T));
			return obj;
		}


		private class TypeRegistredInListenerExtension { }


		private class MockListener : AspNetCoreCommunicationListener
		{
			public MockListener(ServiceContext serviceContext, Func<string, AspNetCoreCommunicationListener, IWebHost> build)
				: base(serviceContext, build) { }

			protected override string GetListenerUrl() => string.Empty;
		}
	}
}
