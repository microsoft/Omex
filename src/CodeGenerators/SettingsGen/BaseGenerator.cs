// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration;
using Microsoft.Omex.CodeGenerators.SettingsGen.Parser;
using Microsoft.Omex.CodeGenerators.SettingsGen.Validation;

namespace Microsoft.Omex.CodeGenerators.SettingsGen
{
	/// <summary>
	/// The base generator which can be inherited in order to implement your own version
	/// </summary>
	/// <typeparam name="TSettingModel">Settings model type</typeparam>
	public abstract class BaseGenerator<TSettingModel> : ISourceGenerator
		where TSettingModel : class, IEqualityComparer<TSettingModel>, IEquatable<TSettingModel>
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
					context.ReportDiagnostic(Diagnostic.Create(s_dontGen, null));
					return;
				}

				if (settingsFile == null)
				{
					throw new Exception("No file to compare or generate given");
				}

				if (Validator?.AreExistingSettingsEqual(settings, settingsFile) == true)
				{
					context.ReportDiagnostic(Diagnostic.Create(s_matchingSettings, null));
					return;
				}

				if (Filegenerator is null)
				{
					throw new Exception($"No file generator set for class {GetType().Name}");
				}

				context.ReportDiagnostic(Diagnostic.Create(s_writingToFile, null, settingsFile.Path));

				Filegenerator.GenerateFile(settings, settingsFile.Path);
			}
			catch (Exception ex)
			{
				context.ReportDiagnostic(Diagnostic.Create(s_failedGeneration, null, ex.Message));
				throw;
			}
		}

		/// <inheritdoc />
		public void Initialize(GeneratorInitializationContext context)
		{
			if (Parser != null)
			{
				context.RegisterForSyntaxNotifications(() => Parser);
			}
		}

		/// <summary>
		/// Parser which parses syntax into a given model format to be used by file generator
		/// </summary>
		protected virtual IParser<TSettingModel>? Parser => null;

		/// <summary>
		/// FileGenerator which takes a given model and generates a file from it
		/// </summary>
		protected virtual IFileGenerator<TSettingModel>? Filegenerator => null;

		/// <summary>
		///	Validator determines whether or not the new and existing settings match
		/// </summary>
		protected virtual IValidation<TSettingModel>? Validator => null;

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

		private static readonly DiagnosticDescriptor s_writingToFile = new DiagnosticDescriptor(id: "SETTINGSGEN001",
			title: "Writing to settings file",
			messageFormat: "Writing to file '{0}'.",
			category: "SettingsGenerator",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		private static readonly DiagnosticDescriptor s_dontGen = new DiagnosticDescriptor(id: "SETTINGSGEN002",
			title: "Not generating",
			messageFormat: "Not generating/updating settings file.",
			category: "SettingsGenerator",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		private static readonly DiagnosticDescriptor s_matchingSettings = new DiagnosticDescriptor(id: "SETTINGSGEN003",
			title: "Existing settings match",
			messageFormat: "No new settings or updated settings so don't need to generate",
			category: "SettingsGenerator",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		private static readonly DiagnosticDescriptor s_failedGeneration = new DiagnosticDescriptor(id: "SETTINGSGEN004",
			title: "Failed with exception",
			messageFormat: "Failed to write settings generator with error {0}",
			category: "SettingsGenerator",
			DiagnosticSeverity.Error,
			isEnabledByDefault: true);
	}
}
