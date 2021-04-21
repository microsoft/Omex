// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.CodeGenerators.SettingsGen.Wrappers
{
	internal sealed class AttributeWrapper : IAttributeWrapper
	{
		public AttributeWrapper(string name)
		{
			Arguments = new Dictionary<string, string>();
			Name = name;
		}

		public string Name { get; private set; }

		public IDictionary<string, string> Arguments { get; private set; }
	}
}
