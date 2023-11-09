// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares.UserIdentity.Options
{
	/// <summary>
	/// Options for static salt provider
	/// </summary>
	public class StaticSaltProviderOptions
	{
		/// <summary>
		/// Set the hash salt
		/// </summary>
		public string SaltValue {  get; set; } = string.Empty;
	}
}
