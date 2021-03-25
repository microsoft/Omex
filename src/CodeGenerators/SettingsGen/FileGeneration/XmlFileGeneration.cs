// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.FileGeneration
{
	/// <summary>
	/// Generates an xml file with the settings model
	/// </summary>
	/// <typeparam name="TSettings">Settings model type</typeparam>
	public class XmlFileGeneration<TSettings> : IFileGenerator<TSettings> where TSettings : class
	{
		/// <inheritdoc/>
		public string FileType => "XML";

		/// <inheritdoc />
		public void GenerateFile(TSettings settings, string filename)
		{
			string content = XmlFileContents(settings);

			// Serializing the xml file creates a different format of the second line in the file as Service Fabric.
			// SF changes the header when the application project gets build and creates changes to the file if these are not swapped.
			content = content.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
			File.WriteAllText(filename, content);
		}

		/// <summary>
		/// Generate file contents from Settings class
		/// </summary>
		/// <param name="settings">Application to generate deployment files from</param>
		/// <returns>Serialised class into Xml string</returns>
		private string XmlFileContents(TSettings settings)
		{
			XmlSerializer serializer = new(typeof(TSettings));
			// There is a more straight forward way of serialising the xml
			// but this is done this way to ensure it uses utf-8 encoding
			using (MemoryStream stringWriter = new())
			{
				UTF8Encoding enc = new(false);
				using (XmlTextWriter writer = new(stringWriter, enc))
				{
					writer.Formatting = Formatting.Indented;
					serializer.Serialize(writer, settings);
					return Encoding.Default.GetString(stringWriter.ToArray());
				}
			}
		}
	}
}
