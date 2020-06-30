// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Compatibility.UlsLoggerAdapter;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for ServiceCollection
	/// </summary>
	public static class ServiceCollectionExtension
	{
		/// <summary>
		/// Register hosted service to report logs from Microsoft.Omex.System.UlsLogger
		/// </summary>
		public static IServiceCollection AddUlsLoggerAddapter(this IServiceCollection services) =>
			services.AddHostedService<UlsLogAddapter>();
	}
}
