// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.Logging
{
	/// <summary>
	/// Logging level
	/// </summary>
	/// <remarks>Implemented as struct to ensure immutable</remarks>
	public struct Level
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="level">level</param>
		internal Level(Levels.LogLevel level)
		{
			LogLevel = level;
		}


		/// <summary>
		/// Level value
		/// </summary>
		public Levels.LogLevel LogLevel { get; }


		/// <summary>
		/// Override equals
		/// </summary>
		/// <param name="obj">object</param>
		/// <returns>true if equal, false otherwise</returns>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Level))
			{
				return false;
			}

			Level level = (Level)obj;
			return level.LogLevel == LogLevel;
		}


		/// <summary>
		/// Get hash code
		/// </summary>
		/// <returns>hash code</returns>
		public override int GetHashCode()
		{
			return LogLevel.GetHashCode();
		}


		/// <summary>
		/// Override == operator
		/// </summary>
		/// <param name="l1">first level</param>
		/// <param name="l2">second level</param>
		/// <returns>true if equal, false otherwise</returns>
		public static bool operator ==(Level l1, Level l2)
		{
			return l1.Equals(l2);
		}


		/// <summary>
		/// Override != operator
		/// </summary>
		/// <param name="l1">first level</param>
		/// <param name="l2">second level</param>
		/// <returns>false if equal, true otherwise</returns>
		public static bool operator !=(Level l1, Level l2)
		{
			return !l1.Equals(l2);
		}


		/// <summary>
		/// String representaion of the level
		/// </summary>
		/// <returns>string representation</returns>
		public override string ToString()
		{
			return LogLevel.ToString();
		}
	}
}