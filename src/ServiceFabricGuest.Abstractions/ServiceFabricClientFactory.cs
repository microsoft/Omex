// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Hosting.Certificates;

namespace Microsoft.Omex.Extensions.ServiceFabricGuest.Abstractions
{
	/// <summary>
	/// Factory for service fabric client wrapper.
	/// </summary>
	/// <remarks>
	/// Please prefer using instance resolved from DI container.
	/// </remarks>
	[Obsolete("InitializationLogger using OmexLogger is obsolete and is pending for removal by 1 July 2024.", DiagnosticId = "OMEX188")]
	public class ServiceFabricClientFactory
	{
		/// <summary>
		/// Creates service fabric client wrapper instance and returns it.
		/// </summary>
		public static IServiceFabricClientWrapper Create(ServiceFabricRestClientOptions options) =>
			new ServiceFabricClientWrapper(Options.Create(options), InitializationCertificateReader.Instance);
	}
}
