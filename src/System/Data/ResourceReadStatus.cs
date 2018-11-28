/***************************************************************************
	ResourceReadStatus.cs

	Enumeration for resource reading status
***************************************************************************/

namespace Microsoft.Omex.System.Data
{
	/// <summary>
	/// Enumeration for resource reading status
	/// </summary>
	public enum ResourceReadStatus
	{
		/// <summary>
		/// Unknown / not yet defined
		/// </summary>
		Unknown,


		/// <summary>
		/// Successful
		/// </summary>
		Success,


		/// <summary>
		/// Resource could not be found
		/// </summary>
		NotFound,


		/// <summary>
		/// Resource data could not be read
		/// </summary>
		ReadFailed
	}
}
