/***************************************************************************
	GatedServiceTypes.cs

	Enumeration of flags that are used for gating services.
***************************************************************************/

using System;

namespace Microsoft.Omex.Gating
{
	/// <summary>
	/// Enumeration of flags that are used for gating services.
	/// </summary>
	[Flags]
	public enum GatedServiceTypes
	{
		/// <summary>
		/// No service.
		/// </summary>
		None = 0x0,


		/// <summary>
		/// Canary service.
		/// </summary>
		CanaryService = 0x1,


		/// <summary>
		/// Full service.
		/// </summary>
		FullService = 0x2,


		/// <summary>
		/// Both full and canary service.
		/// </summary>
		All = CanaryService | FullService,
	}
}
