using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Omex.Gating.Extensions;
using Microsoft.Omex.Gating.Network;
using Microsoft.Omex.System.Caching;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Data.FileSystem;
using Microsoft.Omex.System.Data.Resources;

namespace Microsoft.Omex.Gating.Example
{
	internal class Program : IDisposable
	{
		private static void Main(string[] args)
		{

			using (Program program = new Program())
			{
				program.Run(args);
			}
		}

		private void Run(string[] args)
		{
			GateDataSetLoader = CreateGateDataSetLoader(false);
			Gates gates = new Gates(GateDataSetLoader);

			IGatedRequest gatedRequest = new SampleGatedRequest();
			IGateContext gateContext = new GateContext(gatedRequest, null, null);
			gateContext.PerformAction(new GatedAction(gates.GetGate("sample_allowed_gate"), SampleAction));

			Console.WriteLine("Loaded gates:");
			foreach (string gateName in gates.GateNames)
			{
				Console.WriteLine(gateName);
			}
		}


		private static GateDataSetLoader CreateGateDataSetLoader(bool fromFile)
		{
			if (fromFile)
			{
				// Attempt to load the 'OmexGates.xml' and 'OmexTip.xml' from the same location as the current assembly
				return new GateDataSetLoader(
					new LocalCache(),
					new ResourceMonitor(),
					new FileResource(new IOFile(), Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "OmexGates.xml"),
					new FileResource(new IOFile(), Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "OmexTip.xml"));
			}
			else
			{
				// Attempt to load the 'OmexGates.xml' and 'OmexTip.xml' as embedded resources from the current assembly
				return new GateDataSetLoader(
					new LocalCache(),
					new ResourceMonitor(),
					new EmbeddedResource(Assembly.GetExecutingAssembly(), "OmexGates.xml"),
					new EmbeddedResource(Assembly.GetExecutingAssembly(), "OmexTip.xml"));
			}
		}


		private static void SampleAction()
		{
			Console.WriteLine("SmapleAction has been called");
		}

		#region IDisposable Support
		private bool m_disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!m_disposedValue)
			{
				if (disposing)
				{
					GateDataSetLoader?.Dispose();
				}

				m_disposedValue = true;
			}
		}


		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion


		private GateDataSetLoader GateDataSetLoader { get; set; }
	}


	internal class SampleGatedRequest : IGatedRequest
	{
		public GatedClient CallingClient => throw new NotImplementedException();


		public string Market => throw new NotImplementedException();


		public IEnumerable<GatedUser> Users => throw new NotImplementedException();


		public HashSet<string> RequestedGateIds => new HashSet<string>() { "sample_allowed_gate" };


		public HashSet<string> BlockedGateIds => new HashSet<string>() { "sample_blocked_gate" };


		public string Environment => throw new NotImplementedException();


		public Tuple<UserAgentBrowser, int> GetUserAgentBrowser() => throw new NotImplementedException();


		public bool IsPartOfKnownIPRange(IKnownIPAddresses knownIpAddresses, string ipRange) => throw new NotImplementedException();


		public void UpdateExpectedClients(GatedClient[] clients) => throw new NotImplementedException();
	}
}
