// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.UnitTests.Composables;

/// <summary>
/// A collection of helpers to test the health checks.
/// </summary>
public static class HealthCheckTestHelpers
{
	internal const HealthStatus RegisteredUnhealthyHealthStatus = HealthStatus.Unhealthy;
	internal const string HealthCheckMockRegistrationName = "some-name";

	/// <summary>
	/// Returns an instance of the health check context.
	/// </summary>
	/// <param name="instance">The health check instance.</param>
	/// <param name="registeredFailedHealthStatus">
	/// The health status that the registration should hint in case the health check fails.
	/// </param>
	/// <returns>The health check context instance.</returns>
	public static HealthCheckContext GetHealthCheckContext(
		IHealthCheck instance,
		HealthStatus? registeredFailedHealthStatus = null) =>
		new()
		{
			Registration = new(
				HealthCheckMockRegistrationName,
				instance,
				registeredFailedHealthStatus ?? RegisteredUnhealthyHealthStatus,
				Array.Empty<string>())
		};

	/// <summary>
	/// Gets a new Logger instance that registers the logged messages.
	/// </summary>
	/// <returns>The logger messages.</returns>
	internal static InMemoryLogger GetLoggerInstance() => new();

	/// <summary>
	/// Gets a new Logger instance with a generic parameter that registers the logged messages.
	/// </summary>
	/// <returns>The logger messages.</returns>
	internal static InMemoryLogger<TInstance> GetLoggerInstance<TInstance>() => new();

	public class InMemoryLogger<TInstance> : InMemoryLogger, ILogger<TInstance>
	{
		protected override ILogger InternalLogger => NullLogger<TInstance>.Instance;
	}

	public class InMemoryLogger : ILogger
	{
		protected virtual ILogger InternalLogger => NullLogger.Instance;

		private static readonly IList<string> s_list = new List<string>();

		public IDisposable? BeginScope<TState>(TState state)
			where TState : notnull =>
				InternalLogger.BeginScope(state);

		public bool IsEnabled(LogLevel logLevel) =>
			InternalLogger.IsEnabled(logLevel);

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			InternalLogger.Log(logLevel, eventId, state, exception, formatter);
			s_list.Add(formatter(state, exception));
		}

		public static IEnumerable<string> GetLogMessages() => s_list;
	}

	/// <summary>
	/// Creates a mock of the HttpClientFactory that returns a HttpClient that returns the given message.
	/// </summary>
	/// <param name="message">The response message to return.</param>
	/// <param name="numberOfFailuresBeforeOk">The number of failures before returning the response.</param>
	/// <param name="shouldThrowException">Whether the HttpClient should throw an exception.</param>
	/// <returns>The Http Client factory mock.</returns>
	public static Mock<IHttpClientFactory> GetHttpClientFactoryMock(
		HttpResponseMessage message,
		int? numberOfFailuresBeforeOk = null,
		bool? shouldThrowException = false)
	{
		HttpClientHandler messageHandler =
			(shouldThrowException, numberOfFailuresBeforeOk) switch
			{
				(true, _) => new MockedHttpExceptionMessageHandler(message),
				(_, null) => new MockedHttpMessageHandler(message),
				(_, int n) => new MockedRepeatedErrorsHttpMessageHandler(message, n)
			};
		messageHandler.CheckCertificateRevocationList = true;

		HttpClient httpClientMock = new(messageHandler);

		Mock<IHttpClientFactory> mock = new();
		mock.Setup(f => f.CreateClient(It.IsAny<string>()))
			.Returns(httpClientMock);

		return mock;
	}

	/// <summary>
	/// Returns a new HttpRequestMessage instance for mocking HTTP calls.
	/// </summary>
	/// <param name="uri">The URI.</param>
	/// <returns>The Request message</returns>
	internal static HttpRequestMessage GetHttpRequestMessageMock(string? uri = null) =>
		new(HttpMethod.Get, uri ?? "https://something.com/");

	/// <summary>
	/// Creates a new response message.
	/// </summary>
	/// <param name="statusCode">The response status code.</param>
	/// <param name="message">The response raw body string.</param>
	/// <returns>The response.</returns>
	public static HttpResponseMessage GetHttpResponseMessageMock(HttpStatusCode statusCode, string message) =>
		new(statusCode)
		{
			Content = new StringContent(message)
		};

	public const int SfServiceMockLocalPort = 80;
	public const string SfServiceMockLocalHost = "localhost";


	/// <summary>
	/// Creates the local SF service info mock.
	/// </summary>
	public static void SetLocalServiceInfo()
	{
		const string PublishAddressEvnVariableName = "Fabric_NodeIPOrFQDN";
		const string EndpointPortEvnVariableSuffix = "Fabric_Endpoint_";

		Environment.SetEnvironmentVariable($"{EndpointPortEvnVariableSuffix}ServiceHttpEndpoint", "80", EnvironmentVariableTarget.Process);
		Environment.SetEnvironmentVariable(PublishAddressEvnVariableName, "localhost", EnvironmentVariableTarget.Process);
	}
}

internal class MockedHttpMessageHandler : HttpClientHandler
{
	private readonly HttpResponseMessage m_response;

	public MockedHttpMessageHandler(HttpResponseMessage response)
	{
		m_response = response;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
		Task.FromResult(m_response);
}

/// <summary>
/// Returns 500 for a given number of times before returning the given response.
/// The pattern is recursive, so after returning an OK response, it returns an originally given number of 500 responses.
/// </summary>
internal class MockedRepeatedErrorsHttpMessageHandler : HttpClientHandler
{
	private readonly HttpResponseMessage m_response;
	private readonly int m_failureTimes;
	private int m_failureCount;

	public MockedRepeatedErrorsHttpMessageHandler(HttpResponseMessage response, int failureTime)
	{
		m_response = response;
		m_failureTimes = failureTime;
		m_failureCount = failureTime;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (m_failureCount > 0)
		{
			m_failureCount--;
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
		}

		m_failureCount = m_failureTimes;
		return Task.FromResult(m_response);
	}
}

internal class MockedHttpExceptionMessageHandler : HttpClientHandler
{
	private readonly HttpResponseMessage m_response;

	public MockedHttpExceptionMessageHandler(HttpResponseMessage response)
	{
		m_response = response;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
		// If the status code is OK, the client won't throw an exception.
		m_response.StatusCode == HttpStatusCode.OK
			? Task.FromResult(m_response)
#if NET5_0_OR_GREATER
			: throw new HttpRequestException("An exception was raised", null, m_response.StatusCode);
#else
			: throw new HttpRequestException("An exception was raised", null);
#endif
}
