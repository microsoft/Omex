/***************************************************************************************************
	FileResourceMonitorUnitTests.cs

	Unit tests for the UnitTestFileResourceMonitor.
***************************************************************************************************/

using System;
using System.Threading;
using Microsoft.Omex.System.Data;
using Microsoft.Omex.System.Data.FileSystem;
using Microsoft.Omex.System.Monads;
using Microsoft.Omex.System.UnitTests.Shared;
using Microsoft.Omex.System.UnitTests.Shared.Data.FileSystem;
using Xunit;

namespace MS.Internal.Motif.Office.Web.OfficeMarketplace.Shared.Data
{
	/// <summary>
	/// Unit tests for the FileResourceMonitor.
	/// </summary>
	public class FileResourceMonitorUnitTests : UnitTestBase
	{
		[Fact]
		public void TryStartMonitoring_WithNullArgument_ReturnsFalse()
		{
			FailOnErrors = false;

			using (UnitTestFileResourceMonitor monitor = new UnitTestFileResourceMonitor())
			{
				Assert.False(monitor.TryStartMonitoring(null, PerformEvent));
				Assert.False(monitor.IsEnabled);
			}
		}


		[Fact]
		public void TryStartMonitoring_WithIncorrectResource_ReturnsFalse()
		{
			FailOnErrors = false;

			using (UnitTestFileResourceMonitor monitor = new UnitTestFileResourceMonitor())
			{
				Assert.False(monitor.TryStartMonitoring(new[]
					{
						new FileResource(new UnitTestTextFile(false, false, "contents"), "folder", "name")
					}, PerformEvent));
				Assert.False(monitor.IsEnabled);
			}
		}


		[Fact]
		public void TryStartMonitoring_WithValidResource_ReturnsTrue()
		{
			using (UnitTestFileResourceMonitor monitor = new UnitTestFileResourceMonitor())
			{
				Assert.True(monitor.TryStartMonitoring(new[]
					{
						new FileResource(new UnitTestTextFile(true, true, "contents"), "folder", "name")
					},
					PerformEvent));
				WaitForEnabled(monitor);
			}
		}


		[Fact]
		public void TryStartMonitoring_CalledTwice_ReturnsFalse()
		{
			FailOnErrors = false;

			using (UnitTestFileResourceMonitor monitor = new UnitTestFileResourceMonitor())
			{
				Assert.True(monitor.TryStartMonitoring(new[]
					{
						new FileResource(new UnitTestTextFile(true, true, "contents"), "folder", "name")
					},
					PerformEvent));
				WaitForEnabled(monitor);

				Assert.False(monitor.TryStartMonitoring(new[]
					{
						new FileResource(new UnitTestTextFile(true, true, "contents"), "folder", "name")
					},
					PerformEvent));
				WaitForEnabled(monitor);
			}
		}


		[Fact]
		public void StopMonitoring_WithoutStart_DisablesAction()
		{
			using (UnitTestFileResourceMonitor monitor = new UnitTestFileResourceMonitor())
			{
				monitor.StopMonitoring(PerformEvent);
				Assert.False(monitor.IsEnabled);
			}
		}


		[Fact]
		public void Dispose_WithoutStart_DisablesAction()
		{
			using (UnitTestFileResourceMonitor monitor = new UnitTestFileResourceMonitor())
			{
				monitor.Dispose();
				Assert.False(monitor.IsEnabled);
			}
		}


		[Fact]
		public void Dispose_WithStart_DisablesAction()
		{
			using (UnitTestFileResourceMonitor monitor = new UnitTestFileResourceMonitor())
			{
				Assert.True(monitor.TryStartMonitoring(new[]
					{
						new FileResource(new UnitTestTextFile(true, true, "contents"), "folder", "name")
					},
					PerformEvent));
				WaitForEnabled(monitor);

				monitor.Dispose();
				Assert.False(monitor.IsEnabled);
			}
		}


		[Fact]
		public void ResourceMonitor_WhenResourceUpdates_CausesCallbackToBeCalled()
		{
			using (UnitTestFileResourceMonitor monitor = new UnitTestFileResourceMonitor())
			{
				UnitTestTextFile file = new UnitTestTextFile(true, true, "contents");
				FileResource resource = new FileResource(file, "folder", "name");
				Assert.True(monitor.TryStartMonitoring(new[] { resource }, PerformEvent));
				m_handlerCalledIndicator = false;
				file.WriteAllBytes("", new byte[] { 0, 1 });
				DateTime monitoringStartMoment = DateTime.UtcNow;
				TimeSpan monitoringMaxTime = new TimeSpan(0, 0, 10);
				while (!m_handlerCalledIndicator)
				{
					if (DateTime.UtcNow >= monitoringStartMoment + monitoringMaxTime)
					{
						break;
					}
				}

				Assert.True(m_handlerCalledIndicator);
			}
		}


