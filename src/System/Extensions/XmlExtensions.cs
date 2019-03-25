// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.Extensions
{
	/// <summary>
	/// Extension methods for processing and reading XML from a file according
	/// to a defined schema.
	/// </summary>
	/// <remarks>This class can be used for construction of XML-based caches.</remarks>
	public static class XmlExtensions
	{
		#region Public methods

		/// <summary>
		/// Retrieves attribute value from a provided XML element.
		/// </summary>
		/// <param name="xmlElement">invitation XML element</param>
		/// <param name="attributeName">attribute name</param>
		/// <param name="optional">is the attribute optional? the default value is false</param>
		/// <returns>Null if attribute is missing, otherwise its value</returns>
		public static string GetAttributeValue(this XElement xmlElement, string attributeName, bool optional = false)
		{
			if (!Code.ValidateArgument(xmlElement, "xmlElement", TaggingUtilities.ReserveTag(0x2385060b /* tag_97qyl */)) ||
				!Code.ValidateNotNullOrWhiteSpaceArgument(attributeName, "attributeName", TaggingUtilities.ReserveTag(0x2385060c /* tag_97qym */)))
			{
				return null;
			}

			XAttribute attribute = xmlElement.Attribute(attributeName);

			if (!optional &&
				!Code.Validate(attribute != null, string.Format(CultureInfo.InvariantCulture,
					"{0} attribute is missing in the provided Xml", attributeName), TaggingUtilities.ReserveTag(0x2385060d /* tag_97qyn */)))
			{
				return null;
			}

			return attribute != null ? attribute.Value : null;
		}


		/// <summary>
		/// Creates an untyped object containing the XML data. This object
		/// can be cast to the type passed in parameter "type" by the caller.
		/// </summary>
		/// <param name="stream">The memory stream from which to read.</param>
		/// <param name="type">The object type to read.</param>
		/// <param name="schema">The schema to use.</param>
		/// <param name="schemaUri">The schema URI to use.</param>
		/// <returns>The deserialised object.</returns>
		public static object Read(this Stream stream, Type type, string schema, string schemaUri)
		{
			return Read(stream, CompressionType.NoCompression, type, schema, schemaUri, null, null, null);
		}


		/// <summary>
		/// Creates an untyped object containing the XML data. This object
		/// can be cast to the type passed in parameter "type" by the caller.
		/// </summary>
		/// <typeparam name="T"> Class Type </typeparam>
		/// <param name="stream">The memory stream from which to read.</param>
		/// <param name="schema">The schema to use.</param>
		/// <param name="schemaUri">The schema URI to use.</param>
		/// <returns>The deserialised object.</returns>
		public static T Read<T>(this Stream stream, string schema, string schemaUri = null) where T : class
		{
			return Read(stream, CompressionType.NoCompression, typeof(T), schema, schemaUri, null, null, null) as T;
		}


		/// <summary>
		/// Deserialise stream using provided xml parser objects.
		/// Use this method if you perform multiple deserialisations of the same type to improve performance
		/// </summary>
		/// <param name="stream">The memory stream to deserialise</param>
		/// <param name="compressionType">compression type of the input stream </param>
		/// <param name="context">XmlParserContext to be used to read the stream</param>
		/// <param name="settings">XmlReaderSettings to be used to read the stream</param>
		/// <param name="serialiser">XmlSerialiser to be used to read the stream</param>
		/// <returns>The deserialised object.</returns>
		public static object Read(this Stream stream, CompressionType compressionType, XmlParserContext context, XmlReaderSettings settings, XmlSerializer serialiser)
		{
			return Read(stream, compressionType, null, null, null, context, settings, serialiser);
		}


		/// <summary>
		/// Verifies that given xml file stream is a well formed xml document
		/// </summary>
		/// <param name="stream">xml file stream to validate</param>
		/// <param name="compressionType">compression type of the stream</param>
		/// <param name="errorMessage">validation errors or empty string if no errors found</param>
		/// <returns>true if xml file is well formed, false otherwise</returns>
		public static bool IsWellFormedXml(this Stream stream, CompressionType compressionType, out string errorMessage)
		{
			if (stream == null)
			{
				errorMessage = "Null Stream passed for validation to IsWellFormedXml.";
				return false;
			}

			if (compressionType == CompressionType.GZip)
			{
				using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
				{
					return IsWellFormedXml(gzipStream, CompressionType.NoCompression, out errorMessage);
				}
			}

			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ConformanceLevel = ConformanceLevel.Document;
			settings.IgnoreWhitespace = true;
			settings.IgnoreComments = true;
			settings.CheckCharacters = true;
			try
			{
				using (XmlReader reader = XmlReader.Create(stream, settings))
				{
					while (reader.Read()) { }
				}
				errorMessage = string.Empty;
				return true;
			}
			catch (Exception exception)
			{
				if (exception.IsFatalException())
				{
					throw;
				}
				errorMessage = exception.ToString();
				return false;
			}
		}


		/// <summary>
		/// Validate xml document against the schema
		/// </summary>
		/// <param name="xmlDocument">Xml document</param>
		/// <param name="schemaStream">Schema stream</param>
		/// <param name="schemaNamespace">Schema target namespace</param>
		/// <param name="validationErrors">Validation errors</param>
		/// <returns>True if schema validation succeeds; false if there are errors</returns>
		public static bool ConformsToXmlSchema(this XDocument xmlDocument, Stream schemaStream, string schemaNamespace, out ICollection<string> validationErrors)
		{
			validationErrors = new List<string>();

			if (xmlDocument == null)
			{
				validationErrors.Add("Null xmlDocument passed for validation to ConformsToXmlSchema.");
			}

			if (schemaStream == null)
			{
				validationErrors.Add("Null schemaStream passed for validation to ConformsToXmlSchema.");
			}

			if (schemaNamespace == null)
			{
				validationErrors.Add("Null schemaNamespace passed for validation to ConformsToXmlSchema.");
			}

			if (validationErrors.Count > 0)
			{
				return false;
			}

			XmlSchemaSet schemas = new XmlSchemaSet();
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.XmlResolver = null;
			schemas.Add(schemaNamespace, XmlReader.Create(schemaStream, settings));

			ICollection<string> errors = new List<string>();
			xmlDocument.Validate(schemas, (o, e) =>
			{
				errors.Add(e.Message);
			});
			validationErrors = errors;

			return validationErrors.Count == 0;
		}


		/// <summary>
		/// Verifies if given xml file is valid as specified by the given xml schema
		/// </summary>
		/// <param name="xmlStream">xml file stream to validate</param>
		/// <param name="schemaStream">schema file stream</param>
		/// <param name="schemaNamespace">schema namespace</param>
		/// <param name="compressionType">compression type of the xmlStream</param>
		/// <param name="errorMessage">validation error or empty string if no errors found</param>
		/// <returns>true if the xml file has been successfully validated using the schema provided</returns>
		public static bool ConformsToXmlSchema(this Stream xmlStream, Stream schemaStream, string schemaNamespace, CompressionType compressionType, out string errorMessage)
		{
			if (xmlStream == null)
			{
				errorMessage = "Null xmlStream passed for validation to ConformsToXmlSchema.";
				return false;
			}

			if (compressionType == CompressionType.GZip)
			{
				using (GZipStream gzipStream = new GZipStream(xmlStream, CompressionMode.Decompress))
				{
					return ConformsToXmlSchema(gzipStream, schemaStream, schemaNamespace, CompressionType.NoCompression, out errorMessage);
				}
			}

			if (schemaStream == null)
			{
				errorMessage = "Null schemaStream passed for validation to ConformsToXmlSchema.";
				return false;
			}

			try
			{
				XmlSchemaSet schemas = new XmlSchemaSet();
				using (XmlReader schemaReader = XmlReader.Create(schemaStream))
				{
					schemas.Add(schemaNamespace, schemaReader);
					XmlReaderSettings settings = new XmlReaderSettings();
					settings.ValidationType = ValidationType.Schema;
					settings.Schemas = schemas;

					NameTable nt = new NameTable();
					XmlParserContext context = new XmlParserContext(null, new XmlNamespaceManager(nt), null, XmlSpace.Default);

					using (XmlReader reader = XmlReader.Create(xmlStream, settings, context))
					{
						while (reader.Read()) { }
					}
				}
				errorMessage = string.Empty;
				return true;
			}
			catch (Exception exception)
			{
				if (exception.IsFatalException())
				{
					throw;
				}
				errorMessage = exception.ToString();
				return false;
			}
		}


		/// <summary>
		/// Creates XmlParserContext
		/// </summary>
		/// <param name="schemaUri">The schema URI to use.</param>
		/// <returns>XmlParserContext created</returns>
		public static XmlParserContext GetXmlParserContext(string schemaUri)
		{
			NameTable nt = new NameTable();
			XmlParserContext context = new XmlParserContext(null, new XmlNamespaceManager(nt), null, XmlSpace.Default);
			if (schemaUri != null)
			{
				context.NamespaceManager.AddNamespace(schemaUri, schemaUri);
			}

			return context;
		}


		/// <summary>
		/// Retrieves the XML reader settings.
		/// </summary>
		/// <param name="type">The object type to read.</param>
		/// <param name="schema">The schema to use.</param>
		/// <param name="schemaUri">The schema URI to use.</param>
		/// <returns>The relevant XML reader settings.</returns>
		public static XmlReaderSettings GetXmlReaderSettings(this Type type, string schema, string schemaUri)
		{
			if (type == null)
			{
				ULSLogging.LogTraceTag(0x23850322 /* tag_97qm8 */, Categories.Common, Levels.Error,
					"Null type value passed to GetXmlReaderSettings.");
				return null;
			}

			if (string.IsNullOrEmpty(schema))
			{
				ULSLogging.LogTraceTag(0x23850323 /* tag_97qm9 */, Categories.Common, Levels.Error,
					"Null or empty schema value passed to GetXmlReaderSettings.");
				return null;
			}

			XmlSchemaSet schemas = new XmlSchemaSet();

			using (XmlReader reader = XmlReader.Create(type.Assembly.GetManifestResourceStream(schema)))
			{
				schemas.Add(schemaUri, reader);
			}

			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ValidationType = ValidationType.Schema;
			settings.Schemas = schemas;
			return settings;
		}

		#endregion

		#region Private methods

		/// <summary>Creates an untyped object containing the XML data. This object
		/// can be casted to the type passed in parameter "type" by the caller.</summary>
		/// <param name="stream">The memory stream from which to read.</param>
		/// <param name="compressionType">Compression type of the input stream </param>
		/// <param name="type">The object type to read.</param>
		/// <param name="schema">The schema to use.</param>
		/// <param name="schemaUri">The schema URI to use.</param>
		/// <param name="context">XmlParserContext to be used to read the stream</param>
		/// <param name="settings">XmlReaderSettings to be used to read the stream</param>
		/// <param name="serialiser">XmlSerialiser to be used to read the stream</param>
		/// <returns>The deserialised object.</returns>
		private static object Read(Stream stream, CompressionType compressionType, Type type, string schema,
			string schemaUri, XmlParserContext context, XmlReaderSettings settings, XmlSerializer serialiser)
		{
			try
			{
				if (compressionType == CompressionType.GZip)
				{
					using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
					{
						return Read(gzipStream, CompressionType.NoCompression, type, schema, schemaUri, context, settings, serialiser);
					}
				}

				if (context == null)
				{
					context = GetXmlParserContext(schemaUri);
				}

				if (settings == null)
				{
					settings = type.GetXmlReaderSettings(schema, schemaUri);
				}

				using (XmlReader reader = XmlReader.Create(stream, settings, context))
				{
					if (serialiser == null)
					{
						serialiser = new XmlSerializer(type);
					}

					return serialiser.Deserialize(reader);
				}
			}
			catch (InvalidDataException exception)
			{
				ULSLogging.ReportExceptionTag(0x23850340 /* tag_97qna */, Categories.Common, exception,
					"XmlExtensions failed to read/decompress incorrect file stream.");
				return null;
			}
			catch (InvalidOperationException exception)
			{
				ULSLogging.ReportExceptionTag(0x23850341 /* tag_97qnb */, Categories.Common, exception,
					"XmlExtensions failed to parse file for given object's state.");
				return null;
			}
			catch (Exception exception)
			{
				ULSLogging.ReportExceptionTag(0x23850342 /* tag_97qnc */, Categories.Common, exception,
					"XmlExtensions failed to parse file.");
				return null;
			}
		}

		#endregion
	}
}