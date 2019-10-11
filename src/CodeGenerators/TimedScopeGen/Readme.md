# Microsoft Omex TimedScope C# file code generator.
(c) Microsoft Corporation.

This tool generates a C# code file for timedscopes, based on input timedscope xml file. See The [Gating.Example project](https://github.com/microsoft/Omex/tree/master/src/Gating.Example) in this repository for an example of these input files.

This package should be used from another project that uses timedscopes. After being added to the project it will automatically run an initial build step that will generate the `TimedScope.cs` file allowing the project files to reference strongly typed timedscopes in code.

The project needs to define the following content in the csproj file and include the corresponding timedscope xml file:

```xml
<ItemGroup>
    <TimedScope Include="timedscope_xml_path">
      <Name>timedscope_name</Name>
    </TimedScope>
</ItemGroup>
```