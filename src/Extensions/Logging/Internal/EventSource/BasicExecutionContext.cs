// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging
{
	internal class BasicMachineInformation : EmptyExecutionContext
	{
		public BasicMachineInformation()
		{
			MachineName = GetMachineName();
			MachineId = MachineName;
			ClusterIpAddress = GetIpAddress(MachineName);
			BuildVersion = GetBuildVersion();
		}
	}
}
