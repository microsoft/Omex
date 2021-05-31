// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Comparing
{
	/// <summary>
	/// Validate whether new settings match settings in XML file
	/// </summary>
	public sealed class XmlComparison<TSettingModel> : IComparison<TSettingModel>
		where TSettingModel : class, IEquatable<TSettingModel>
	{
		/// <inheritdoc/>
		public bool AreExistingSettingsEqual(TSettingModel newSettings, AdditionalText filename)
		{
			XmlSerializer serializer = new(typeof(TSettingModel));

			string? fileContent = filename.GetText()?.ToString();

			if (fileContent is null)
			{
				return false;
			}

			try
			{
				// A FileStream is needed to read the XML document.
				using XmlReader stringReader = XmlReader.Create(fileContent);
				// Declares an object variable of the type to be deserialized.
				TSettingModel model = (TSettingModel)serializer.Deserialize(stringReader);

				return newSettings.Equals(model);
			}
			catch
			{
				return false;
			}
		}
	}
}
