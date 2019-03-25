// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#region Using Directives

using System;
using System.Runtime.Serialization;
using Microsoft.Omex.System.Logging;

#endregion

namespace Microsoft.Omex.System.Model.Types
{
	/// <summary>
	/// Product code
	/// </summary>
	[DataContract]
	public class ProductCode : IEquatable<ProductCode>
	{
		/// <summary>
		/// Gets the application portion of the Product Code, e.g. Word
		/// </summary>
		public string Application { get; private set; }


		/// <summary>
		/// Gets the platform portion of the Product Code, e.g. Win32
		/// </summary>
		public string Platform { get; private set; }


		/// <summary>
		/// The Product code.
		/// </summary>
		[DataMember]
		protected string m_code;


		/// <summary>
		/// A protected constructor. Callers should use TryParse() on derived classes instead.
		/// </summary>
		/// <param name="code">a product code</param>
		public ProductCode(string code)
		{
			m_code = code ?? string.Empty;

			if (!string.IsNullOrWhiteSpace(code))
			{
				string[] parts = code.Split(new char[] { '_' });

				if (parts.Length == 2)
				{
					Platform = parts[0];
					Application = parts[1];
				}
			}
		}


		/// <summary>
		/// Return the product code.
		/// </summary>
		/// <returns>product code</returns>
		public override string ToString() => m_code;


		/// <summary>
		/// Checks if the current object equals the specified other.
		/// </summary>
		/// <param name="other">the other object</param>
		/// <returns>true if the current object is equal to the other parameter; false otherwise</returns>
		public bool Equals(ProductCode other)
		{
			if ((object)other == null)
			{
				return false;
			}

			return m_code.Equals(other.m_code, StringComparison.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Determines whether the specified object is equal to this instance.
		/// </summary>
		/// <param name="obj">the object to compare with this instance</param>
		/// <returns>true if the specified object is equal to this instance; false otherwise</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (!(obj is ProductCode other))
			{
				return false;
			}

			return m_code.Equals(other.m_code, StringComparison.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Returns a hash code for this instance. To be consistent with the overriden Equals method,
		/// calculates hash value in a case insensitive way.
		/// </summary>
		/// <returns>a hash code for this instance</returns>
		public override int GetHashCode() => m_code?.ToLower().GetHashCode() ?? 0;


		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>true if the first product code is equal to the second product code; false otherwise</returns>
		public static bool operator ==(ProductCode code1, ProductCode code2)
		{
			if (ReferenceEquals(code1, code2))
			{
				return true;
			}

			if ((object)code1 == null || (object)code2 == null)
			{
				return false;
			}

			return code1.m_code.Equals(code2.m_code, StringComparison.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>true if the first product code is not equal to the second product code; false otherwise</returns>
		public static bool operator !=(ProductCode code1, ProductCode code2)
		{
			return !(code1 == code2);
		}


		/// <summary>
		/// An implicit convertion from ProductCode to string.
		/// </summary>
		/// <param name="code">the ProductCode to convert</param>
		/// <returns>the product code from the object being converted</returns>
		public static implicit operator string(ProductCode code)
		{
			if (code == null)
			{
				ULSLogging.LogTraceTag(0x23850284 /* tag_97qke */, Categories.Common, Levels.Verbose, "Cannot convert null product code to string.");
				return null;
			}

			return code.ToString();
		}
	}
}