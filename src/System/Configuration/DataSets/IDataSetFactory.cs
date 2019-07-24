// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.Configuration.DataSets
{
	/// <summary>
	/// Data set factory interface
	/// </summary>
	/// <typeparam name="T">Type of the data set to create</typeparam>
	public interface IDataSetFactory<T>
	{
		T Create();
	}
}
