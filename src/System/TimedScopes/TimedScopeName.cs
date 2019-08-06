// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Bond.Tag;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Represents Scope name, subtype and metada values in a strongly typed and immutable fashion
	/// </summary>
	[DataContract]
	[Serializable]
	[Bond.Schema]
	public sealed class TimedScopeName : IEquatable<TimedScopeName>
	{
		/// <summary>
		/// Separator used in the string representation
		/// </summary>
		private const char Separator = '/';


		/// <summary>
		/// Scope name
		/// </summary>
		[DataMember]
		[Bond.Id(0)]
		public string Scope { get; private set; }


		/// <summary>
		/// SubType
		/// </summary>
		/// <remarks>The value is null if no subtype is specified. Empty value is allowed.</remarks>
		[DataMember]
		[Bond.Id(1), Bond.Type(typeof(nullable<wstring>))]
		public string SubType { get; private set; }


		/// <summary>
		/// MetaData
		/// </summary>
		/// <remarks>The value is null if no metadata is specified. Empty value is allowed.</remarks>
		[DataMember]
		[Bond.Id(2), Bond.Type(typeof(nullable<wstring>))]
		public string MetaData { get; private set; }


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="scope">Scope name</param>
		/// <param name="subType">SubType</param>
		/// <param name="metaData">MetaData</param>
		public TimedScopeName(string scope, string subType = null, string metaData = null)
		{
			Code.ExpectsNotNullOrWhiteSpaceArgument(scope, nameof(scope), TaggingUtilities.ReserveTag(0));

			Scope = scope;
			SubType = subType;
			MetaData = metaData;
		}


		/// <summary>
		/// Constructor that copies all fields (except the classification) from a TimedScopeInstanceName.
		/// </summary>
		/// <param name="copyFrom">Object to copy values from</param>
		public TimedScopeName(TimedScopeInstanceName copyFrom)
		{
			Code.ExpectsArgument(copyFrom, nameof(copyFrom), TaggingUtilities.ReserveTag(0));

			Scope = copyFrom.Scope;
			SubType = copyFrom.SubType;
			MetaData = copyFrom.MetaData;
		}


		/// <summary>
		/// Parameterless constructor for Bond serialization
		/// </summary>
		public TimedScopeName()
		{
		}


		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = 23 * hash + Scope.GetHashCode();
				hash = 23 * hash + (SubType == null ? 0 : SubType.GetHashCode());
				hash = 23 * hash + (MetaData == null ? 0 : MetaData.GetHashCode());
				return hash;
			}
		}


		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// true if the specified object  is equal to the current object; otherwise, false.
		/// </returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object obj)
		{
			return this == obj as TimedScopeName;
		}


		/// <summary>
		/// Determines whether the specified TimedScopeName is equal to this object
		/// </summary>
		/// <param name="other">The other UserScopeEvent instance to compare with this instance</param>
		/// <returns><c>true</c> if they are equal, <c>false</c> otherwise</returns>
		public bool Equals(TimedScopeName other)
		{
			return this == other;
		}


		/// <summary>
		/// Equality operator
		/// </summary>
		/// <param name="x">First object</param>
		/// <param name="y">Second object</param>
		/// <returns><c>true</c> if they are equal, <c>false</c> otherwise</returns>
		public static bool operator ==(TimedScopeName x, TimedScopeName y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
			{
				return false;
			}

			return string.Equals(x.Scope, y.Scope, StringComparison.Ordinal)
				&& string.Equals(x.SubType, y.SubType, StringComparison.Ordinal)
				&& string.Equals(x.MetaData, y.MetaData, StringComparison.Ordinal);
		}


		/// <summary>
		/// Inequality operator
		/// </summary>
		/// <param name="x">First object</param>
		/// <param name="y">Second object</param>
		/// <returns><c>true</c> if they are not equal, <c>false</c> otherwise</returns>
		public static bool operator !=(TimedScopeName x, TimedScopeName y)
		{
			return !(x == y);
		}


		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder(Scope);

			if (SubType != null)
			{
				builder.Append(Separator).Append(SubType);
			}

			if (MetaData != null)
			{
				builder.Append(Separator).Append(MetaData);
			}

			return builder.ToString();
		}


		/// <summary>
		/// Creates an instance name with provided scope result
		/// </summary>
		/// <param name="result">Scope result</param>
		/// <returns>Returns instance name</returns>
		public TimedScopeInstanceName CreateInstanceName(TimedScopeResult result)
		{
			return new TimedScopeInstanceName(this, result);
		}


		/// <summary>
		/// All instances (all timed scope results) of given timed scope name
		/// </summary>
		public IEnumerable<TimedScopeInstanceName> AllInstances
		{
			get
			{
				foreach (TimedScopeResult result in Enum.GetValues(typeof(TimedScopeResult)))
				{
					yield return CreateInstanceName(result);
				}
			}
		}


		/// <summary>
		/// True if it has a subtype
		/// </summary>
		public bool HasSubType => SubType != null;


		/// <summary>
		/// True if it has a metadata
		/// </summary>
		public bool HasMetaData => MetaData != null;


		/// <summary>
		/// Tries to parse a TimedScopeName instance from a string
		/// </summary>
		/// <param name="toParse">String to be parsed, cannot be null</param>
		/// <param name="parsed">This output parameter is set to a new TimedScopeName instance when succeeded, null otherwise</param>
		/// <param name="preferMetaData">If <c>true</c>, the second field is assumed to be MetaData value</param>
		/// <returns>Success status</returns>
		public static bool TryParse(string toParse, out TimedScopeName parsed, bool preferMetaData = false)
		{
			parsed = null;

			if (string.IsNullOrWhiteSpace(toParse))
			{
				return false;
			}

			try
			{
				// The scope name should have the following form: scope[/subtype][/metadata]
				// We just split the string, check the number of fields and assign correct fields

				string[] parts = toParse.Split(new[] { Separator }, StringSplitOptions.None);

				string scope = parts[0];
				string subType = null;
				string metaData = null;

				if (string.IsNullOrWhiteSpace(scope))
				{
					return false;
				}

				if (parts.Length == 2)
				{
					if (preferMetaData)
					{
						metaData = parts[1];
					}
					else
					{
						subType = parts[1];
					}
				}
				else if (parts.Length == 3)
				{
					subType = parts[1];
					metaData = parts[2];
				}
				else if (parts.Length > 3)
				{
					return false;
				}

				parsed = new TimedScopeName(scope, subType, metaData);
				return true;
			}
			catch (Exception exception)
			{
				// The parsing shouldn't throw but we catch exceptions to be safe
				ULSLogging.ReportExceptionTag(0, Categories.Common, exception,
					"An unexpected exception occured during TimedScopeName.TryParse. Returning false.");
				return false;
			}
		}
	}
}
