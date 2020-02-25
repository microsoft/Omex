using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Omex.Extensions.Logging.UnitTests
{
	[TestClass]
	public class OmexLogsEventSourceTests
	{
		[DataTestMethod]
		[DataRow(EventLevel.Error, LogLevel.Error)]
		[DataRow(EventLevel.Warning, LogLevel.Warning)]
		[DataRow(EventLevel.Informational, LogLevel.Information)]
		public void EventSourceLogsMessage(EventLevel eventLevel, LogLevel logLevel)
		{
			OmexLogsEventSource logEvent = new OmexLogsEventSource(new BasicMachineInformation(), new EmptyServiceContext());

			CustomEventListener listener = new CustomEventListener();
			listener.EnableEvents(logEvent, eventLevel);

			int eventId = 0x4FFFFFFF;
			logEvent.LogMessage("0", default, "0", logLevel, eventId, 0, "My message ");

			EventWrittenEventArgs eventInfo = listener.EventsInformation.Single();
			//TODO check that event agrs are correct
		}


		private class CustomEventListener : EventListener
		{
			public List<EventWrittenEventArgs> EventsInformation { get; } = new List<EventWrittenEventArgs>();

			protected override void OnEventWritten(EventWrittenEventArgs eventData) => EventsInformation.Add(eventData);
		}
	}
}
