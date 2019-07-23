Microsoft Omex Gates C# file code generator.
(c) Microsoft Corporation.

This tool generates a C# code file for gating, based on input OmexGates.xml and OmexTip.xml files. See The Gating.Example project in this repository for an example of these input files.

Usage: Microsoft.Omex.CodeGenerator.GateGen.exe omexgates.xml omextip.xml output.cs [namespace]
where omexgates.xml and omextip.xml are the input files, output.cs is the generated C# file and an optional namespace can be given to be used in the generated C# file.
