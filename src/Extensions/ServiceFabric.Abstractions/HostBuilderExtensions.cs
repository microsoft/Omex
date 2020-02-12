using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.Omex.Extensions.Compatability;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>Add required Omex dependencies</summary>
		public static IHostBuilder ConfigureOmexService(this IHostBuilder builder)
		{
			return builder.ConfigureServices(collection =>
			{
				collection
					.AddServiceFabricDependencies()
					.AddOmexLogging<OmexServiceFabricContext>()
					.AddTimedScopes()
					.AddOmexCompatability();
			});
		}
	}
}
