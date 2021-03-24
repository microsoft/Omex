// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Validation
{
	/// <summary>
	/// Validate whether new settings match settings in XML file
	/// </summary>
	public class XmlValidation<TSettingModel> : IValidation<TSettingModel>
		where TSettingModel : class, IEqualityComparer<TSettingModel>, IEquatable<TSettingModel>
	{
		/// <inheritdoc/>
		public bool AreExistingSettingsEqual(TSettingModel newSettings, AdditionalText filename)
		{
			XmlSerializer serializer = new(typeof(TSettingModel));
			serializer.UnknownNode += new XmlNodeEventHandler(SerializerUnknownNode);
			serializer.UnknownAttribute += new
			XmlAttributeEventHandler(SerializerUnknownAttribute);

			string? fileContent = filename.GetText()?.ToString();

			if (fileContent is null)
			{
				return false;
			}

			// A FileStream is needed to read the XML document.
			using StringReader stringReader = new(filename.GetText()?.ToString());
			// Declares an object variable of the type to be deserialized.
			TSettingModel model = (TSettingModel)serializer.Deserialize(stringReader);

			return newSettings.Equals(model);
		}

		/// <summary>
		/// Serialize unknown node
		/// </summary>
		/// <param name="sender">Sender object</param>
		/// <param name="e">Xml node event args</param>
		protected void SerializerUnknownNode(object sender, XmlNodeEventArgs e)
		{
			Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
		}

		/// <summary>
		/// Seriliaze unknown attribute
		/// </summary>
		/// <param name="sender">Sender object</param>
		/// <param name="e">Xml attribute event arguments</param>
		protected void SerializerUnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			System.Xml.XmlAttribute attr = e.Attr;
			Console.WriteLine("Unknown attribute " +
			attr.Name + "='" + attr.Value + "'");
		}
	}
}
