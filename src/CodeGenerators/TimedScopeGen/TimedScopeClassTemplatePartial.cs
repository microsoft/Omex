// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Reflection;

namespace Microsoft.Omex.CodeGenerators.TimedScopeGen
{
	/// <summary>
	/// Partial definitions for TimedScopeClassTemplate genarated generator
	/// </summary>
	public partial class TimedScopeClassTemplate
	{
		/// <summary>
		/// Collection of timed scopes to be generated to the C# class
		/// </summary>
		private TimedScopeCollection ScopeCollection { get; }

		/// <summary>
		/// Are we rendering shared timed scopes?
		/// </summary>
		private bool IsSharedTimedScopes { get; }

		/// <summary>
		/// Name of the generated class
		/// </summary>
		private string TimedScopeClassName => IsSharedTimedScopes ? "SharedTimedScopes" : "TimedScopes";

		/// <summary>
		/// Assembly name
		/// </summary>
		private string AssemblyName => Assembly.GetExecutingAssembly().GetName().Name;

		/// <summary>
		/// Assembly version
		/// </summary>
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
		public TimedScopeClassTemplate(TimedScopeCollection scopeCollection, bool isSharedTimedScopes)
		{
			ScopeCollection = scopeCollection;
			IsSharedTimedScopes = isSharedTimedScopes;
		}
	}
}
