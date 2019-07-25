/***************************************************************************
	TimedScopesPartial.cs

	Owner: matoma
	Copyright (c) Microsoft Corporation

	Partial definitions added to the generated TimedScopes.cs file
***************************************************************************/

namespace Microsoft.Office.Web.OfficeMarketplace.TimedScopeGen
{
	/// <summary>
	/// Partal definitions for the TimedScope class
	/// </summary>
	/// <owner alias="matoma"/>
	public partial class TimedScope
	{
		/// <summary>
		/// Gets the description
		/// </summary>
		/// <returns>Description</returns>
		/// <owner alias="matoma"/>
		public string GetDescription() => Description ?? string.Join(string.Empty, Text);
	}
}