/***************************************************************************
	TimedScopeClassTemplatePartial.cs

	Owner: matoma
	Copyright (c) Microsoft Corporation

	Partial definitions for TimedScopeClassTemplate genarated generator
***************************************************************************/

using System.Diagnostics;
using System.Reflection;

namespace Microsoft.Office.Web.OfficeMarketplace.TimedScopeGen
{
	/// <summary>
	/// Partial definitions for TimedScopeClassTemplate genarated generator
	/// </summary>
	/// <owner alias="matoma"/>
	public partial class TimedScopeClassTemplate
	{
		/// <summary>
		/// Collection of timed scopes to be generated to the C# class
		/// </summary>
		/// <owner alias="matoma"/>
		private TimedScopeCollection ScopeCollection { get; }


		/// <summary>
		/// Are we rendering shared timed scopes?
		/// </summary>
		/// <owner alias="matoma"/>
		private bool IsSharedTimedScopes { get; }


		/// <summary>
		/// Name of the generated class
		/// </summary>
		/// <owner alias="matoma"/>
		private string TimedScopeClassName => IsSharedTimedScopes ? "SharedTimedScopes" : "TimedScopes";


		/// <summary>
		/// Assembly name
		/// </summary>
		/// <owner alias="matoma"/>
		private string AssemblyName => Assembly.GetExecutingAssembly().GetName().Name;


		/// <summary>
		/// Assembly version
		/// </summary>
		/// <owner alias="matoma"/>
		private string AssemblyVersion
		{
			get
			{
				FileVersionInfo version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
				return string.Concat(version.FileMajorPart, ".", version.FileMinorPart, ".0000.0000");
			}
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="scopeCollection">Scope collection</param>
		/// <param name="isSharedTimedScopes">Are we rendering shared timed scopes?</param>
		/// <owner alias="matoma"/>
		public TimedScopeClassTemplate(TimedScopeCollection scopeCollection, bool isSharedTimedScopes)
		{
			ScopeCollection = scopeCollection;
			IsSharedTimedScopes = isSharedTimedScopes;
		}
	}
}