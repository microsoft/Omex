// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging
{
	internal class BasicMachineInformation : EmptyMachineInformation
	{
		public BasicMachineInformation()
		{
			MachineName = GetMachineName();
			MachineId = MachineName;
			MachineClusterIpAddress = GetIpAddress(MachineName);
			BuildVersion = GetBuildVersion();
		}
	}
}
