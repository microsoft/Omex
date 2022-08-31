﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable IDE1006 // ASP.NET Core codebase naming convention
#pragma warning disable IDE0008 // Use explicit type
#pragma warning disable CS8795 // Partial method must have an implementation part because it has accessibility modifiers

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Omex.Preview.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Health Check Publisher Service is responsible for filtering the provided HealthCheck Registrations based on the function predicate,
/// and running the associated healthchecks with their own individual options (delay, period, timeout)
/// </summary>
internal sealed partial class OmexHealthCheckPublisherHostedService : IHostedService
{
	private readonly HealthCheckService _healthCheckService;
	private readonly IOptions<HealthCheckServiceOptions> _healthCheckServiceOptions;
	private readonly IOptions<HealthCheckPublisherOptions> _healthCheckPublisherOptions;
	private readonly IOptions<HealthCheckRegistrationParametersOptions> _healthCheckRegistrationOptions;
	private readonly ILogger _logger;
	private readonly IHealthCheckPublisher[] _publishers;
	private readonly HealthCheckRegistrationParameters _defaultHealthCheckOptions;
	private readonly Dictionary<HealthCheckRegistrationParameters, List<HealthCheckRegistration>> _healthChecksByOptions;
	private Dictionary<HealthCheckRegistrationParameters, Timer>? _timersByOptions;

	private readonly CancellationTokenSource _stopping;
	private CancellationTokenSource? _runTokenSource;

	public OmexHealthCheckPublisherHostedService(
		HealthCheckService healthCheckService,
		IOptions<HealthCheckServiceOptions> healthCheckServiceOptions,
		IOptions<HealthCheckPublisherOptions> healthCheckPublisherOptions,
		IOptions<HealthCheckRegistrationParametersOptions> healthCheckRegistrationOptions,
		ILogger<OmexHealthCheckPublisherHostedService> logger,
		IEnumerable<IHealthCheckPublisher> publishers)
	{
		if (healthCheckService == null)
		{
			throw new ArgumentNullException(nameof(healthCheckService));
		}

		if (healthCheckServiceOptions == null)
		{
			throw new ArgumentNullException(nameof(healthCheckServiceOptions));
		}

		if (healthCheckPublisherOptions == null)
		{
			throw new ArgumentNullException(nameof(healthCheckPublisherOptions));
		}

		if (healthCheckRegistrationOptions == null)
		{
			throw new ArgumentNullException(nameof(healthCheckRegistrationOptions));
		}

		if (logger == null)
		{
			throw new ArgumentNullException(nameof(logger));
		}

		if (publishers == null)
		{
			throw new ArgumentNullException(nameof(publishers));
		}

		_healthCheckService = healthCheckService;
		_healthCheckServiceOptions = healthCheckServiceOptions;
		_healthCheckPublisherOptions = healthCheckPublisherOptions;
		_healthCheckRegistrationOptions = healthCheckRegistrationOptions;
		_logger = logger;
		_publishers = publishers.ToArray();

		_stopping = new CancellationTokenSource();

		// We're specifically going out of our way to do this at startup time. We want to make sure you
		// get any kind of health-check related error as early as possible. Waiting until someone
		// actually tries to **run** health checks would be real baaaaad.
		ValidateRegistrations(_healthCheckServiceOptions.Value.Registrations);

		_defaultHealthCheckOptions = new HealthCheckRegistrationParameters(_healthCheckPublisherOptions.Value.Delay, _healthCheckPublisherOptions.Value.Period);
		// Group healthcheck registrations by delay, period and timeout, to build a Dictionary<(TimeSpan, TimeSpan, TimeSpan), List<HealthCheckRegistration>>
		// For HCs with no Delay or Period, we default to the publisher values
		_healthChecksByOptions = _healthCheckServiceOptions.Value.Registrations.GroupBy(r => GetHealthCheckRegistrationParametersOrDefault(r.Name) ?? _defaultHealthCheckOptions).ToDictionary(g => g.Key, g => g.ToList());
	}

	private HealthCheckRegistrationParameters? GetHealthCheckRegistrationParametersOrDefault(string registrationName)
	{
		_healthCheckRegistrationOptions.Value.RegistrationParameters.TryGetValue(registrationName, out var registrationParameters);

		return registrationParameters;
	}

	internal bool IsStopping => _stopping.IsCancellationRequested;

	internal bool IsTimerRunning => _timersByOptions != null;

	public Task StartAsync(CancellationToken cancellationToken = default)
	{
		if (_publishers.Length == 0)
		{
			return Task.CompletedTask;
		}

		// IMPORTANT - make sure this is the last thing that happens in this method. The timers can
		// fire before other code runs.
		_timersByOptions = CreateTimers(_healthChecksByOptions);

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			_stopping.Cancel();
		}
		catch
		{
			// Ignore exceptions thrown as a result of a cancellation.
		}

		if (_publishers.Length == 0)
		{
			return Task.CompletedTask;
		}

