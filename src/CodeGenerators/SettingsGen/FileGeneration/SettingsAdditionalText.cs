// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration
{
	/// <summary>
	/// Additional text for settings
	/// </summary>
	internal sealed class SettingsAdditionalText : AdditionalText
	{
		private readonly string m_path;

		private readonly SourceText? m_sourceText;

		/// <summary>
		/// Constructor
		/// </summary>
		public SettingsAdditionalText(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Empty filename given");
			}

			m_path = path;

			string fileContent = File.ReadAllText(m_path);
			if (!string.IsNullOrWhiteSpace(fileContent))
			{
				m_sourceText = SourceText.From(fileContent);
			}
		}

		public override string Path => m_path;

		public override SourceText? GetText(CancellationToken cancellationToken = default) => m_sourceText;
	}
}
