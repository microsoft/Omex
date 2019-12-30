// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Omex.Extensions.Compatability.Validation;

namespace Microsoft.Omex.System.Validation
{
	/// <summary>
	/// Indicates to code analysis tools that a method validates a particular parameter to not be null.
	/// 
	/// This can for instance be used to suppress VS design warning CA1062 (validate public method parameters before use) by applying to
	/// external validation libraries, such as the <see cref="Code" /> class in this project.
	/// 
	/// If an attribute with the name ValidatedNotNull is present on the parameter of the validation method it tells the analyzer that the 
	/// parameter will never be null in the code path following the execution of that method.
	/// </summary>
	/// <example>
	/// In this example the <c>Guard.NotNull</c> has the attribute present on its parameter. If it wasn't present the analyzer would not know
	/// that <c>s</c> cannot be <c>null</c> following the execution of the method, resulting in a warning on the execution of the
	/// <c>toUpperInvariant</c> method.
	/// <code>
	/// string s = mightBeNullClass?.stringProperty;
	///
	/// Guard.NotNull(s);
	///
	/// // The analyzer now knows `s` will never be null here
	/// return s.toUpperInvariant();
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	internal sealed class ValidatedNotNullAttribute : Attribute { }
}
