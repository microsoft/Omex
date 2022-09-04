// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

// Unit tests extended from: https://github.com/dotnet/aspnetcore/blob/main/src/HealthChecks/HealthChecks/test/HealthCheckPublisherHostedServiceTest.cs

#pragma warning disable IDE1006 // ASP.NET Core codebase naming convention
#pragma warning disable IDE0007 // Use explicit type

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Preview.Extensions.DependencyInjection;
using Microsoft.Omex.Preview.Extensions.Diagnostics.HealthChecks.UnitTests.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Preview.Extensions.Diagnostics.HealthChecks.UnitTests
{
	[TestClass]
	public class OmexHealthCheckPublisherHostedServiceTest
	{
		[TestMethod]
		public async Task StartAsync_WithoutPublishers_DoesNotStartTimer()
		{
			// Arrange
			var publishers = Array.Empty<IHealthCheckPublisher>();

			var service = CreateService(publishers);

			try
			{
				// Act
				await service.StartAsync();

				// Assert
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsFalse(service.IsStopping);
			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task StartAsync_WithPublishers_StartsTimer()
		{
			// Arrange
			var publishers = new IHealthCheckPublisher[]
			{
				new TestPublisher(),
			};

			var service = CreateService(publishers);

			try
			{
				// Act
				await service.StartAsync();

				// Assert
				Assert.IsFalse(service.IsStopping);
				Assert.IsTrue(service.IsTimerRunning);
			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task StartAsync_WithPublishers_StartsTimer_RunsPublishers()
		{
			// Arrange
			var unblock0 = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			var unblock1 = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			var unblock2 = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

			var publishers = new TestPublisher[]
			{
				new TestPublisher() { Wait = unblock0.Task, },
				new TestPublisher() { Wait = unblock1.Task, },
				new TestPublisher() { Wait = unblock2.Task, },
			};

			OmexHealthCheckPublisherHostedService? service = CreateService(publishers, configurePublisherOptions: (options) =>
			{
				options.Delay = TimeSpan.FromMilliseconds(0);
			});

			try
			{
				// Act
				await service.StartAsync();

				await publishers[0].Started.TimeoutAfter(TimeSpan.FromSeconds(10));
				await publishers[1].Started.TimeoutAfter(TimeSpan.FromSeconds(10));
				await publishers[2].Started.TimeoutAfter(TimeSpan.FromSeconds(10));

				unblock0.SetResult(null);
				unblock1.SetResult(null);
				unblock2.SetResult(null);

				// Assert
				Assert.IsTrue(service.IsTimerRunning);
				Assert.IsFalse(service.IsStopping);
			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task StopAsync_CancelsExecution()
		{
			// Arrange
			var unblock = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

			var publishers = new TestPublisher[]
			{
				new TestPublisher() { Wait = unblock.Task, }
			};

			OmexHealthCheckPublisherHostedService? service = CreateService(publishers);

			try
			{
				await service.StartAsync();

				// Start execution
				var running = service.RunAsync();

				// Wait for the publisher to see the cancellation token
				await publishers[0].Started.TimeoutAfter(TimeSpan.FromSeconds(10));
				Assert.IsTrue(publishers[0].Entries.Count == 1);

				// Act
				await service.StopAsync(); // Trigger cancellation

				// Assert
				await AssertCanceledAsync(publishers[0].Entries[0].cancellationToken);
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);

				unblock.SetResult(null);

				await running.TimeoutAfter(TimeSpan.FromSeconds(10));
			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task RunAsync_WaitsForCompletion_Single()
		{
			// Arrange
			var unblock = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

			var publishers = new TestPublisher[]
			{
				new TestPublisher() { Wait = unblock.Task, },
			};

			OmexHealthCheckPublisherHostedService? service = CreateService(publishers);

			try
			{
				await service.StartAsync();

				// Act
				var running = service.RunAsync();

				await publishers[0].Started.TimeoutAfter(TimeSpan.FromSeconds(10));

				unblock.SetResult(null);

				await running.TimeoutAfter(TimeSpan.FromSeconds(10));

				// Assert
				Assert.IsTrue(service.IsTimerRunning);
				Assert.IsFalse(service.IsStopping);

				for (var i = 0; i < publishers.Length; i++)
				{
					Assert.IsTrue(publishers[i].Entries.Count == 1);
					var report = publishers[i].Entries.Single().report;
					CollectionAssert.AreEqual(new[] { "one", "two", }, report.Entries.Keys.OrderBy(k => k).ToArray());
				}
			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task RunAsync_WaitsForCompletion_Single_RegistrationParameters()
		{
			// Arrange
			const string HealthyMessage = "Everything is A-OK";

			var unblock = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			var unblockDelayedCheck = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

			var publishers = new TestPublisher[]
			{
				new TestPublisher() { Wait = unblock.Task, },
			};

			var service = CreateService(publishers, configureBuilder: b =>
			{
				b.AddAsyncCheck("CheckDefault", _ =>
				{
					return Task.FromResult(HealthCheckResult.Healthy(HealthyMessage));
				});

				b.AddAsyncCheck("CheckDelay2Period18", _ =>
				{
					return new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy(HealthyMessage));
				},
				parameters: new(name: "CheckDelay2Period18", delay: TimeSpan.FromSeconds(2), period: TimeSpan.FromSeconds(18)));

				b.AddAsyncCheck("CheckDelay7Period11", _ =>
				{
					return new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy(HealthyMessage));
				},
				parameters: new(name: "CheckDelay7Period11", delay: TimeSpan.FromSeconds(7), period: TimeSpan.FromSeconds(11)));

				b.AddAsyncCheck("CheckDelay9Period5", _ =>
				{
					unblockDelayedCheck.TrySetResult(null); // Unblock last delayed check
					return new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy(HealthyMessage));
				},
				parameters: new(name: "CheckDelay9Period5", delay: TimeSpan.FromSeconds(9), period: TimeSpan.FromSeconds(5)));

				b.AddAsyncCheck("DisabledCheck", _ =>
				{
					unblockDelayedCheck.TrySetResult(null); // Unblock last delayed check
					return new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy(HealthyMessage));
				},
				parameters: new(name: "DisabledCheck", delay: TimeSpan.FromSeconds(9), period: TimeSpan.FromSeconds(5), isEnabled: false));
			});

			try
			{
				await service.StartAsync();

				// Act
				var running = service.RunAsync();

				await publishers[0].Started.TimeoutAfter(TimeSpan.FromSeconds(10));

				unblock.SetResult(null);

				await running.TimeoutAfter(TimeSpan.FromSeconds(20));

				await Task.WhenAll(unblock.Task, unblockDelayedCheck.Task);

				// Assert
				Assert.IsTrue(service.IsTimerRunning);
				Assert.IsFalse(service.IsStopping);

				for (var i = 0; i < publishers.Length; i++)
				{
					Assert.IsTrue(publishers[i].Entries.Count == 4);
					var entries = publishers[i].Entries.SelectMany(e => e.report.Entries.Select(e2 => e2.Key)).OrderBy(k => k).ToArray();
					CollectionAssert.AreEqual(
						new[] { "CheckDefault", "CheckDelay2Period18", "CheckDelay7Period11", "CheckDelay9Period5" },
						entries);
				}
			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task RunAsync_WaitsForCompletion_Multiple()
		{
			// Arrange
			var unblock0 = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			var unblock1 = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			var unblock2 = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

			var publishers = new TestPublisher[]
			{
				new TestPublisher() { Wait = unblock0.Task, },
				new TestPublisher() { Wait = unblock1.Task, },
				new TestPublisher() { Wait = unblock2.Task, },
			};

			OmexHealthCheckPublisherHostedService? service = CreateService(publishers);

			try
			{
				await service.StartAsync();

				// Act
				var running = service.RunAsync();

				await publishers[0].Started.TimeoutAfter(TimeSpan.FromSeconds(10));
				await publishers[1].Started.TimeoutAfter(TimeSpan.FromSeconds(10));
				await publishers[2].Started.TimeoutAfter(TimeSpan.FromSeconds(10));

				unblock0.SetResult(null);
				unblock1.SetResult(null);
				unblock2.SetResult(null);

				await running.TimeoutAfter(TimeSpan.FromSeconds(10));

				// Assert
				Assert.IsTrue(service.IsTimerRunning);
				Assert.IsFalse(service.IsStopping);

				for (var i = 0; i < publishers.Length; i++)
				{
					Assert.IsTrue(publishers[i].Entries.Count == 1);
					var report = publishers[i].Entries.Single().report;
					CollectionAssert.AreEqual(new[] { "one", "two", }, report.Entries.Keys.OrderBy(k => k).ToArray());
				}
			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task RunAsync_PublishersCanTimeout()
		{
			// Arrange
			var unblock = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

			var publishers = new TestPublisher[]
			{
				new TestPublisher() { Wait = unblock.Task, },
			};

			var service = CreateService(publishers);

			try
			{
				await service.StartAsync();

				// Act
				var running = service.RunAsync();

				await publishers[0].Started.TimeoutAfter(TimeSpan.FromSeconds(10));

				service.CancelToken();

				await AssertCanceledAsync(publishers[0].Entries[0].cancellationToken);

				unblock.SetResult(null);

				await running.TimeoutAfter(TimeSpan.FromSeconds(10));

				// Assert
				Assert.IsTrue(service.IsTimerRunning);
				Assert.IsFalse(service.IsStopping);
			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task RunAsync_CanFilterHealthChecks()
		{
			// Arrange
			var publishers = new TestPublisher[]
			{
				new TestPublisher(),
				new TestPublisher(),
			};

			var service = CreateService(publishers, configurePublisherOptions: (options) =>
			{
				options.Predicate = (r) => r.Name == "one";
			});

			try
			{
				await service.StartAsync();

				// Act
				await service.RunAsync().TimeoutAfter(TimeSpan.FromSeconds(10));

				// Assert
				for (var i = 0; i < publishers.Length; i++)
				{
					Assert.IsTrue(publishers[i].Entries.Count == 1);
					var report = publishers[i].Entries.Single().report;
					CollectionAssert.AreEqual(new[] { "one" }, report.Entries.Keys.OrderBy(k => k).ToArray());
				}
			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task RunAsync_CanFilterHealthChecks_RegistrationParameters()
		{
			// Arrange
			const string HealthyMessage = "Everything is A-OK";

			var unblockDelayedCheck = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

			var publishers = new TestPublisher[]
			{
				new TestPublisher(),
				new TestPublisher(),
			};

			var service = CreateService(
				publishers,
				configurePublisherOptions: (options) =>
				{
					options.Predicate = (r) => r.Name.Contains("Delay");
				},
				configureBuilder: b =>
				{
					b.AddAsyncCheck("CheckDefault", _ =>
					{
						return Task.FromResult(HealthCheckResult.Healthy(HealthyMessage));
					});

					b.AddAsyncCheck("CheckDelay2Period18", _ =>
					{
						return new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy(HealthyMessage));
					},
					parameters: new(name: "CheckDelay2Period18", delay: TimeSpan.FromSeconds(2), period: TimeSpan.FromSeconds(18)));

					b.AddAsyncCheck("CheckDelay7Period11", _ =>
					{
						return new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy(HealthyMessage));
					},
					parameters: new(name: "CheckDelay7Period11", delay: TimeSpan.FromSeconds(7), period: TimeSpan.FromSeconds(11)));

					b.AddAsyncCheck("CheckDelay9Period5", _ =>
					{
						unblockDelayedCheck.TrySetResult(null); // Unblock last delayed check
						return new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy(HealthyMessage));
					},
					parameters: new(name: "CheckDelay9Period5", delay: TimeSpan.FromSeconds(9), period: TimeSpan.FromSeconds(5)));

					b.AddAsyncCheck("DisabledCheck", _ =>
					{
						unblockDelayedCheck.TrySetResult(null); // Unblock last delayed check
						return new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy(HealthyMessage));
					},
					parameters: new(name: "DisabledCheck", delay: TimeSpan.FromSeconds(9), period: TimeSpan.FromSeconds(5), isEnabled: false));
				});

			try
			{
				await service.StartAsync();

				// Act
				await service.RunAsync().TimeoutAfter(TimeSpan.FromSeconds(20));

				await unblockDelayedCheck.Task;

				// Assert
				for (var i = 0; i < publishers.Length; i++)
				{
					var entries = publishers[i].Entries.SelectMany(e => e.report.Entries.Select(e2 => e2.Key)).OrderBy(k => k).ToArray();

					Assert.IsTrue(entries.Count() == 3);
					CollectionAssert.AreEqual(
						new[] { "CheckDelay2Period18", "CheckDelay7Period11", "CheckDelay9Period5" },
						entries);
				}
			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task RunAsync_HandlesExceptions()
		{
			// Arrange
			var publishers = new TestPublisher[]
			{
				new TestPublisher() { Exception = new InvalidTimeZoneException(), },
			};

			var service = CreateService(publishers);

			try
			{
				await service.StartAsync();

				// Act
				await service.RunAsync().TimeoutAfter(TimeSpan.FromSeconds(10));

			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		[TestMethod]
		public async Task RunAsync_HandlesExceptions_Multiple()
		{
			// Arrange
			var publishers = new TestPublisher[]
			{
				new TestPublisher() { Exception = new InvalidTimeZoneException(), },
				new TestPublisher(),
				new TestPublisher() { Exception = new InvalidTimeZoneException(), },
			};

			var service = CreateService(publishers);

			try
			{
				await service.StartAsync();

				// Act
				await service.RunAsync().TimeoutAfter(TimeSpan.FromSeconds(10));

			}
			finally
			{
				await service.StopAsync();
				Assert.IsFalse(service.IsTimerRunning);
				Assert.IsTrue(service.IsStopping);
			}
		}

		private OmexHealthCheckPublisherHostedService CreateService(
			IHealthCheckPublisher[] publishers,
			Action<HealthCheckPublisherOptions>? configurePublisherOptions = null,
			Action<IOmexHealthChecksBuilder>? configureBuilder = null)
		{
			ServiceCollection serviceCollection = new();
			serviceCollection.AddOptions();
			serviceCollection.AddLogging();
			IOmexHealthChecksBuilder builder = serviceCollection.AddOmexHealthChecks();
			if (configureBuilder == null)
			{
				// Default builder configuration
				builder.AddAsyncCheck("one", () => new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy()))
					   .AddAsyncCheck("two", () => new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy()));
			}
			else
			{
				configureBuilder(builder);
			}

			// Choosing big values for tests to make sure that we're not dependent on the defaults.
			// All of the tests that rely on the timer will set their own values for speed.
			serviceCollection.Configure<HealthCheckPublisherOptions>(options =>
			{
				options.Delay = TimeSpan.FromMinutes(5);
				options.Period = TimeSpan.FromMinutes(5);
				options.Timeout = TimeSpan.FromMinutes(5);
			});

			if (publishers != null)
			{
				for (var i = 0; i < publishers.Length; i++)
				{
					serviceCollection.AddSingleton(publishers[i]);
				}
			}

			if (configurePublisherOptions != null)
			{
				serviceCollection.Configure(configurePublisherOptions);
			}

			var services = serviceCollection.BuildServiceProvider();
			return services.GetServices<IHostedService>().OfType<OmexHealthCheckPublisherHostedService>().Single();
		}

		private static async Task AssertCanceledAsync(CancellationToken cancellationToken)
		{
			await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => Task.Delay(TimeSpan.FromSeconds(10), cancellationToken));
		}

		private class TestPublisher : IHealthCheckPublisher
		{
			private readonly TaskCompletionSource<object?> m_started;

			public TestPublisher()
			{
				m_started = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			}

			public List<(HealthReport report, CancellationToken cancellationToken)> Entries { get; } = new List<(HealthReport report, CancellationToken cancellationToken)>();

			public Exception? Exception { get; set; }

			public Task Started => m_started.Task;

			public Task? Wait { get; set; }

			public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
			{
				Entries.Add((report, cancellationToken));

				// Signal that we've started
				m_started.TrySetResult(null);

				if (Wait != null)
				{
					await Wait;
				}

				if (Exception != null)
				{
					throw Exception;
				}

				cancellationToken.ThrowIfCancellationRequested();
			}
		}
	}
}
