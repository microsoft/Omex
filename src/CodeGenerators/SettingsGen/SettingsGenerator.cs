// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models;
using Microsoft.Omex.CodeGenerators.SettingsGen.Parser;
using Microsoft.Omex.CodeGenerators.SettingsGen.Validation;

namespace Microsoft.Omex.CodeGenerators.SettingsGen
{
	/// <summary>
	/// Settings generator for settings used in service fabric settings.xml file.
	/// </summary>
	[Generator]
	public class SettingsGenerator : BaseGenerator<SettingsXmlModel>
	{
		/// <summary>
		/// Parser which parses syntax into a given model format to be used by file generator
		/// </summary>
		protected override IParser<SettingsXmlModel>? Parser => new ClassesWithAttributeFinder(DefaultAttributes);

		/// <summary>
		/// FileGenerator which takes a given model and generates a file from it
		/// </summary>
		protected override IFileGenerator<SettingsXmlModel>? Filegenerator => new XmlFileGeneration<SettingsXmlModel>();

		/// <inheritdoc />
		protected override IValidation<SettingsXmlModel>? Validator => new XmlValidation<SettingsXmlModel>();

		/// <inheritdoc/>
		protected override bool ShouldGenerateFile(GeneratorExecutionContext context, out AdditionalText? settingsFile)
		{
			try
			{
				foreach (AdditionalText file in context.AdditionalFiles)
				{
					if (string.Equals(Path.GetFileName(file.Path), Filename, StringComparison.OrdinalIgnoreCase))
					{
						settingsFile = file;

						return ShouldGen(context, file);
					}
				}
				settingsFile = null;
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Encountered exception {ex.Message} when getting settings xml");
				throw;
			}
		}

		private bool ShouldGen(GeneratorExecutionContext context, AdditionalText filename)
		{
			AnalyzerConfigOptions options = context.AnalyzerConfigOptions.GetOptions(filename);
			if (options.TryGetValue("build_metadata.additionalfiles.Generate", out string? shouldGenerate))
			{
				return bool.TryParse(shouldGenerate, out bool result) && result;
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		public static ISet<string> DefaultAttributes => new HashSet<string>
		{
			"SectionAttribute",
		};

		/// <summary>
		/// 
		/// </summary>
		public static readonly string Filename = "Settings.xml";
	}
}
