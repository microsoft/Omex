// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Omex.CodeGenerators.SettingsGen.Comparing;
using Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes;
using Microsoft.Omex.CodeGenerators.SettingsGen.Parser;
using Microsoft.Omex.CodeGenerators.SettingsGen.Wrappers;

namespace Microsoft.Omex.CodeGenerators.SettingsGen
{
	/// <summary>
	/// Settings generator for settings used in service fabric settings.xml file.
	/// </summary>
	[Generator]
	public class SettingsGenerator : BaseGenerator<SettingsXmlModel>
	{
		/// <inheritdoc />
		protected override IParser<SettingsXmlModel>? Parser => new SettingsFromAttributeParser(DefaultAttributes, new GeneratorSyntaxContextWrapper());

		/// <inheritdoc />
		protected override IFileGenerator<SettingsXmlModel>? Filegenerator => new XmlFileGeneration<SettingsXmlModel>();

		/// <inheritdoc />
		protected override IComparison<SettingsXmlModel>? Comparer => new XmlComparison<SettingsXmlModel>();

		/// <inheritdoc/>
		protected override bool ShouldGenerateFile(GeneratorExecutionContext context, out AdditionalText? settingsFile)
		{
			try
			{
				// Load settings from file provided via property in csproj
				if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GenerateSettingsFile", out string? value) &&
					bool.TryParse(value ?? string.Empty, out bool shouldGen))
				{
					if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.SettingsFile", out value))
					{
						settingsFile = new SettingsAdditionalText(value);
					}
					else
					{
						settingsFile = null;
					}
					return shouldGen;

				}

				// Load settings from additional files
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
				context.ReportDiagnostic(Diagnostic.Create(EncounteredError, null, ex.Message));
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
		/// Default attributes to look for.
		/// </summary>
		public static ISet<string> DefaultAttributes => new HashSet<string>
		{
			AttributeNames.Section,
		};

		/// <summary>
		/// Default filename the settings are in.
		/// </summary>
		public static readonly string Filename = "Settings.xml";
	}
}