		[Fact]
		public void ResourceMonitor_WhenMultipleResourcesUpdated_CorrectCallbacksAreCalled()
		{
			using (UnitTestFileResourceMonitor monitor = new UnitTestFileResourceMonitor())
			{
				UnitTestTextFile file1 = new UnitTestTextFile(true, true, "contents1");
				FileResource resource1 = new FileResource(file1, "folder", "name1");
				UnitTestTextFile file2 = new UnitTestTextFile(true, true, "contents2");
				FileResource resource2 = new FileResource(file2, "folder", "name2");
				bool handlerCalled1 = false;
				bool handlerCalled2 = false;
				ResourceUpdatedHandler handler1 = eventArgs => { handlerCalled1 = true; };
				ResourceUpdatedHandler handler2 = eventArgs => { handlerCalled2 = true; };

				Assert.True(monitor.TryStartMonitoring(new[] { resource1 }, handler1));
				Assert.True(monitor.TryStartMonitoring(new[] { resource2 }, handler2));
				DateTime monitoringStartMoment = DateTime.UtcNow;
				while (!handlerCalled1 || !handlerCalled2)
				{
					if (DateTime.UtcNow >= monitoringStartMoment + m_monitoringMaxTime)
					{
						break;
					}
				}

				Assert.True(handlerCalled1 && handlerCalled2);

				handlerCalled1 = false;
				handlerCalled2 = false;

				monitoringStartMoment = DateTime.UtcNow;
				file2.WriteAllBytes("", new byte[] { 1 });
				while (!handlerCalled2)
				{
					if (DateTime.UtcNow >= monitoringStartMoment + m_monitoringMaxTime)
					{
						break;
					}
				}

				Assert.True(handlerCalled2);
				Assert.False(handlerCalled1);
			}
		}


		[Fact]
		public void ResourceMonitor_WhenResourceMonitoredByMultipleParties_CorrectCallbacksAreCalled()
		{
			using (UnitTestFileResourceMonitor monitor = new UnitTestFileResourceMonitor())
			{
				UnitTestTextFile file1 = new UnitTestTextFile(true, true, "contents1");
				FileResource resource1 = new FileResource(file1, "folder", "name1");
				UnitTestTextFile file2 = new UnitTestTextFile(true, true, "contents2");
				FileResource resource2 = new FileResource(file2, "folder", "name2");

				bool handlerCalled1 = false;
				bool handlerCalled12 = false;
				bool handlerCalled2 = false;
				ResourceUpdatedHandler handler1 = eventArgs => { handlerCalled1 = true; };
				ResourceUpdatedHandler handler12 = eventArgs => { handlerCalled12 = true; };
				ResourceUpdatedHandler handler2 = eventArgs => { handlerCalled2 = true; };

				Assert.True(monitor.TryStartMonitoring(new[] { resource1 }, handler1));
				Assert.True(monitor.TryStartMonitoring(new[] { resource1, resource2 }, handler12));
				Assert.True(monitor.TryStartMonitoring(new[] { resource2 }, handler2));
				DateTime monitoringStartMoment = DateTime.UtcNow;
				while (!handlerCalled1 || !handlerCalled2 || !handlerCalled12)
				{
					if (DateTime.UtcNow >= monitoringStartMoment + m_monitoringMaxTime)
					{
						break;
					}
				}

				Assert.True(handlerCalled1 && handlerCalled2 && handlerCalled12);
				handlerCalled1 = false;
				handlerCalled12 = false;
				handlerCalled2 = false;

				monitoringStartMoment = DateTime.UtcNow;
				file1.WriteAllBytes("", new byte[] { 1 });
				while (!handlerCalled1 || !handlerCalled12)
				{
					if (DateTime.UtcNow >= monitoringStartMoment + m_monitoringMaxTime)
					{
						break;
					}
				}

				Assert.True(handlerCalled1);
				Assert.True(handlerCalled12);
				Assert.False(handlerCalled2);
			}
		}


		/// <summary>
		/// Performs the event.
		/// </summary>
		/// <param name="e">The <see cref="ResourceUpdatedEventArgs"/> instance containing the event data.</param>
		/// <owner alias="mwoulfe">Muiris Woulfe</owner>
		private void PerformEvent(ResourceUpdatedEventArgs e)
		{
			m_handlerCalledIndicator = true;
		}


		/// <summary>
		/// Waits for a file resource monitor to become enabled.
		/// </summary>
		/// <param name="monitor">The monitor on which to wait.</param>
		/// <owner alias="mwoulfe">Muiris Woulfe</owner>
		private void WaitForEnabled(IResourceMonitor monitor)
		{
			int counter = 0;
			while (!monitor.IsEnabled && counter < 5)
			{
				Thread.Sleep(500);
				counter++;
			}

			Assert.True(monitor.IsEnabled);
		}


		/// <summary>
		/// An indicator that the ResourceUpdateHandler was called
		/// </summary>
		/// <owner alias="kobalyts" />
		private bool m_handlerCalledIndicator;


		/// <summary>
		/// Maximum time we dedicate to resource monitoring in the unit test
		/// </summary>
		/// <owner alias="kobalyts" />
		private TimeSpan m_monitoringMaxTime = new TimeSpan(0, 0, 10);


		/// <summary>
		/// A unit test wrapper around the FileResourceMonitor class.
		/// </summary>
		/// <owner alias="mwoulfe">Muiris Woulfe</owner>
		private sealed class UnitTestFileResourceMonitor : ResourceMonitor
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="UnitTestFileResourceMonitor"/> class.
			/// </summary>
			/// <owner alias="mwoulfe">Muiris Woulfe</owner>
			public UnitTestFileResourceMonitor()
				: base(new RetryPolicy(new LinearBackoffPolicy(), 1, 1), TimeSpan.FromMilliseconds(1))
			{
			}
		}
	}
}
