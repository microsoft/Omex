using System.Fabric;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	internal interface IStatelessServiceContextAccessor
	{
		StatelessServiceContext? ServiceContext { get; }
	}
}
