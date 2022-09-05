﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//Imported from https://github.com/dotnet/aspnetcore/blob/main/src/HealthChecks/HealthChecks/src/DelegateHealthCheck.cs

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

#pragma warning disable IDE1006 // ASP.NET Core codebase naming convention

namespace Microsoft.Omex.Preview.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// A simple implementation of <see cref="IHealthCheck"/> which uses a provided delegate to
/// implement the check.
/// </summary>
internal sealed class DelegateHealthCheck : IHealthCheck
{
	private readonly Func<CancellationToken, ValueTask<HealthCheckResult>> _check;

	/// <summary>
	/// Create an instance of <see cref="DelegateHealthCheck"/> from the specified delegate.
	/// </summary>
	/// <param name="check">A delegate which provides the code to execute when the health check is run.</param>
	public DelegateHealthCheck(Func<CancellationToken, ValueTask<HealthCheckResult>> check)
	{
		_check = check ?? throw new ArgumentNullException(nameof(check));
	}

	/// <summary>
	/// Runs the health check, returning the status of the component being checked.
	/// </summary>
	/// <param name="context">A context object associated with the current execution.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check.</param>
	/// <returns>A <see cref="Task{HealthCheckResult}"/> that completes when the health check has finished, yielding the status of the component being checked.</returns>
	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) => _check(cancellationToken).AsTask();
}
