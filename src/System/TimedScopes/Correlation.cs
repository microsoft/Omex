// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.Context;
using Microsoft.Omex.System.Diagnostics;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Correlation class, handle in-memory correlation data for logging purposes.
	/// </summary>
	/// <remarks>
	/// Allows correlation data to be set and retrieved on a calling context. ULS logging (when hooked up)
	/// only allows for the correlation id to be retrieved, this allows for additional data to be retrieved.
	/// Correlation data by default only applies to the current thread and is lost when a thread hop
	/// occurs (be it ASP.NET underlying thread hop or an explicit thread change). By calling
	/// CorrelationStart with cloned CorrelationData, the correlation can be transferred to the new
	/// thread.
	/// </remarks>
	public sealed class Correlation
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="correlationHandler">correlation handler, used to store correlation data in underlying logging system</param>
		/// <param name="callContextManager">call context manager, to handle context operations</param>
		/// <param name="machineInformation">machine information</param>
		public Correlation(ICorrelationStorage correlationHandler, ICallContextManager callContextManager, IMachineInformation machineInformation)
		{
			Code.ExpectsArgument(correlationHandler, nameof(correlationHandler), TaggingUtilities.ReserveTag(0x2381771b /* tag_96x21 */));
			Code.ExpectsArgument(callContextManager, nameof(callContextManager), TaggingUtilities.ReserveTag(0x2381771c /* tag_96x22 */));
			Code.ExpectsArgument(machineInformation, nameof(machineInformation), TaggingUtilities.ReserveTag(0x2381771d /* tag_96x23 */));

			CorrelationHandler = correlationHandler;
			CallContextManager = callContextManager;
			MachineInformation = machineInformation;
		}


		/// <summary>
		/// Current call context
		/// </summary>
		private ICallContext CallContext => CallContextManager.CallContextHandler(MachineInformation);


		/// <summary>
		/// Event raised when the correlation has started
		/// </summary>
		public event EventHandler<CorrelationEventArgs> CorrelationStarted;


		/// <summary>
		/// Event raised when correlation data has been added
		/// </summary>
		public event EventHandler<CorrelationEventArgs> CorrelationDataAdded;


		/// <summary>
		/// Event raised when the correlation has ended
		/// </summary>
		public event EventHandler<CorrelationEventArgs> CorrelationEnded;


		/// <summary>
		/// Key used to store the correlation context in the current HttpContext
		/// </summary>
		private const string CorrelationContextKey = "ULSLogging.correlations";


		/// <summary>
		/// Key used to store if direct logging should be used
		/// </summary>
		private const string LoggingContextKey = "ULSLogging.directLogging";


		/// <summary>
		/// Get the current correlation if set
		/// </summary>
		/// <exception cref="InvalidCastException">Data exists on the CallContext
		/// but it isn't a valid CorrelationData object.</exception>
		public CorrelationData CurrentCorrelation
		{
			get
			{
				return GetCorrelationData();
			}

			private set
			{
				ICallContext existingContext = CallContext.ExistingCallContext();
				if (existingContext != null)
				{
					if (value != null)
					{
						existingContext.Data[CorrelationContextKey] = value;
					}
					else
					{
						existingContext.Data.Remove(CorrelationContextKey);
					}
				}
			}
		}


		/// <summary>
		/// Should log directly to the underlying log handler
		/// </summary>
		public bool ShouldLogDirectly
		{
			get
			{
				CorrelationData correlation = CurrentCorrelation;
				if (correlation?.ShouldLogDirectly == true)
				{
					return true;
				}

				ICallContext existingContext = CallContext.ExistingCallContext();
				if (existingContext != null)
				{
					object data;
					return existingContext.Data.TryGetValue(LoggingContextKey, out data) && data != null;
				}

				return false;
			}

			set
			{
				CorrelationData correlation = CurrentCorrelation;
				if (correlation != null)
				{
					correlation.ShouldLogDirectly = value;
				}
				else
				{
					ICallContext existingContext = CallContext.ExistingCallContext();
					if (existingContext != null)
					{
						if (value)
						{
							existingContext.Data[LoggingContextKey] = new object();
						}
						else
						{
							existingContext.Data.Remove(LoggingContextKey);
						}
					}
				}
			}
		}


		/// <summary>
		/// The correlation handler used to store the correlation
		/// </summary>
		/// <remarks>
		/// Calling methods on this interface bypasses the
		/// event handlers for correlation and commits the correlation
		/// directly to the correlation handler.
		/// </remarks>
		public ICorrelationStorage CorrelationHandler { get; }


		/// <summary>
		/// The correlation handler used to store the correlation
		/// </summary>
		/// <remarks>
		/// Calling methods on this interface bypasses the
		/// event handlers for correlation and commits the correlation
		/// directly to the correlation handler.
		/// </remarks>
		public ICallContextManager CallContextManager { get; }


		/// <summary>
		/// The correlation handler used to store the correlation
		/// </summary>
		/// <remarks>
		/// Calling methods on this interface bypasses the
		/// event handlers for correlation and commits the correlation
		/// directly to the correlation handler.
		/// </remarks>
		public IMachineInformation MachineInformation { get; }


		/// <summary>
		/// Start a correlation
		/// </summary>
		/// <param name="data">correlation to set on the thread, null for default (new) correlation</param>
		public void CorrelationStart(CorrelationData data)
		{
			data = CorrelationHandler.CorrelationStart(data);
			if (data != null)
			{
				data.ParentCorrelation = CurrentCorrelation;
				if (data.ParentCorrelation == null)
				{
					data.ShouldLogDirectly = ShouldLogDirectly;
				}

				CurrentCorrelation = data;

				// Note: Creating a copy of the event handler to avoid multi-threaded race conditions
				// Not creating extension methods to avoid unnecessary creating of arguments if not set
				EventHandler<CorrelationEventArgs> correlationStarted = CorrelationStarted;
				if (correlationStarted != null)
				{
					correlationStarted(this, new CorrelationEventArgs(data));
				}
			}
		}


		/// <summary>
		/// Add data to the current correlation
		/// </summary>
		/// <remarks>If there is no current correlation, starts a new correlation</remarks>
		/// <param name="key">key (name) of the correlation</param>
		/// <param name="value">value of the added correlation data</param>
		public void CorrelationAdd(string key, string value)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(key, nameof(key), TaggingUtilities.ReserveTag(0x2381771e /* tag_96x24 */));
			Code.ExpectsNotNullOrWhiteSpaceArgument(value, nameof(value), TaggingUtilities.ReserveTag(0x2381771f /* tag_96x25 */));

			CorrelationData data = CurrentCorrelation;
			if (data == null)
			{
				CorrelationStart(null);
				data = CurrentCorrelation;
			}

			if (data != null)
			{
				string oldData = data.Data(key);

				CorrelationHandler.CorrelationAdd(key, value, data);

				EventHandler<CorrelationEventArgs> correlationDataAdded = CorrelationDataAdded;
				if (correlationDataAdded != null)
				{
					correlationDataAdded(this, new CorrelationEventArgs(data, key, oldData));
				}
			}
		}


		/// <summary>
		/// Get correlation data from a thread
		/// </summary>
		/// <param name="threadId">Id of the thread</param>
		/// <returns>Correlation data</returns>
		private CorrelationData GetCorrelationData(int? threadId = null)
		{
			ICallContext existingContext = CallContext.ExistingCallContext(threadId);
			if (existingContext != null)
			{
				object data;
				if (existingContext.Data.TryGetValue(CorrelationContextKey, out data))
				{
					return data as CorrelationData;
				}
			}

			return null;
		}


		/// <summary>
		/// End the correlation
		/// </summary>
		/// <param name="id">Id of the thread</param>
		/// <param name="invokeEventHandler">Should we invoke the correlation ended event handler</param>
		public void CorrelationEnd(int? id = null, bool invokeEventHandler = true)
		{
			CorrelationData correlationData = GetCorrelationData(id);
			if (correlationData != null)
			{
				CurrentCorrelation = correlationData.ParentCorrelation;

				CorrelationHandler.CorrelationEnd(correlationData);

				if (invokeEventHandler)
				{
					EventHandler<CorrelationEventArgs> correlationEnded = CorrelationEnded;
					if (correlationEnded != null)
					{
						correlationEnded(this, new CorrelationEventArgs(correlationData));
					}
				}
			}
		}


		/// <summary>
		/// Clear all correlations from the current thread
		/// </summary>
		public void CorrelationClear()
		{
			while (CurrentCorrelation != null)
			{
				CorrelationEnd();
			}
		}
	}
}
