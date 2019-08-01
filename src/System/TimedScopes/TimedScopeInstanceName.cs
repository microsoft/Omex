// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Represents an immutable scope based instance name in a strongly typed fashion
	/// </summary>
	[DataContract]
	[Serializable]
	[Bond.Schema]
	public sealed class TimedScopeInstanceName : IEquatable<TimedScopeInstanceName>
	{
		/// <summary>
		/// Separator used for separating SubType and MetaData
		/// </summary>
		private const char FieldsSeparator = '/';


		/// <summary>
		/// Separator used for separating FailureClassification suffix
		/// </summary>
		private const char FailureClassificationSeparator = '.';


		/// <summary>
		/// Complete strongly typed scope name
		/// </summary>
		[DataMember]
		[Bond.Id(0)]
		public TimedScopeName CompleteScopeName { get; private set; }


		/// <summary>
		/// Failure Classification
		/// </summary>
		[DataMember]
		[Bond.Id(1)]
		public TimedScopeResult Classification { get; private set; }


		/// <summary>
		/// Scope name
		/// </summary>
		public string Scope
		{
			get
			{
				return CompleteScopeName.Scope;
			}
		}


		/// <summary>
		/// SubType
		/// </summary>
		/// <remarks>The value is null if no subtype is specified</remarks>
		public string SubType
		{
			get
			{
				return CompleteScopeName.SubType;
			}
		}


		/// <summary>
		/// MetaData
		/// </summary>
		/// <remarks>The value is null if no metadata is specified</remarks>
		public string MetaData
		{
			get
			{
				return CompleteScopeName.MetaData;
			}
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="scopeName">Strongy typed cope name</param>
		/// <param name="classification">Failure classification</param>
		public TimedScopeInstanceName(TimedScopeName scopeName, TimedScopeResult classification)
		{
			Code.ExpectsArgument(scopeName, nameof(scopeName), TaggingUtilities.ReserveTag(0));

			CompleteScopeName = scopeName;
			Classification = classification;
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="scope">Scope name</param>
		/// <param name="classification">Failure classification</param>
		/// <param name="subType">SubType</param>
		/// <param name="metaData">MetaData</param>
		public TimedScopeInstanceName(string scope, TimedScopeResult classification, string subType = null, string metaData = null)
			: this(new TimedScopeName(scope, subType, metaData), classification)
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
				hash = 23 * hash + CompleteScopeName.GetHashCode();
				hash = 23 * hash + Classification.GetHashCode();
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
			return this == obj as TimedScopeInstanceName;
		}


		/// <summary>
		/// Determines whether the specified instance is equal to this object
		/// </summary>
		/// <param name="other">The other instance to compare with this instance</param>
		/// <returns><c>true</c> if they are equal, <c>false</c> otherwise</returns>
		public bool Equals(TimedScopeInstanceName other)
		{
			return this == other;
		}


		/// <summary>
		/// Equality operator
		/// </summary>
		/// <param name="x">First object</param>
		/// <param name="y">Second object</param>
		/// <returns><c>true</c> if they are equal, <c>false</c> otherwise</returns>
		public static bool operator ==(TimedScopeInstanceName x, TimedScopeInstanceName y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
			{
				return false;
			}

			return x.Classification.Equals(y.Classification)
				&& x.CompleteScopeName.Equals(y.CompleteScopeName);
		}


		/// <summary>
		/// Inequality operator
		/// </summary>
		/// <param name="x">First object</param>
		/// <param name="y">Second object</param>
		/// <returns><c>true</c> if they are not equal, <c>false</c> otherwise</returns>
		public static bool operator !=(TimedScopeInstanceName x, TimedScopeInstanceName y)
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
			builder.Append(FailureClassificationSeparator).Append(Classification);

			if (SubType != null)
			{
				builder.Append(FieldsSeparator).Append(SubType);
			}

			if (MetaData != null)
			{
				builder.Append(FieldsSeparator).Append(MetaData);
			}

			return builder.ToString();
		}


		/// <summary>
		/// Tries to parse a TimedScopeInstanceName instance from a string
		/// </summary>
		/// <param name="toParse">String to be parsed, cannot be null</param>
		/// <param name="parsed">This output parameter is set to a new TimedScopeInstanceName instance when succeeded, null otherwise</param>
		/// <param name="preferMetaData">If <c>true</c>, the second field is assumed to be MetaData value</param>
		/// <returns>Success status</returns>
		public static bool TryParse(string toParse, out TimedScopeInstanceName parsed, bool preferMetaData = false)
		{
			parsed = null;

			if (string.IsNullOrWhiteSpace(toParse))
			{
				return false;
			}

			try
			{
				// The scope instance names have the following form: scope.classification[.description][/subtype][/metadata]
				// We are going to extract the classification substring, the remaining string with the classification and
				// description substring removed should form an ordinary timed scope name so we will parse it by TimedScopeName.TryParse.

				// Split the string by the field separators ('/') first
				string[] parts = toParse.Split(new[] { FieldsSeparator }, StringSplitOptions.None);

				// The first part is further divided by classification separator. It should have two or three fields, the first is
				// a scope, the second is a classification
				string[] firstFieldsubParts = parts[0].Split(new[] { FailureClassificationSeparator }, StringSplitOptions.None);
				if (firstFieldsubParts.Length < 2 || firstFieldsubParts.Length > 3)
				{
					return false;
				}

				string scope = firstFieldsubParts[0];
				string classification = firstFieldsubParts[1];

				// Try to parse the classification substring
				TimedScopeResult parsedClassification;
				if (!Enum.TryParse(classification, out parsedClassification))
				{
					return false;
				}

				// Reconstruct the scope name string without classification and try to parse it
				parts[0] = scope;
				string scopeName = string.Join(FieldsSeparator.ToString(CultureInfo.InvariantCulture), parts);

				TimedScopeName parsedScopeName;
				if (!TimedScopeName.TryParse(scopeName, out parsedScopeName, preferMetaData))
				{
					return false;
				}

				// Create a new instance
				parsed = new TimedScopeInstanceName(parsedScopeName, parsedClassification);
				return true;
			}
			catch (Exception exception)
			{
				// The parsing shouldn't throw but we catch exceptions to be safe
				ULSLogging.ReportExceptionTag(0x23850297 /* tag_97qkx */, Categories.Common, exception,
					"An unexpected exception occured during TimedScopeInstanceName.TryParse. Returning false.");
				return false;
			}
		}


		/// <summary>
		/// Tries to parse a TimedScopeInstanceName instance from a string.
		/// </summary>
		/// <param name="toParse">String to be parsed, cannot be null</param>
		/// <param name="preferMetaData">If <c>true</c>, the second field is assumed to be MetaData value</param>
		/// <returns>New TimedScopeInstanceName instance if succeeded, <c>null</c> otherwise</returns>
		public static TimedScopeInstanceName ParseOrReturnNull(string toParse, bool preferMetaData = false)
		{
			TimedScopeInstanceName name;
			if (TryParse(toParse, out name, preferMetaData))
			{
				return name;
			}
			else
			{
				return null;
			}
		}
	}
}
