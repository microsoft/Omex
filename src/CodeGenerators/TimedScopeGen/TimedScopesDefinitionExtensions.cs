/*****************************************************************************
	TimedScopesDefinitionExtensions.cs

	Owner: matoma
	Copyright (c) Microsoft Corporation.

	Contains extension methods for timed scope definitions
******************************************************************************/

using System;
using System.Text;

namespace Microsoft.Office.Web.OfficeMarketplace.TimedScopeGen
{
	/// <summary>
	/// Contains extension methods for converting timed scope definitions to perf counters
	/// </summary>
	/// <owner alias="matoma"/>
	public static class TimedScopesDefinitionExtensions
	{
		/// <summary>
		/// Strips TimedScopeArea name
		/// </summary>
		/// <param name="timedScopeArea"></param>
		/// <returns>Striped name</returns>
		/// <owner alias="matoma"/>
		public static string StrippedName(this TimedScopeArea timedScopeArea)
		{
			if (timedScopeArea.name.StartsWith(Program.ProductOmex, StringComparison.OrdinalIgnoreCase))
			{
				return timedScopeArea.name.Substring(Program.ProductOmex.Length);
			}

			return timedScopeArea.name;
		}


		/// <summary>
		/// Formats timed scope description by dividing it into several lines.
		/// </summary>
		/// <param name="scope">Scope</param>
		/// <param name="maxCharactersPerLine">the maximal number of characters allowed per line</param>
		/// <param name="indentationLevel">the indentation level used for new lines</param>
		/// <returns>Description text devided into several lines</returns>
		/// <owner alias="hubgezi">Hubert Gezikiewicz</owner>
		public static string FormatDescription(this TimedScope scope, int maxCharactersPerLine = 140, int indentationLevel = 5)
		{
			string description = scope.GetDescription() ?? "MissingDescription";
			StringBuilder result = new StringBuilder(description.Length + 50);
			int startIndex = 0;

			do
			{
				if (startIndex + maxCharactersPerLine < description.Length)
				{
					result.Append(description.Substring(startIndex, maxCharactersPerLine));
					result.AppendLine("\" +");
					for (int i = 0; i < indentationLevel; i++)
					{
						result.Append('\t');
					}

					result.Append("\"");
				}
				else
				{
					result.Append(description.Substring(startIndex, description.Length - startIndex));
					break;
				}

				startIndex += maxCharactersPerLine;
			}
			while (startIndex < description.Length);

			return result.ToString();
		}
	}
}