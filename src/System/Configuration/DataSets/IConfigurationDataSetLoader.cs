// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Omex.System.Data;

namespace Microsoft.Omex.System.Configuration.DataSets
{
	/// <summary>
	/// Interface for configuration DataSet loader
	/// </summary>
	/// <typeparam name="T">Type of DataSet</typeparam>
	public interface IConfigurationDataSetLoader<out T> : IDisposable
		where T : IConfigurationDataSet
	{
		/// <summary>
		/// The event raised on dataset load.
		/// </summary>
		event EventHandler DataSetLoaded;


		/// <summary>
		/// Resources loaded into the DataSet
		/// </summary>
		IEnumerable<IResource> Resources { get; }


		/// <summary>
		/// Loaded DataSet instance
		/// </summary>
		T LoadedDataSet { get; }


		/// <summary>
		/// DataSet instance corresponding to last unsuccessful loading
		/// </summary>
		T FailedToLoadDataSet { get; }
	}
}