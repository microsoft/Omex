using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>Creates comunication listener for SF service</summary>
	public interface IListenerBuilder<in T> where T : ServiceContext
	{
		/// <summary>
		/// Listener name
		/// </summary>
		string Name { get; }


		/// <summary>
		/// Creates comunication listener for SF service 
		/// </summary>
		ICommunicationListener Build(T context);
	}
}
