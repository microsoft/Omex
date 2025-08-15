// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.AspNetCore;

/// <summary>
/// This endpoint filter tries to identify whether the current <seealso cref="Activity"/> has been marked
/// as a Liveness health check. If it is, it returns a 200 response.
/// </summary>
public class LivenessCheckActionFilterAttribute : ActionFilterAttribute
{
	/// <inheritdoc />
	public override void OnActionExecuting(ActionExecutingContext context)
	{
		if (Activity.Current?.IsLivenessCheck() == true)
		{
			context.Result = new OkObjectResult("Running test healthcheck transaction");
		}
	}
}
