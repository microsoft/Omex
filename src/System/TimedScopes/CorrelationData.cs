// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Correlation data used for logging purposes
	/// </summary>
	public sealed class CorrelationData
	{
		/// <summary>
		/// Dictionary storing additional correlation data, only used when
		/// non-standard correlation data (AddData) is used.
		/// </summary>
		/// <remarks>ListDictionary choosen since the correlation data
		/// should be small (less than 10 items).</remarks>
		private ListDictionary m_additionalData;


		/// <summary>
		/// Parent correlation for nested correlations.
		/// </summary>
		private CorrelationData m_parentCorrelation;


		/// <summary>
		/// Should log directly to the underlying log handler
		/// </summary>
		/// <remarks>
		/// This forces any logging with this correlation data to bypass
		/// any logging handlers.
		/// </remarks>
		private bool m_shouldLogDirectly;


		/// <summary>
		/// Is this correlation running in ULS replay mode
		/// </summary>
		/// <remarks>
		/// When first set, all previous ULS traces are replayed under the tag_repl event Id, then
		/// subsequent events are also replayed until the end of the correlation; all events are written at Info level
		/// </remarks>
		private bool m_ulsReplay;


		/// <summary>
		/// Information if previously cached ULS events for this correlation already been replayed
		/// </summary>
		private int m_cachedUlsEventsReplayed;


		/// <summary>
		/// Event sequence number for guarenteed logging order within a correlation
		/// </summary>
		private long m_eventSequenceNumber;


		/// <summary>
		/// Log event cache reference for triggering replay events.
		/// </summary>
		private readonly ILogEventCache m_logEventCache;


		/// <summary>
		/// Root scopes have this sequence number (with high probability),
		/// non-root scopes have always a larger sequence number.
		/// </summary>
		public const int RootScopeBoundary = 1000;


		/// <summary>
		/// Transaction Id key
		/// </summary>
		public const string TransactionIdKey = "TransactionId";


		/// <summary>
		/// The ID of the correlation
		/// </summary>
		/// <remarks>Seting this value only modifies the in memory representation
		/// of the correlation data, to modify the actual correlation data, use
		/// Correlation.CorrelationStart(CorrelationData correlation).</remarks>
		public Guid VisibleId { get; set; }


		/// <summary>
		/// Transaction id for the correlation
		/// </summary>
		/// <remarks>If non-zero, indicates that the correlation data
		/// is used as part of a specific monitored transaction</remarks>
		public uint TransactionId { get; set; }


		/// <summary>
		/// Id of a registered ITransactionContext to apply
		/// </summary>
		public uint TransactionContextId { get; set; }


		/// <summary>
		/// Step number for a multi-step Transaction
		/// </summary>
		public uint TransactionStep { get; set; }


		/// <summary>
		/// Is this a fallback call
		/// </summary>
		public bool IsFallbackCall { get; set; }


		/// <summary>
		/// Tracks the depth of the correlation through proxying layers
		/// Original caller is Depth 0, subsequent hops are +1 each time
		/// </summary>
		public uint CallDepth { get; set; }


		/// <summary>
		/// Gets or sets the user hash for the current correlation.  Only call the setter directly
		/// if you already have a hashed value, use a hashing function to compute the hash
		/// of a user string and set it.
		/// This can be set by web service calls where there is user information available.
		/// Its value is read and logged by <see cref="TimedScope.EndScope"/>.
		/// </summary>
		/// <remarks>Used to count timed scope hits by user.</remarks>
		public string UserHash { get; set; }


		/// <summary>
		/// Current event sequence number for this correlation
		/// </summary>
		public long EventSequenceNumber
		{
			get
			{
				return m_eventSequenceNumber;
			}

			set
			{
				m_eventSequenceNumber = value;
			}
		}


		/// <summary>
		/// All correlation data keys
		/// </summary>
		/// <remarks>Null if there are no correlation data keys</remarks>
		public ICollection Keys
		{
			get
			{
				if (m_additionalData == null)
				{
					return null;
				}
				else
				{
					return m_additionalData.Keys;
				}
			}
		}


		/// <summary>
		/// Does the correlation have any correlation data
		/// </summary>
		public bool HasData
		{
			get
			{
				return m_additionalData == null ? false : m_additionalData.Count > 0;
			}
		}


		/// <summary>
		/// Parent correlation for nested correlation
		/// </summary>
		/// <exception cref="InvalidOperationException">Attempting to set
		/// the parent correlation when it is already set, or setting
		/// the parent correlation causes circular reference</exception>
		/// <remarks>Implemented as a parent node rather than stack
		/// as most correlation will never have a parent correlation.</remarks>
		public CorrelationData ParentCorrelation
		{
			get
			{
				return m_parentCorrelation;
			}

			set
			{
				if (this.ParentCorrelation != null)
				{
					throw new InvalidOperationException("Parent correlation is already set.");
				}

				CorrelationData temp = value;
				while (temp != null)
				{
					if (temp == this)
					{
						throw new InvalidOperationException("Cannot set the parent correlation as it will cause circular reference.");
					}

					temp = temp.ParentCorrelation;
				}

				m_parentCorrelation = value;
			}
		}


		/// <summary>
		/// Allocates the next sequence number to be used for logging purposes
		/// </summary>
		/// <returns>Incremented sequence number unique within the correlation</returns>
		public long NextEventSequenceNumber()
		{
			return Interlocked.Increment(ref m_eventSequenceNumber);
		}


		/// <summary>
		/// Allocates the next sequence number to be used for logging the
		/// start of a new scope.
		/// </summary>
		/// <returns>Incremented sequence number unique within the correlation</returns>
		public long NextScopeStartSequenceNumber()
		{
			// Make the root scope of a correlation identifiable
			// by giving it a sequence of at least 'RootScopeBoundary'.
			// Note that this is only a heuristic, but avoids excessive code changes.
			if (m_eventSequenceNumber < RootScopeBoundary)
			{
				m_eventSequenceNumber = RootScopeBoundary;
				return m_eventSequenceNumber;
			}

			return Interlocked.Increment(ref m_eventSequenceNumber);
		}


		/// <summary>
		/// Should log directly to the underlying log handler
		/// </summary>
		public bool ShouldLogDirectly
		{
			get
			{
				return m_shouldLogDirectly || (ParentCorrelation != null && ParentCorrelation.ShouldLogDirectly);
			}

			set
			{
				m_shouldLogDirectly = value;
			}
		}


		/// <summary>
		/// Determines if ULS events should be replayed for the current correlation.
		/// Setting this to true will only affect events logged afterwards, see ReplayPreviouslyCachedUlsEvents below.
		/// One-way switch, cannot be set back to false.
		/// </summary>
		public bool ShouldReplayUls
		{
			get
			{
				return m_ulsReplay;
			}

			set
			{
				// this flag is a one way switch
				if (value == false)
				{
					return;
				}

				m_ulsReplay = true;
			}
		}


		/// <summary>
		/// Have previously cached ULS events for this correlation already been replayed?
		/// </summary>
		public bool CachedUlsEventsReplayed
		{
			get
			{
				return m_cachedUlsEventsReplayed == 1;
			}
		}


		/// <summary>
		/// Replay previously cached ULS events for this correlation now.
		/// This should be called along with ShouldReplayUls = true to replay previous events in the correlation.
		/// Will only run once per correlation.
		/// </summary>
		public void ReplayPreviouslyCachedUlsEvents()
		{
			int originalValue = Interlocked.CompareExchange(ref m_cachedUlsEventsReplayed, 1, 0);

			if (originalValue == 0)
			{
				m_logEventCache.ReplayCorrelation(VisibleId);
			}
		}


		/// <summary>
		/// Get the correlation data for a key
		/// </summary>
		/// <param name="key">key to get data for</param>
		/// <returns>string containing the data, null if it doesn't exist</returns>
		public string Data(string key)
		{
			if (m_additionalData == null || key == null)
			{
				return null;
			}

			return (string)m_additionalData[key];
		}


		/// <summary>
		/// Add correlation data
		/// </summary>
		/// <param name="key">key to get data for</param>
		/// <param name="value">value of data</param>
		/// <remarks>Calling this method only modifies the in memory representation
		/// of the correlation data, to modify the actual correlation data, use
		/// Correlation.CorrelationAdd(string key, string value).</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="value"/> is null</exception>
		public void AddData(string key, string value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			if (string.Equals(key, TransactionIdKey, StringComparison.OrdinalIgnoreCase))
			{
				// Caching the TransactionId specifically as it is likely to be queried for
				// repeatedly
				uint transactionId;
				if (uint.TryParse(value, out transactionId))
				{
					TransactionId = transactionId;
				}
				else
				{
					TransactionId = 0;
				}
			}

			if (m_additionalData == null)
			{
				m_additionalData = new ListDictionary(StringComparer.OrdinalIgnoreCase);
			}

			m_additionalData[key] = value;
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="cachedUlsEventsReplayed">Have previously cached ULS events for this correlation already been replayed?</param>
		public CorrelationData(bool cachedUlsEventsReplayed = false)
		{
			m_cachedUlsEventsReplayed = cachedUlsEventsReplayed ? 1 : 0;
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="logEventCache">Log event cache</param>
		/// <param name="cachedUlsEventsReplayed">Have previously cached ULS events for this correlation already been replayed?</param>
		public CorrelationData(ILogEventCache logEventCache, bool cachedUlsEventsReplayed = false)
		{
			Code.ExpectsArgument(logEventCache, nameof(logEventCache), TaggingUtilities.ReserveTag(0x2381771a /* tag_96x20 */));
			m_logEventCache = logEventCache;
			m_cachedUlsEventsReplayed = cachedUlsEventsReplayed ? 1 : 0;
		}
	}


	/// <summary>
	/// Correlation data extension methods
	/// </summary>
	public static class CorrelationDataExtensionMethods
	{
		/// <summary>
		/// Clone the correlation
		/// </summary>
		/// <param name="existingCorrelation">existing correlation</param>
		/// <returns>cloned correlation, null if existing correlation is null</returns>
		/// <remarks>Added as a extension method instead of a method on the
		/// object to allow for cloning of the data when it is null, i.e.
		/// CorrelationData data = null; data.Clone(); will not throw exception.</remarks>
		public static CorrelationData Clone(this CorrelationData existingCorrelation)
		{
			if (existingCorrelation == null)
			{
				return null;
			}

			CorrelationData newCorrelation = new CorrelationData(existingCorrelation.CachedUlsEventsReplayed);
			newCorrelation.ShouldLogDirectly = existingCorrelation.ShouldLogDirectly;
			newCorrelation.ShouldReplayUls = existingCorrelation.ShouldReplayUls;
			newCorrelation.VisibleId = existingCorrelation.VisibleId;
			newCorrelation.CallDepth = existingCorrelation.CallDepth;
			newCorrelation.UserHash = existingCorrelation.UserHash;
			newCorrelation.EventSequenceNumber = existingCorrelation.EventSequenceNumber;
			newCorrelation.IsFallbackCall = existingCorrelation.IsFallbackCall;
			newCorrelation.TransactionContextId = existingCorrelation.TransactionContextId;
			newCorrelation.TransactionId = existingCorrelation.TransactionId;
			newCorrelation.TransactionStep = existingCorrelation.TransactionStep;

			if (existingCorrelation.HasData)
			{
				bool copiedSuccessfully = SpinWait.SpinUntil(() =>
				{
					bool success = false;
					try
					{
						foreach (object key in existingCorrelation.Keys)
						{
							string keystring = (string)key;
							newCorrelation.AddData(keystring, existingCorrelation.Data(keystring));
						}

						success = true;
					}
					catch (InvalidOperationException)
					{
					}

					return success;
				}, 10);

				if (!copiedSuccessfully)
				{
					// Add a marker to the correlation data indicating it is not complete
					newCorrelation.AddData("Error", "Failed to clone correlation data.");
				}
			}

			return newCorrelation;
		}


		/// <summary>
		/// Creates a new TransactionData object from the current CorrelationData object.
		/// </summary>
		/// <param name="correlationData">The CorrelationData object to copy when creating the TransactionData object.</param>
		/// <returns>A new TransactionData copy of the supplied CorrelationData or null if the current CorrelationData is null.</returns>
		public static TransactionData ToTransactionData(this CorrelationData correlationData)
		{
			if (correlationData == null)
			{
				return null;
			}

			return new TransactionData()
			{
				CallDepth = correlationData.CallDepth,
				CorrelationId = correlationData.VisibleId,
				EventSequenceNumber = correlationData.EventSequenceNumber,
				TransactionContextId = correlationData.TransactionContextId,
				TransactionId = correlationData.TransactionId,
				TransactionStep = correlationData.TransactionStep,
				UserHash = correlationData.UserHash,
				IsFallbackCall = correlationData.IsFallbackCall
			};
		}
	}
}