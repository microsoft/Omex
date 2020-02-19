using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal interface IOmexServiceRunner
	{
		Task RunServiceAsync(CancellationToken cancellationToken);
	}
}
