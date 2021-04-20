// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Models.Attributes
{
	/// <summary>
	/// This value defines the type of value you have specified in the 'Value' Attribute.
	/// It can be SecretsStoreRef/Encrypted/PlainText.
	/// </summary>
	public enum TypeKind
	{
		/// <summary>
		/// If set to SecretsStoreRef, we retrieve the reference value from the SecretStore
		/// </summary>
		SecretsStoreRef,

		/// <summary>
		///  If set to Encrypted,
		///  the application developer is responsible for creating a certificate
		///  and using the Invoke-ServiceFabricEncryptSecret cmdlet to encrypt sensitive information
		/// </summary>
		Encrypted,

		/// <summary>
		/// Plaintext
		/// </summary>
		PlainText
	}
}
