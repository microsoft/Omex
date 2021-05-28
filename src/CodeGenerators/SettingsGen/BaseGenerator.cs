// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.Omex.CodeGenerators.SettingsGen.Comparing;
using Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration;
using Microsoft.Omex.CodeGenerators.SettingsGen.Parser;

namespace Microsoft.Omex.CodeGenerators.SettingsGen
{
	/// <summary>
	/// The base generator which can be inherited in order to implement your own version
	/// </summary>
	/// <typeparam name="TSettingModel">Settings model type</typeparam>
	public abstract class BaseGenerator<TSettingModel> : ISourceGenerator
		where TSettingModel : class, IEquatable<TSettingModel>
	{
		/// <inheritdoc />
		public void Execute(GeneratorExecutionContext context)
		{
			if (context.SyntaxContextReceiver is not IParser<TSettingModel> parser)
			{
				return;
			}

			try
			{
				TSettingModel settings = parser.GetSettings();
				bool shouldGen = ShouldGenerateFile(context, out AdditionalText? settingsFile);

				if (!shouldGen)
				{
					context.ReportDiagnostic(Diagnostic.Create(DontGen, null));
					return;
				}

				if (settingsFile == null)
				{
					throw new Exception("No file to compare given");
				}

				if (Comparer?.AreExistingSettingsEqual(settings, settingsFile) == true)
				{
					context.ReportDiagnostic(Diagnostic.Create(MatchingSettings, null));
					return;
				}

				if (FileGenerator is null)
				{
					throw new Exception($"No file generator set for class {GetType().Name}");
				}

				context.ReportDiagnostic(Diagnostic.Create(WritingToFile, null, settingsFile.Path));

				FileGenerator.GenerateFile(settings, settingsFile.Path);
			}
			catch (Exception ex)
			{
				context.ReportDiagnostic(Diagnostic.Create(FailedGeneration, null, ex.Message));
			}
		}

		/// <inheritdoc />
		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(() => Parser);
		}

		/// <summary>
		/// Parser which parses syntax into a given model format to be used by file generator
		/// </summary>
		protected virtual IParser<TSettingModel>? Parser => null;

		/// <summary>
		/// FileGenerator which takes a given model and generates a file from it
		/// </summary>
		protected virtual IFileGenerator<TSettingModel>? FileGenerator => null;

		/// <summary>
		/// Comparer determines whether or not the new and existing settings match
		/// </summary>
		protected virtual IComparison<TSettingModel>? Comparer => null;

		/// <summary>
		/// Whether or not to generate file
		/// </summary>
		/// <param name="context">Execution context</param>
		/// <param name="settingsFile">File containing existing settings</param>
		/// <returns>Whether or not the settings file should be generated </returns>
		protected virtual bool ShouldGenerateFile(GeneratorExecutionContext context, out AdditionalText? settingsFile)
		{
			settingsFile = null;
			return false;
		}

		/// <summary>
		/// Writing to a file
		/// </summary>
		protected static readonly DiagnosticDescriptor WritingToFile = new(id: "SETTINGSGEN001",
			title: "Writing to settings file",
			messageFormat: "Writing to file '{0}'.",
			category: "SettingsGenerator",
			DiagnosticSeverity.Info,
			isEnabledByDefault: true);

		/// <summary>
		/// Don't generate
		/// </summary>
		protected static readonly DiagnosticDescriptor DontGen = new(id: "SETTINGSGEN002",
			title: "Not generating",
			messageFormat: "Not generating/updating settings file.",
			category: "SettingsGenerator",
			DiagnosticSeverity.Info,
			isEnabledByDefault: true);

		/// <summary>
		/// Matching settings
		/// </summary>
		protected static readonly DiagnosticDescriptor MatchingSettings = new(id: "SETTINGSGEN003",
			title: "Existing settings match",
			messageFormat: "No new settings or updated settings so don't need to generate",
			category: "SettingsGenerator",
			DiagnosticSeverity.Info,
			isEnabledByDefault: true);

		/// <summary>
		/// Failed to generate
		/// </summary>
		protected static readonly DiagnosticDescriptor FailedGeneration = new(id: "SETTINGSGEN004",
			title: "Failed with exception",
			messageFormat: "Failed to write settings generator with error {0}",
			category: "SettingsGenerator",
			DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		/// <summary>
		/// Encoutered error in settings gen
		/// </summary>
		protected static readonly DiagnosticDescriptor EncounteredError = new(id: "SETTINGSGEN005",
			title: "Error encountered",
			messageFormat: "Encountered error {0} when running settings gen",
			category: "SettingsGenerator",
			DiagnosticSeverity.Error,
			isEnabledByDefault: true);
	}
}
