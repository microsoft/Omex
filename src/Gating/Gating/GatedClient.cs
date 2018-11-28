/**************************************************************************************************
	GatedClient.cs

	Class describing a gated client.
**************************************************************************************************/

using System;
using System.Globalization;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Model.Types;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// A gated client.
	/// </summary>
	public class GatedClient
	{
		/// <summary>
		/// Name of the client
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// App code of the client
		/// </summary>
		public string AppCode { get; set; }


		/// <summary>
		/// Product code of the client
		/// </summary>
		public ProductCode ProductCode { get; set; }


		/// <summary>
		/// Version of the client
		/// </summary>
		public ProductVersion Version { get; set; }


		/// <summary>
		/// Merge two gated clients
		/// </summary>
		/// <param name="client1">first client</param>
		/// <param name="client2">second client</param>
		/// <returns>merged gated client</returns>
		/// <remarks>The name must be the same on both clients. The Version returned is the
		/// greatest version of the two clients. If one or no client has a product code, that
		/// code is used, if both have a product code, the code from the client with the greatest
		/// version is used.</remarks>
		public static GatedClient MergeClient(GatedClient client1, GatedClient client2)
		{
			if (client1 == null)
			{
				return client2;
			}

			if (client2 == null)
			{
				return client1;
			}

			if (!string.Equals(client1.Name, client2.Name, StringComparison.OrdinalIgnoreCase))
			{
				ULSLogging.LogTraceTag(0x238502a1 /* tag_97qk7 */, Categories.GateSelection, Levels.Error,
					"The name of the clients must be the same. Client1 '{0}', Client2 '{1}'.",
					client1.Name, client2.Name);
				return null;
			}

			GatedClient mergedClient = new GatedClient
			{
				Name = client1.Name
			};

			if (client1.Version == null)
			{
				mergedClient.Version = client2.Version;
				mergedClient.ProductCode = client2.ProductCode ?? client1.ProductCode;
				mergedClient.AppCode = client2.AppCode;
			}
			else if (client2.Version == null)
			{
				mergedClient.Version = client1.Version;
				mergedClient.ProductCode = client1.ProductCode ?? client2.ProductCode;
				mergedClient.AppCode = client1.AppCode;
			}
			else if (client1.Version > client2.Version)
			{
				mergedClient.Version = client1.Version;
				mergedClient.ProductCode = client1.ProductCode ?? client2.ProductCode;
				mergedClient.AppCode = client1.AppCode;
			}
			else
			{
				mergedClient.Version = client2.Version;
				mergedClient.ProductCode = client2.ProductCode ?? client1.ProductCode;
				mergedClient.AppCode = client2.AppCode;
			}

			return mergedClient;
		}


		/// <summary>
		/// Returns a string representation of the client.
		/// </summary>
		/// <returns>A string representation of the client.</returns>
		public override string ToString() => ToGatedClientString(ProductCode, Version);


		/// <summary>
		/// Returns a string representation of the client.
		/// </summary>
		/// <param name="code">The product code.</param>
		/// <param name="version">The product version.</param>
		/// <returns>A string representation of the client.</returns>
		public static string ToGatedClientString(ProductCode code, ProductVersion version) => string.Format(CultureInfo.InvariantCulture, "{0} | {1}", code, version);
	}
}
