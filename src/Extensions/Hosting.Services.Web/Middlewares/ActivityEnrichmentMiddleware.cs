// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	/// <summary>
	/// Enrich request activity with Result, SubType and Metadata
	/// </summary>
	internal class ActivityEnrichmentMiddleware : IMiddleware
	{
		async Task IMiddleware.InvokeAsync(HttpContext context, RequestDelegate next)
		{
			await next(context).ConfigureAwait(false);

			Activity? activity = Activity.Current;

			if (activity == null)
			{
				return;
			}

			int statusCode = context.Response.StatusCode;

			activity.SetResult(GetResult(statusCode))
				.SetSubType($"{context.Request.Scheme}:{context.Connection.LocalPort}")
				.SetMetadata(statusCode.ToString());
		}

		private TimedScopeResult GetResult(int statusCode) =>
			statusCode >= 100 && statusCode < 400
				? TimedScopeResult.Success
				: statusCode >= 400 && statusCode < 500
					? TimedScopeResult.ExpectedError
					: TimedScopeResult.SystemError;
	}

	/// <summary>
	/// Adds user information if needed
	/// </summary>
	internal class UserIdentiyMiddleware : IMiddleware
	{
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

			string userIdentifier = remoteIpAddress.ToString();


		}

		private static string GetStringSha256Hash(string text)
		{
			if (!Code.ValidateNotNullOrWhiteSpaceArgument(text, nameof(text), TaggingUtilities.ReserveTag(0x237464a2 /* tag_93gs8 */)))
			{
				return string.Empty;
			}

			using SHA256Managed sha = new SHA256Managed();
			byte[] textData = Encoding.UTF8.GetBytes(text);
			byte[] hash = sha.ComputeHash(textData);
			return BitConverter.ToString(hash).Replace("-", string.Empty);
		}

		private static byte[] GetSalt()
		{
			if ((DateTime.Now - s_saltGenerationTime).TotalHours < 40)
			{
				using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
				{
					random.GetNonZeroBytes(s_salt);
					s_saltGenerationTime = DateTime.Now;
				}
			}

			return s_salt;
		}

		private static byte[] s_salt = new byte[180];
		private static DateTime s_saltGenerationTime = DateTime.Now;
	}

	/// <summary>
	/// User identity configuration middleware
	/// </summary>
	public class UserIdentityConfigurationMiddleware
	{

		public Task Invoke(HttpContext context)
		{
			try
			{
				string userId = GetUserId(context);
				if (!string.IsNullOrEmpty(userId))
				{
					string userHash = GetStringSha256Hash(userId);

					// TODO (3894523): remove when moving to OmexSharedSF
					ULSLogging.LogTraceTag(0x237464a0 /* tag_93gs6 */, SharedUlsCategories.OmexInfrastructure, Levels.Info,
						$"Setting user hash: '{userHash}'.");

					Office.Web.OfficeMarketplace.Shared.CrossCuttingConcerns.Correlation.CurrentCorrelation.UserHash = userHash;
				}
			}
			catch (Exception ex)
			{
				ULSLogging.ReportExceptionTag(0x237464a1 /* tag_93gs7 */, SharedUlsCategories.OmexInfrastructure, ex,
					"An exception occurred while trying to determine and set user hash on current correlation.");
			}

			return m_next(context);
		}


		private string GetUserId(HttpContext context)
		{
			if (context.User?.Identity?.IsAuthenticated ?? false)
			{
				string userNameIdentifier = GetUserNameIdentifier(context);
				if (!string.IsNullOrEmpty(userNameIdentifier))
				{
					return userNameIdentifier;
				}

				string userSubject = GetUserSubject(context);
				if (!string.IsNullOrEmpty(userSubject))
				{
					return userSubject;
				}
			}

			// TODO (3894523): add constants when moving to OmexSharedSF
			if (context.Request.Cookies.TryGetValue("anonUID", out string anonUidCookie))
			{
				return anonUidCookie;
			}

			if (context.Request.Headers.TryGetValue("X-Office-Session", out StringValues officeSessionHeader) &&
				officeSessionHeader.Count > 0)
			{
				return officeSessionHeader.First();
			}

			if (context.Request.Query.TryGetValue("ClientSessionIdQueryParam", out StringValues clientSessionId) &&
				clientSessionId.Count > 0)
			{
				return clientSessionId.First();
			}

			global::System.Net.IPAddress remoteIpAddress = context.Connection.RemoteIpAddress;
			if (remoteIpAddress != null)
			{
				return remoteIpAddress.ToString();
			}

			return null;
		}


		private static string GetUserNameIdentifier(HttpContext context) => context.User?.Claims?.FirstOrDefault(
			c => string.Equals(c.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))?.Value;


		private static string GetUserSubject(HttpContext context) => context.User?.Claims?.FirstOrDefault(
			c => string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase))?.Value;
	}
}