		if (_timersByOptions != null)
		{
			foreach (var timer in _timersByOptions.Values)
			{
				timer.Dispose();
			}

			_timersByOptions = null;
		}

		return Task.CompletedTask;
	}

	private Dictionary<HealthCheckRegistrationParameters, Timer> CreateTimers(IReadOnlyDictionary<HealthCheckRegistrationParameters, List<HealthCheckRegistration>> periodHealthChecksMap)
	{
		return periodHealthChecksMap.Select(m => CreateTimer(m.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
	}

	private KeyValuePair<HealthCheckRegistrationParameters, Timer> CreateTimer(HealthCheckRegistrationParameters healthCheckOptions)
	{
		return new KeyValuePair<HealthCheckRegistrationParameters, Timer>(
			healthCheckOptions,
			NonCapturingTimer.Create(
			async (state) =>
			{
				await RunAsync(healthCheckOptions: healthCheckOptions).ConfigureAwait(false);
			},
			null,
			dueTime: healthCheckOptions.Delay ?? _healthCheckPublisherOptions.Value.Delay, // Default to publisher Delay
			period: healthCheckOptions.Period ?? _healthCheckPublisherOptions.Value.Period) // Default to publisher Period
		);
	}

	// Internal for testing
	internal void CancelToken()
	{
		_runTokenSource!.Cancel();
	}

	// Internal for testing
	internal async Task RunAsync(HealthCheckRegistrationParameters? healthCheckOptions = default)
	{
		if (healthCheckOptions == default)
		{
			healthCheckOptions = _defaultHealthCheckOptions;
		}

		var duration = ValueStopwatch.StartNew();
		Logger.HealthCheckPublisherProcessingBegin(_logger);

		CancellationTokenSource? cancellation = null;
		try
		{
			var timeout = healthCheckOptions.Timeout ?? _healthCheckPublisherOptions.Value.Timeout; // Default to publisher timeout if null

			cancellation = CancellationTokenSource.CreateLinkedTokenSource(_stopping.Token);
			_runTokenSource = cancellation;
			cancellation.CancelAfter(timeout);

			await RunAsyncCore(healthCheckOptions, cancellation.Token).ConfigureAwait(false);

			Logger.HealthCheckPublisherProcessingEnd(_logger, duration.GetElapsedTime());
		}
		catch (OperationCanceledException) when (IsStopping)
		{
			// This is a cancellation - if the app is shutting down we want to ignore it. Otherwise, it's
			// a timeout and we want to log it.
		}
		catch (Exception ex)
		{
			// This is an error, publishing failed.
			Logger.HealthCheckPublisherProcessingEnd(_logger, duration.GetElapsedTime(), ex);
		}
		finally
		{
			cancellation?.Dispose();
		}
	}

	private async Task RunAsyncCore(HealthCheckRegistrationParameters healthCheckOptions, CancellationToken cancellationToken)
	{
		// Forcibly yield - we want to unblock the timer thread.
		await Task.Yield();

		// Concatenate predicates - we only run HCs with the set delay and period
		var withOptionsPredicate = (HealthCheckRegistration r) =>
		{
			var options = GetHealthCheckRegistrationParametersOrDefault(r.Name);
			var hasOptions = (options == default && healthCheckOptions == _defaultHealthCheckOptions) ||
							  options == healthCheckOptions;
			if (_healthCheckPublisherOptions?.Value.Predicate == null)
			{
				return hasOptions;
			}

			return hasOptions && _healthCheckPublisherOptions.Value.Predicate(r);
		};

		// The health checks service does it's own logging, and doesn't throw exceptions.
		var report = await _healthCheckService.CheckHealthAsync(withOptionsPredicate, cancellationToken).ConfigureAwait(false);

		var publishers = _publishers;
		var tasks = new Task[publishers.Length];
		for (var i = 0; i < publishers.Length; i++)
		{
			tasks[i] = RunPublisherAsync(publishers[i], report, cancellationToken);
		}

		await Task.WhenAll(tasks).ConfigureAwait(false);
	}

	private async Task RunPublisherAsync(IHealthCheckPublisher publisher, HealthReport report, CancellationToken cancellationToken)
	{
		var duration = ValueStopwatch.StartNew();

		try
		{
			Logger.HealthCheckPublisherBegin(_logger, publisher);

			await publisher.PublishAsync(report, cancellationToken).ConfigureAwait(false);
			Logger.HealthCheckPublisherEnd(_logger, publisher, duration.GetElapsedTime());
		}
		catch (OperationCanceledException) when (IsStopping)
		{
			// This is a cancellation - if the app is shutting down we want to ignore it. Otherwise, it's
			// a timeout and we want to log it.
		}
		catch (OperationCanceledException)
		{
			Logger.HealthCheckPublisherTimeout(_logger, publisher, duration.GetElapsedTime());
			throw;
		}
		catch (Exception ex)
		{
			Logger.HealthCheckPublisherError(_logger, publisher, duration.GetElapsedTime(), ex);
			throw;
		}
	}

	private static void ValidateRegistrations(IEnumerable<HealthCheckRegistration> registrations)
	{
		// Scan the list for duplicate names to provide a better error if there are duplicates.

		StringBuilder? builder = null;
		var distinctRegistrations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var registration in registrations)
		{
			if (!distinctRegistrations.Add(registration.Name))
			{
				builder ??= new StringBuilder("Duplicate health checks were registered with the name(s): ");

				builder.Append(registration.Name).Append(", ");
			}
		}

		if (builder is not null)
		{
			throw new ArgumentException(builder.ToString(0, builder.Length - 2), nameof(registrations));
		}
	}

	internal static class EventIds
	{
		public const int HealthCheckPublisherProcessingBeginId = 100;
		public const int HealthCheckPublisherProcessingEndId = 101;
		public const int HealthCheckPublisherBeginId = 102;
		public const int HealthCheckPublisherEndId = 103;
		public const int HealthCheckPublisherErrorId = 104;
		public const int HealthCheckPublisherTimeoutId = 104;

		// Hard code the event names to avoid breaking changes. Even if the methods are renamed, these hard-coded names shouldn't change.
		public const string HealthCheckPublisherProcessingBeginName = "HealthCheckPublisherProcessingBegin";
		public const string HealthCheckPublisherProcessingEndName = "HealthCheckPublisherProcessingEnd";
		public const string HealthCheckPublisherBeginName = "HealthCheckPublisherBegin";
		public const string HealthCheckPublisherEndName = "HealthCheckPublisherEnd";
		public const string HealthCheckPublisherErrorName = "HealthCheckPublisherError";
		public const string HealthCheckPublisherTimeoutName = "HealthCheckPublisherTimeout";
	}

	private static partial class Logger
	{
		[LoggerMessage(EventIds.HealthCheckPublisherProcessingBeginId, LogLevel.Debug, "Running health check publishers", EventName = EventIds.HealthCheckPublisherProcessingBeginName)]
		public static partial void HealthCheckPublisherProcessingBegin(ILogger logger);

		public static void HealthCheckPublisherProcessingEnd(ILogger logger, TimeSpan duration, Exception? exception = null) =>
			HealthCheckPublisherProcessingEnd(logger, duration.TotalMilliseconds, exception);

		[LoggerMessage(EventIds.HealthCheckPublisherProcessingEndId, LogLevel.Debug, "Health check publisher processing completed after {ElapsedMilliseconds}ms", EventName = EventIds.HealthCheckPublisherProcessingEndName)]
		private static partial void HealthCheckPublisherProcessingEnd(ILogger logger, double ElapsedMilliseconds, Exception? exception = null);

		[LoggerMessage(EventIds.HealthCheckPublisherBeginId, LogLevel.Debug, "Running health check publisher '{HealthCheckPublisher}'", EventName = EventIds.HealthCheckPublisherBeginName)]
		public static partial void HealthCheckPublisherBegin(ILogger logger, IHealthCheckPublisher HealthCheckPublisher);

		public static void HealthCheckPublisherEnd(ILogger logger, IHealthCheckPublisher HealthCheckPublisher, TimeSpan duration) =>
			HealthCheckPublisherEnd(logger, HealthCheckPublisher, duration.TotalMilliseconds);

		[LoggerMessage(EventIds.HealthCheckPublisherEndId, LogLevel.Debug, "Health check '{HealthCheckPublisher}' completed after {ElapsedMilliseconds}ms", EventName = EventIds.HealthCheckPublisherEndName)]
		private static partial void HealthCheckPublisherEnd(ILogger logger, IHealthCheckPublisher HealthCheckPublisher, double ElapsedMilliseconds);

		public static void HealthCheckPublisherError(ILogger logger, IHealthCheckPublisher publisher, TimeSpan duration, Exception exception) =>
			HealthCheckPublisherError(logger, publisher, duration.TotalMilliseconds, exception);

#pragma warning disable SYSLIB1006
		[LoggerMessage(EventIds.HealthCheckPublisherErrorId, LogLevel.Error, "Health check {HealthCheckPublisher} threw an unhandled exception after {ElapsedMilliseconds}ms", EventName = EventIds.HealthCheckPublisherErrorName)]
		private static partial void HealthCheckPublisherError(ILogger logger, IHealthCheckPublisher HealthCheckPublisher, double ElapsedMilliseconds, Exception exception);

		public static void HealthCheckPublisherTimeout(ILogger logger, IHealthCheckPublisher publisher, TimeSpan duration) =>
			HealthCheckPublisherTimeout(logger, publisher, duration.TotalMilliseconds);

		[LoggerMessage(EventIds.HealthCheckPublisherTimeoutId, LogLevel.Error, "Health check {HealthCheckPublisher} was canceled after {ElapsedMilliseconds}ms", EventName = EventIds.HealthCheckPublisherTimeoutName)]
		private static partial void HealthCheckPublisherTimeout(ILogger logger, IHealthCheckPublisher HealthCheckPublisher, double ElapsedMilliseconds);
#pragma warning restore SYSLIB1006
	}
}
