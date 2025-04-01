// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Options for registering a service.
	/// </summary>
	public class ServiceRegistratorOptions
	{
		/// <summary>
		/// Gets or sets the name of the service type.
		/// </summary>
		public string ServiceTypeName { get; set; } = string.Empty;
	}
}
