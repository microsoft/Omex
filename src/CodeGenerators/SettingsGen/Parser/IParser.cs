// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Parser
{
	/// <summary>
	/// 
	/// </summary>
	public interface IParser<TSettings> : ISyntaxContextReceiver where TSettings : class
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		TSettings GetSettings();
	}
}
