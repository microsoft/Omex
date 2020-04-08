// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace Microsoft.Omex.Extensions.Services.Remoting
{
	/// <summary>
	/// Helper class for adding remoting headers
	/// </summary>
	public static class OmexRemotingHeaders
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

		private static readonly BinaryFormatter s_binaryFormatter = new BinaryFormatter
		{
			Binder = new HeadersSerializationBinder()
		};

		/// <summary>
		/// Attach activity information to outgoing remoting request headers
		/// </summary>
		public static void AttachActivityToOuthgoingRequest(Activity? activity, IServiceRemotingRequestMessage requestMessage)
		{
			if (activity == null)
			{
				return;
			}

			IServiceRemotingRequestMessageHeader header = requestMessage.GetHeader();
			header.AddHeader(TraceParentHeaderName, s_encoding.GetBytes(activity.Id));
			header.AddHeader(TraceStateHeaderName, SerializeBaggage(activity.Baggage.ToArray()));
		}

		/// <summary>
		/// Extract activity information from incoming remoting request headers
		/// </summary>
		public static void ExtractActivityFromIncominRequest(Activity? activity, IServiceRemotingRequestMessage requestMessage)
		{
			if (activity == null)
			{
				return;
			}

			IServiceRemotingRequestMessageHeader headers = requestMessage.GetHeader();

			if (headers.TryGetHeaderValue(TraceParentHeaderName, out byte[] idBytes))
			{
				activity.SetParentId(s_encoding.GetString(idBytes));
			}

			if (headers.TryGetHeaderValue(TraceStateHeaderName, out byte[] baggageBytes))
			{
				foreach (KeyValuePair<string, string> pair in DeserializeBaggage(baggageBytes))
				{
					activity.AddBaggage(pair.Key, pair.Value);
				}
			}
		}

		private static byte[] SerializeBaggage(KeyValuePair<string, string>[] baggage)
		{
			using MemoryStream stream = new MemoryStream();
			s_binaryFormatter.Serialize(stream, baggage);
			return stream.ToArray();
		}

		private static KeyValuePair<string, string>[] DeserializeBaggage(byte[] bytes)
		{
			using MemoryStream stream = new MemoryStream(bytes);
			return (KeyValuePair<string, string>[])s_binaryFormatter.Deserialize(stream);
		}

		private class HeadersSerializationBinder : SerializationBinder
		{
			public override Type BindToType(string assemblyName, string typeName)
			{
				Type? BindToTypeOrDefault<TTypeToMatch>()
				{
					Type type = typeof(TTypeToMatch);
					return string.Equals(typeName, type.FullName, StringComparison.InvariantCulture)
						&& string.Equals(assemblyName, type.Assembly.FullName, StringComparison.InvariantCulture)
							? type
							: null;
				}

				return BindToTypeOrDefault<KeyValuePair<string, string>[]>()
					?? BindToTypeOrDefault<KeyValuePair<string, string>>()
					?? BindToTypeOrDefault<string>()
					?? throw new ArgumentException(
						$"Unexpected deserialization type. TypeName:'{typeName}', AssemplyName:'{assemblyName}'.",
						nameof(typeName));
			}
		}
	}
}
