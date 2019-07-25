/****************************************************************************
	Program.cs

	Owner: andremcq
	Copyright (c) Microsoft Corporation

	Main entry point.
****************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Office.Web.OfficeMarketplace.TimedScopeGen
{
	/// <summary>
	/// Main entry point
	/// </summary>
	/// <owner alias="andremcq">Andre McQuaid</owner>
	internal class Program
	{
		/// <summary>
		/// Omex product name
		/// </summary>
		/// <owner alias="andremcq">Andre McQuaid</owner>
		public const string ProductOmex = "Omex";


		/// <summary>
		/// Main entry point from command line.
		/// </summary>
		/// <param name="arguments">Command line arguments</param>
		/// <returns>0 if successfull; 1 otherwise</returns>
		/// <owner alias="andremcq">Andre McQuaid</owner>
		private static int Main(string[] arguments)
		{
			if (arguments.Length < 2)
			{
				FileVersionInfo version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
				Console.WriteLine("Office Marketplace timed scope generator, version {0}", version.FileVersion);
				Console.WriteLine("Usage: countergen.exe [TimedScopes.xml] [OutputFile.cs]");
				return 1;
			}

			try
			{
				FileInfo timedScopesDefinitionFile = new FileInfo(arguments[0]);
				FileInfo timedScopesClassFile = new FileInfo(arguments[1]);
				bool isSharedTimedScopes = string.Equals(arguments[2], "true", StringComparison.OrdinalIgnoreCase);

				if (!timedScopesDefinitionFile.Exists)
				{
					Console.WriteLine("Error. File does not exist: {0}.", timedScopesDefinitionFile);
					return 1;
				}

				GenerateTimedScopesClass(timedScopesDefinitionFile, timedScopesClassFile, isSharedTimedScopes);
			}
			catch (Exception exception)
			{
				Console.WriteLine("Error. Failed to generate timed scopes. Exception: {0}", exception);
				return 1;
			}

			Console.WriteLine("Success");
			return 0;
		}


		/// <summary>
		/// Generates TimedScope.cs from TimedScopes.xml
		/// </summary>
		/// <param name="timedScopesDefinitionFile">Timed scopes definition file</param>
		/// <param name="timedScopesFile">File to generate C# static class</param>
		/// <param name="isSharedTimedScope">Is the TimedScope class to be generated a Shared one.</param>
		/// <owner alias="matoma" />
		public static void GenerateTimedScopesClass(FileInfo timedScopesDefinitionFile, FileInfo timedScopesFile, bool isSharedTimedScope = false)
		{
			TimedScopeCollection timedScopeCollection = ReadFromFile<TimedScopeCollection>(timedScopesDefinitionFile);

			TimedScopeClassTemplate template = new TimedScopeClassTemplate(timedScopeCollection, isSharedTimedScope);
			string generatedClass = template.TransformText();
			using (StreamWriter writer = new StreamWriter(timedScopesFile.FullName))
			{
				writer.Write(generatedClass);
			}
		}


		/// <summary>
		/// Reads object from file using xml deserializer
		/// </summary>
		/// <typeparam name="T">Type of object to read</typeparam>
		/// <param name="file">File from which the object should be read</param>
		/// <returns>Deserialized object</returns>
		/// <owner alias="matoma"/>
		public static T ReadFromFile<T>(FileInfo file)
		{
			using (FileStream stream = file.OpenRead())
			{
				using (XmlReader reader = XmlReader.Create(stream))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(T));
					return (T)serializer.Deserialize(reader);
				}
			}
		}
	}
}