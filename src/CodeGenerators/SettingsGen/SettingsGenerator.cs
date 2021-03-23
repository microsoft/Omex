using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration;
using Microsoft.Omex.CodeGenerators.SettingsGen.Models;
using Microsoft.Omex.CodeGenerators.SettingsGen.Parser;

namespace Microsoft.Omex.CodeGenerators.SettingsGen
{
	/// <summary>
	/// 
	/// </summary>
	public class SettingsGenerator : ISourceGenerator
	{

		/// <inheritdoc />
		public void Execute(GeneratorExecutionContext context)
		{
			if (context.SyntaxContextReceiver is not IParser<SettingsXmlModel> parser)
			{
				return;
			}

			SettingsXmlModel settings = parser.GetSettings();

			if (Filegenerator is null)
			{
				throw new Exception($"No file generator set for class {nameof(SettingsGenerator)}");
			}

			Filegenerator.GenerateFile(settings);
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
		/// 
		/// </summary>
		protected virtual ISyntaxContextReceiver? SyntaxcontextReceiver => new ClassesWithAttributeFinder(DefaultAttributes);

		/// <summary>
		/// Parser which parses syntax into a given model format to be used by file generator
		/// </summary>
		protected virtual IParser<SettingsXmlModel>? Parser => null;

		/// <summary>
		/// FileGenerator which takes a given model and generates a file from it
		/// </summary>
		protected virtual IFileGenerator<SettingsXmlModel>? Filegenerator => null;

		/// <summary>
		/// 
		/// </summary>
		public static ISet<string> DefaultAttributes => new HashSet<string>
		{

		};
	}
}
