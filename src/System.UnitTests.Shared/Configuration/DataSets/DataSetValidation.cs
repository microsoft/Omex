// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#region Using directives

using System.Collections.Generic;
using System.Text;
using Microsoft.Omex.System.Configuration.DataSets;
using Microsoft.Omex.System.Data;

#endregion

namespace Microsoft.Omex.System.UnitTests.Shared.Configuration.DataSets
{
	/// <summary>
	/// Helpers for validating datasets
	/// </summary>
	public static class DataSetValidation
	{
		/// <summary>
		/// Verifies that DataSet loads successfully
		/// </summary>
		/// <typeparam name="T">Type of dataset to load</typeparam>
		/// <param name="resources">Resources to load the dataset from</param>
		/// <param name="errors">Errors found in loading</param>
		/// <returns>Loaded data set</returns>
		public static T LoadDataSetWithNoErrors<T>(IDictionary<string, IResourceDetails> resources, out string errors)
			where T : class, IConfigurationDataSet, new()
		{
			StringBuilder loadingErrors = new StringBuilder();
			T dataSet = new T();

			// Expecting DataSet load to return true
			if (!dataSet.Load(resources))
			{
				loadingErrors.AppendLine("Expecting DataSet load to return true.");
			}

			// Expecting DataSet to be loaded
			if (!dataSet.IsHealthy)
			{
				loadingErrors.AppendFormat("Expecting DataSet to be healthy, but loaded with errors: '{0}'", GetDataSetLoadingErrors(dataSet.Errors));
			}

			errors = loadingErrors.ToString();
			return dataSet;
		}


		/// <summary>
		/// Verifies that DataSet loads with one parsing error from given resources
		/// </summary>
		/// <typeparam name="T">Type of dataset</typeparam>
		/// <param name="resources">resources to load from</param>
		/// <param name="errorSubstrings">Substrings that should be in the error messages</param>
		/// <param name="errors">Errors builder</param>
		/// <returns>Loaded data set</returns>
		public static T LoadDataSetWithErrors<T>(IDictionary<string, IResourceDetails> resources, string[] errorSubstrings, StringBuilder errors)
			where T : class, IConfigurationDataSet, new()
		{
			T dataSet = new T();

			// Expecting DataSet load to return false
			if (dataSet.Load(resources))
			{
				errors.AppendLine("Expecting DataSet load to return false.");
			}

			// Expecting DataSet to be loaded
			if (dataSet.IsHealthy)
			{
				errors.AppendLine("Expecting DataSet to be not healthy.");
			}

			// Expecting number of errors
			if (dataSet.Errors.Count != errorSubstrings.Length)
			{
				errors.AppendLine(GetDataSetLoadingErrors(dataSet.Errors));
			}
			else
			{
				for (int errorId = 0; errorId < errorSubstrings.Length; errorId++)
				{
					// Expecting certain text in the error
					if (!dataSet.Errors[errorId].Contains(errorSubstrings[errorId]))
					{
						errors.AppendFormat("Error {0} expected to contain '{1}' instead contains '{2}'",
							errorId, errorSubstrings[errorId], dataSet.Errors[errorId]);
					}
				}
			}

			return dataSet;
		}


		/// <summary>
		/// Merges all load errors into one string for easier unit test output
		/// </summary>
		/// <param name="errors">The list of errors from the test.</param>
		/// <returns>String with all loading errors</returns>
		public static string GetDataSetLoadingErrors(IList<string> errors)
		{
			if (errors == null || errors.Count == 0)
			{
				return "No errors reported.";
			}

			StringBuilder stringBuilder = new StringBuilder(500);
			foreach (string error in errors)
			{
				stringBuilder.AppendLine(error);
			}

			return stringBuilder.ToString();
		}
	}
}