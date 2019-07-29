// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.System.UnitTests.Shared;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Omex.System.AspNetCore.UnitTests
{

	/// <summary>
	/// Asp Net Core Unit test base class
	/// </summary>
	public abstract class AspNetCoreUnitTestsBase : UnitTestBase
	{

		/// <summary>
		/// Static constructor
		/// </summary>
		static AspNetCoreUnitTestsBase()
		{
			HttpContextWrapper.Configure(m_httpContextAccessor.Value);
		}


		/// <summary>
		/// Http context accessor
		/// </summary>
		protected static readonly Lazy<IHttpContextAccessor> m_httpContextAccessor = new Lazy<IHttpContextAccessor>(
			() => new HttpContextAccessor(),
			LazyThreadSafetyMode.PublicationOnly);
	}
}
