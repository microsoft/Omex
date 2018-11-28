/***************************************************************************
	Category.cs

	Logging category
***************************************************************************/

using System;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Logging
{
	/// <summary>
	/// Logging category
	/// </summary>
	public class Category
	{
		/// <summary>
		/// Category
		/// </summary>
		/// <param name="name">name</param>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is null or white space</exception>
		public Category(string name)
		{
			Name = Code.ExpectsNotNullOrWhiteSpaceArgument(name, nameof(name), null);
		}


		/// <summary>
		/// Name
		/// </summary>
		public string Name { get; }
	}
}
