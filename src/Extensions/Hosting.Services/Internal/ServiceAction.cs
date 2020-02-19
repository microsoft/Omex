using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal class ServiceAction
	{
		public ServiceAction(Func<StatelessService, CancellationToken, Task> action) =>
			m_action = action;


		public Task RunAsync(StatelessService service, CancellationToken cancellationToken) =>
			m_action(service, cancellationToken);


		private readonly Func<StatelessService, CancellationToken, Task> m_action;
	}
}
