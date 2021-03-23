// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="U"></typeparam>
	public interface IFileGenerator<U>
		where U : class 
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="settings"></param>
		void GenerateFile(U settings);
		
		/// <summary>
		/// 
		/// </summary>
		string FileType { get; }
	}
}
