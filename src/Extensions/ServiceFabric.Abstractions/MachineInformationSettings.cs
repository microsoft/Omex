// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.ServiceFabric.Abstractions
{
	internal class MachineInformationSettings
	{
		public string? Service { get; set; }
		public string? Environment { get; set; }
		public string? Region { get; set; }
		public string? ClusterName { get; set; }
	}
}
