// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration
{
	/// <summary>
	/// Interface for generating file for a given settings model
	/// </summary>
	/// <typeparam name="TSettings">Model representing the settings</typeparam>
	public interface IFileGenerator<TSettings>
		where TSettings : class 
	{
		/// <summary>
		/// Generate the file with settings in it
		/// </summary>
		/// <param name="settings">Settings to write to a file</param>
		/// <param name="filename">Name of the file to write to</param>
		void GenerateFile(TSettings settings, string filename);

		/// <summary>
		/// File type that is being written
		/// </summary>
		string FileType { get; }
	}
}
