// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// Adds user information if needed
	/// </summary>
	internal class UserIdentiyMiddleware : IMiddleware, IDisposable
	{
		private const int HashSize = 32; // sha256 hash size, from here https://github.com/dotnet/runtime/blob/26a71f95b708721065f974fd43ba82a1dcb3e8f0/src/libraries/System.Security.Cryptography.Algorithms/src/Internal/Cryptography/HashProviderDispenser.Windows.cs#L85
		private const int MaxIpByteSize = 16; // IPv6 size, from here https://github.com/dotnet/runtime/blob/26a71f95b708721065f974fd43ba82a1dcb3e8f0/src/libraries/Common/src/System/Net/IPAddressParserStatics.cs#L9
		private readonly HashAlgorithm m_hashAlgorithm;
		private readonly ISaltProvider m_saltProvider;

		public UserIdentiyMiddleware(ISaltProvider saltProvider)
		{
			m_saltProvider = saltProvider;
			m_hashAlgorithm = new SHA256Managed();
		}

		Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
		{
			Activity? activity = Activity.Current;

			if (activity != null && string.IsNullOrEmpty(activity.GetUserHash()))
			{
				string userHash = GetUserHash(context);
				if (string.IsNullOrWhiteSpace(userHash))
				{
					activity.SetUserHash(userHash);
				}
			}

			return next(context);
		}

		private string GetUserHash(HttpContext context)
		{
			IHttpConnectionFeature connection = context.Features.Get<IHttpConnectionFeature>();
			IPAddress? remoteIpAddress = connection.RemoteIpAddress;
			if (remoteIpAddress == null)
			{
				return string.Empty;
			}

			return CreateHash(remoteIpAddress);
		}

		public string CreateHash(IPAddress ip)
		{
			Span<byte> saltSpan = m_saltProvider.GetSalt();

			using IMemoryOwner<byte> uidMemoryOwner = MemoryPool<byte>.Shared.Rent(MaxIpByteSize + saltSpan.Length);
			Span<byte> uidSpan = uidMemoryOwner.Memory.Span;
			if (!ip.TryWriteBytes(uidSpan.Slice(0, MaxIpByteSize), out int ipBytesWritten))
			{
				return string.Empty;
			}

			saltSpan.CopyTo(uidSpan.Slice(ipBytesWritten));

			using IMemoryOwner<byte> hashMemoryOwner = MemoryPool<byte>.Shared.Rent(HashSize);
			Span<byte> hashSpan = hashMemoryOwner.Memory.Span;
			if (!m_hashAlgorithm.TryComputeHash(uidSpan, hashSpan, out int hashBytesWritten))
			{
				return string.Empty;
			}

			// TODO: Should be replaced by Convert.ToHexStringUpper after Net 5 release, https://github.com/dotnet/runtime/issues/17837#issuecomment-639018813
			return BitConverter.ToString(hashSpan.ToArray()).Replace("-", "");
		}

		public void Dispose()
		{
			m_hashAlgorithm.Dispose();
			m_saltProvider.Dispose();
		}
	}

	///// <summary>
	///// User identity configuration middleware
	///// </summary>
	//public class UserIdentityConfigurationMiddleware
	//{

	//	private string GetUserId(HttpContext context)
	//	{
	//		if (context.User?.Identity?.IsAuthenticated ?? false)
	//		{
	//			string userNameIdentifier = GetUserNameIdentifier(context);
	//			if (!string.IsNullOrEmpty(userNameIdentifier))
	//			{
	//				return userNameIdentifier;
	//			}

	//			string userSubject = GetUserSubject(context);
	//			if (!string.IsNullOrEmpty(userSubject))
	//			{
	//				return userSubject;
	//			}
	//		}

	//		// TODO (3894523): add constants when moving to OmexSharedSF
	//		if (context.Request.Cookies.TryGetValue("anonUID", out string anonUidCookie))
	//		{
	//			return anonUidCookie;
	//		}

	//		if (context.Request.Headers.TryGetValue("X-Office-Session", out StringValues officeSessionHeader) &&
	//			officeSessionHeader.Count > 0)
	//		{
	//			return officeSessionHeader.First();
	//		}

	//		if (context.Request.Query.TryGetValue("ClientSessionIdQueryParam", out StringValues clientSessionId) &&
	//			clientSessionId.Count > 0)
	//		{
	//			return clientSessionId.First();
	//		}

	//		global::System.Net.IPAddress remoteIpAddress = context.Connection.RemoteIpAddress;
	//		if (remoteIpAddress != null)
	//		{
	//			return remoteIpAddress.ToString();
	//		}

	//		return null;
	//	}


	//	private static string GetUserNameIdentifier(HttpContext context) => context.User?.Claims?.FirstOrDefault(
	//		c => string.Equals(c.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))?.Value;


	//	private static string GetUserSubject(HttpContext context) => context.User?.Claims?.FirstOrDefault(
	//		c => string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase))?.Value;
	//}
	}
