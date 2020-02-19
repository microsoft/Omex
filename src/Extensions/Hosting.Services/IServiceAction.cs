using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Action that will be executed in RunAsync methond of SF service
	/// </summary>
	public interface IServiceAction<in TServiceContext> where TServiceContext : ServiceContext
	{
		/// <summary>
		/// Action that will be executed in RunAsync methond of SF service
		/// </summary>
		Task RunAsync(TServiceContext service, CancellationToken cancellationToken);
	}
}
