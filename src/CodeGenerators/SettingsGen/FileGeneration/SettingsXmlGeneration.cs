// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.CodeGenerators.SettingsGen.Models;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration
{
	/// <summary>
	/// 
	/// </summary>
	public class SettingsXmlGeneration : IFileGenerator<SettingsXmlModel>
	{
		/// <inheritdoc/>
		public string FileType => "XML";

		/// <inheritdoc />
		public void GenerateFile(SettingsXmlModel settings)
		{

		}
	}
}
