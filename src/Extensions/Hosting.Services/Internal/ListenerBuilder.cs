using System;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal class ListenerBuilder
	{
		public ListenerBuilder(
			string name,
			Func<StatelessServiceContext, ICommunicationListener> createListener) =>
			(Name, m_createListener) = (name, createListener);


		public string Name { get; }


		public ICommunicationListener Build(StatelessServiceContext context) =>
			m_createListener(context);


		private readonly Func<StatelessServiceContext, ICommunicationListener> m_createListener;
	}
}
