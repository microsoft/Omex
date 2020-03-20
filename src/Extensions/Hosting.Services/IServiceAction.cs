// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Action that will be executed in RunAsync methond of SF service
	/// </summary>
	public interface IServiceAction<in TService>
	{
		/// <summary>
		/// Action that will be executed in RunAsync methond of SF service
		/// </summary>
		Task RunAsync(TService service, CancellationToken cancellationToken);
	}
}
