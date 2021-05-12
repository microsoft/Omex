// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Text.Json;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration
{
	/// <summary>
	/// Basic Json file generation for a settings model
	/// </summary>
	/// <typeparam name="TSetting">Setting model</typeparam>
	public class JsonGeneration<TSetting> : IFileGenerator<TSetting>
		where TSetting : class
	{
		/// <inheritdoc/>
		public void GenerateFile(TSetting settings, string filename)
		{
			string content = JsonSerializer.Serialize(settings);
			File.WriteAllText(filename, content);
		}
	}
}
