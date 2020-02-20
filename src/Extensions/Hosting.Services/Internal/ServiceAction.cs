using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal sealed class ServiceAction<TServiceContext> : IServiceAction<TServiceContext>
		where TServiceContext : ServiceContext
	{
		public ServiceAction(Func<TServiceContext, CancellationToken, Task> action) =>
			m_action = action;


		public Task RunAsync(TServiceContext service, CancellationToken cancellationToken) =>
			m_action(service, cancellationToken);


		private readonly Func<TServiceContext, CancellationToken, Task> m_action;
	}
}
