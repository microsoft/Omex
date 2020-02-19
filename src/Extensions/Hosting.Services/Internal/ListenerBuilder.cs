using System;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal class ListenerBuilder<TServiceContext> : IListenerBuilder<TServiceContext>
		 where TServiceContext : ServiceContext
	{
		public ListenerBuilder(
			string name,
			Func<TServiceContext, ICommunicationListener> createListener) =>
			(Name, m_createListener) = (name, createListener);


		public string Name { get; }


		public ICommunicationListener Build(TServiceContext context) =>
			m_createListener(context);


		private readonly Func<TServiceContext, ICommunicationListener> m_createListener;
	}
}
