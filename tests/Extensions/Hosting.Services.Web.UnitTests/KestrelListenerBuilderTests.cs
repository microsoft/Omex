using System;
using System.Fabric;
using System.Security.Permissions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Hosting.Services;
using Microsoft.Omex.Extensions.Hosting.Services.Web;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;

namespace Hosting.Services.Web.UnitTests
{
	//[TestClass]
	//public class HostBuilderExtensionsTests
	//{
	//	[TestMethod]
	//	public void CheckTypeRegistrationForStateless() => CheckTypeRegistration(MockStatelessServiceContextFactory.Default);


	//	[TestMethod]
	//	public void CheckTypeRegistrationForStateful() => CheckTypeRegistration(MockStatefulServiceContextFactory.Default);


	//	private void CheckTypeRegistration<TContext>(TContext context)
	//		where TContext : ServiceContext
	//	{
	//		ListenerValidator validator = new ListenerValidator();

	//		IListenerBuilder<TContext> builder = new HostBuilder()
	//			.AddKestrelStatefulListener<ListenerValidator.Startup>(validator.ListenerName, validator.IntegrationOptions, validator.BuilderAction)
	//			.AddKestrelStatelessListener<ListenerValidator.Startup>(validator.ListenerName, validator.IntegrationOptions, validator.BuilderAction)
	//			.UseDefaultServiceProvider(options =>
	//			{
	//				options.ValidateOnBuild = true;
	//				options.ValidateScopes = true;
	//			})
	//			.Build()
	//			.Services.GetService<IListenerBuilder<TContext>>();

	//		KestrelListenerBuilder<TContext, ListenerValidator.Startup> kestrelBuilder
	//			= (KestrelListenerBuilder<TContext, ListenerValidator.Startup>)builder;

	//		IWebHost host = validator.ValidateListenerBuilder(context, kestrelBuilder);
	//		validator.ValidateOmexTypesRegistred(host);
	//		validator.ValidateBuildFunction(context, kestrelBuilder);
	//	}
	//}


	[TestClass]
	public class KestrelListenerBuilderTests
	{
		[TestMethod]
		public void CheckTypeRegistrationForStateless() => CheckTypeRegistration(MockStatelessServiceContextFactory.Default);


		[TestMethod]
		public void CheckTypeRegistrationForStateful() => CheckTypeRegistration(MockStatefulServiceContextFactory.Default);


		private void CheckTypeRegistration<TContext>(TContext context)
			where TContext : ServiceContext
		{
			ListenerValidator validator = new ListenerValidator();

			KestrelListenerBuilder<ListenerValidator.Startup, TContext> builder =
				new KestrelListenerBuilder<ListenerValidator.Startup, TContext>(
					validator.ListenerName,
					validator.IntegrationOptions,
					validator.BuilderAction);

			validator.ValidateListenerBuilder(context, builder);
			validator.ValidateBuildFunction(context, builder);
		}
	}

	internal class ListenerValidator
	{
		public ListenerValidator()
		{
			m_logsEventSourcMock = new Mock<ILogsEventSource>();
			ListenerName = "TestListener";
			IntegrationOptions = ServiceFabricIntegrationOptions.None;
			BuilderAction = builder =>
			{
				builder.ConfigureServices(collection =>
				{
					collection
						.AddTransient<TypeRegistredInListenerExtension>()
						.AddSingleton(m_logsEventSourcMock.Object);
				});
			};
		}


		public string ListenerName { get; }
		public ServiceFabricIntegrationOptions IntegrationOptions { get; }
		public Action<IWebHostBuilder> BuilderAction { get; }


		public IWebHost ValidateListenerBuilder<TContext>(TContext context, KestrelListenerBuilder<Startup, TContext> builder)
			where TContext : ServiceContext
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
			ResolveType<TypeRegistredInStartup>(host);

			return host;
		}


		public void ValidateOmexTypesRegistred(IWebHost host)
		{
			ResolveType<ITimedScopeProvider>(host);
			ILogger logger = ResolveType<ILogger>(host);
			m_logsEventSourcMock.Invocations.Clear();
			logger.LogError("TestMessage");
			Assert.AreEqual(1, m_logsEventSourcMock.Invocations.Count, "Omex logger should be registred in WebHost");
		}


		public ICommunicationListener ValidateBuildFunction<TContext>(TContext context, KestrelListenerBuilder<Startup, TContext> builder)
			where TContext : ServiceContext
		{
			ICommunicationListener listener = builder.Build(context);
			Assert.IsNotNull(listener, "Listener should not be null");
			return listener;
		}


		private Mock<ILogsEventSource> m_logsEventSourcMock;


		private T ResolveType<T>(IWebHost host) where T : class
		{
			T obj = host.Services.GetService<T>();
			Assert.IsNotNull(obj, "Failed to resolve {0}", typeof(T));
			return obj;
		}


		internal class Startup
		{
			public Startup() { }


			public void ConfigureServices(IServiceCollection services) => services.AddTransient<TypeRegistredInStartup>();


			public void Configure(IApplicationBuilder app, IHostEnvironment env)
			{
			}
		}


		private class TypeRegistredInStartup { }


		private class TypeRegistredInListenerExtension { }


		private class MockListener : AspNetCoreCommunicationListener
		{
			public MockListener(ServiceContext serviceContext, Func<string, AspNetCoreCommunicationListener, IWebHost> build)
				: base(serviceContext, build) { }

			protected override string GetListenerUrl() => string.Empty;
		}
	}
}
