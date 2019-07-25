Microsoft Omex Gates C# file code generator.
(c) Microsoft Corporation.

This tool generates a C# code file for gating, based on input OmexGates.xml and OmexTip.xml files. See The [Gating.Example project](https://github.com/microsoft/Omex/tree/master/src/Gating.Example) in this repository for an example of these input files.

This package should be used from another project that uses gating. After being added to the project it will automatically run an initial build step that will generate the `GeneratedGates.cs` file allowing the project files to reference strongly typed gates in code.

The project needs to define the following content in the csproj file and include the corresponding gates and tip xml files:

```xml
  <PropertyGroup>
    <GatesXml>OmexGates.xml</GatesXml>
    <TipXml>OmexTip.xml</TipXml>
    <OmexGatesNamespace></OmexGatesNamespace>
  </PropertyGroup>
```

The `OmexGatesNamespace` value is optional.

Manual usage (strongly not recommended, see above on how to use it from another project):

```shell
  Microsoft.Omex.CodeGenerators.GateGen.exe omexgates.xml omextip.xml output.cs [namespace]
```

where `omexgates.xml` and `omextip.xml` are the input files, `output.cs` is the generated C# file and an optional `namespace` can be given to be used in the generated C# file.
