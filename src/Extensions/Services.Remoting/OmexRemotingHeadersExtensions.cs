// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace Microsoft.Omex.Extensions.Services.Remoting
{
	/// <summary>
	/// Helper class for adding remoting headers
	/// </summary>
	/// <remarks>
	/// Implemented similarly to Asp .Net Core http request from here:
	/// - reading headers https://github.com/dotnet/aspnetcore/blob/master/src/Hosting/Hosting/src/Internal/HostingApplicationDiagnostics.cs
	/// - writing headers https://github.com/dotnet/runtime/blob/master/src/libraries/System.Net.Http/src/System/Net/Http/DiagnosticsHandler.cs
	/// </remarks>
	public static class OmexRemotingHeadersExtensions
	{
		/// <summary>
		/// Header name for passing <see cref="Activity" /> id
		/// </summary>
		/// <remarks>
		/// W3C standard for trace context parent 'traceparent', so we've added omex to avoid conflicts if support would be added in future
		/// link: https://www.w3.org/TR/trace-context/
		/// </remarks>
		private const string TraceParentHeaderName = "omex-traceparent";

		/// <summary>
		/// Header name for passing <see cref="Activity" /> baggage
		/// </summary>
		/// <remarks>
		/// W3C standard for trace context parent 'tracestate', so we've added omex to avoid conflicts if support would be added in future
		/// link: https://www.w3.org/TR/trace-context/
		/// </remarks>
		private const string TraceStateHeaderName = "omex-tracestate";

		private static readonly Encoding s_encoding = Encoding.Unicode;

		/// <summary>
		/// Attach activity information to outgoing remoting request headers
		/// </summary>
		public static void AttachActivityToOutgoingRequest(this IServiceRemotingRequestMessage requestMessage, Activity? activity)
		{
			if (activity == null || string.IsNullOrWhiteSpace(activity.Id))
			{
				return;
			}

			IServiceRemotingRequestMessageHeader header = requestMessage.GetHeader();
			if (!header.TryGetHeaderValue(TraceParentHeaderName, out byte[] _)) // header update not supported
			{
				header.AddHeader(TraceParentHeaderName, s_encoding.GetBytes(activity.Id));
				header.AddHeader(TraceStateHeaderName, SerializeBaggage(activity.Baggage.ToArray()));
			}
		}

		/// <summary>
		/// Extract activity information from incoming remoting request headers
		/// </summary>
		public static void ExtractActivityFromIncomingRequest(this IServiceRemotingRequestMessage requestMessage, Activity? activity)
		{
			if (activity == null)
			{
				return;
			}

			IServiceRemotingRequestMessageHeader headers = requestMessage.GetHeader();

			if (headers.TryGetHeaderValue(TraceParentHeaderName, out byte[] idBytes))
			{
				activity.SetParentId(s_encoding.GetString(idBytes));

				if (headers.TryGetHeaderValue(TraceStateHeaderName, out byte[] baggageBytes))
				{
					KeyValuePair<string, string>[] baggage = DeserializeBaggage(baggageBytes);

					// AddBaggage adds items at the beginning of the list, so we need to add them in reverse to keep the same order as the client
					// An order could be important if baggage has two items with the same key (that is allowed by the contract)
					for (int i = baggage.Length - 1; i >= 0; i--)
					{
						KeyValuePair<string, string> pair = baggage[i];
						activity.AddBaggage(pair.Key, pair.Value);
					}
				}
			}
		}

		private static byte[] SerializeBaggage(KeyValuePair<string, string?>[] baggage)
		{
			using MemoryStream stream = new MemoryStream();
			s_serializer.WriteObject(stream, baggage);
			return stream.ToArray();
		}

		private static KeyValuePair<string, string>[] DeserializeBaggage(byte[] bytes)
		{
			using MemoryStream stream = new MemoryStream(bytes);
			return s_serializer.ReadObject(stream) as KeyValuePair<string, string>[]
				?? Array.Empty<KeyValuePair<string, string>>();
		}

		private static readonly DataContractSerializer s_serializer = new DataContractSerializer(typeof(KeyValuePair<string, string>[]));
	}
}
