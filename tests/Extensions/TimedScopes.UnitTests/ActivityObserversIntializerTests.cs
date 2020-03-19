// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hosting.Services.UnitTests
{
	[TestClass]
	public class ActivityObserversIntializerTests
	{
		[TestMethod]
		public async Task ActivityObserversInvokedProperly()
		{
			string name = nameof(ActivityObserversInvokedProperly);
			Mock<IActivityStartObserver> startObserver = new Mock<IActivityStartObserver>();
			Mock<IActivityStopObserver> stopObserver = new Mock<IActivityStopObserver>();
			ActivityObserversIntializer initializer = new ActivityObserversIntializer(
				new [] { startObserver.Object },
				new [] { stopObserver.Object });

			try
			{
				await initializer.StartAsync(CancellationToken.None);

				using DiagnosticListener listener = new DiagnosticListener(name);

				Assert.IsTrue(listener.IsEnabled(MakeStartName(name)), "Should be enabled for Activity.Start");
				Assert.IsFalse(listener.IsEnabled(name, "Should be disabled for other events"));
				Assert.IsTrue(listener.IsEnabled(MakeStopName(name)), "Should be enabled for Activity.Stop");

				Activity activity = new Activity(name);
				object obj = new object();

				listener.StartActivity(activity, obj);
				startObserver.Verify(obs => obs.OnStart(activity, obj), Times.Once);

				listener.StopActivity(activity, obj);
				stopObserver.Verify(obs => obs.OnStop(activity, obj), Times.Once);
			}
			catch
			{
				await initializer.StopAsync(CancellationToken.None);
				throw;
			}
		}

		private string MakeStartName(string name) => name + ".Start";

		private string MakeStopName(string name) => name + ".Stop";
	}
}
