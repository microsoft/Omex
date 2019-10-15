# Microsoft Omex TimedScopes C# file code generator.
(c) Microsoft Corporation.

This tool generates a C# code file for timed scopes, based on input timedscope xml file. You can use timed scopes to handle logging or error and success conditions for a block of code and for measuring runtime performance of the given block.

This package should be used from another project that uses timedscopes. After being added to the project it will automatically run an initial build step that will generate the `TimedScopes.cs` file allowing the project files to reference strongly typed timedscopes in code.

The project needs to define the following content in the csproj file:

```xml
<ItemGroup>
    <TimedScope Include="timedscope xml path">
      <Name><timedscope_file_prefix></Name>
    </TimedScope>
</ItemGroup>
```

And include the corresponding timedscope xml file with at that path, similar to this:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<TimedScopes xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" namespace="Microsoft.Omex.CodeGenerators.TimedScopeGen.UnitTests" xmlns="http://tempuri.org/TimedScopes.xsd">
  <TimedScopeArea name="ExampleArea" friendlyName="Example TimedScopes area">
    <TimedScope name="ExampleTimedScope">Example TimedScope</TimedScope>
  </TimedScopeArea>
</TimedScopes>
```

Then you can build and then use the strongly typed timedscope in your code like this:

```csharp
    // Use the timedscope you defined in the XML file
    // For a simpler Create() call, create an extension that fills in all the parameters in with objects you already created
    // So that your new call just does (ExampleTimedScope.Create(TimedScopeResult.SystemError))
    
    using (TimedScope scope =  TimedScopes.ExampleArea.ExampleTimedScope.Create(correlationData, machineInfo,
        timedScopeLogger, replayEventConfigurator, timedScopeStackManager, TimedScopeResult.SystemError))
    {
        // It's good practice to create the timedscope with a SystemError result from the start so that on any
        // unexpected error you exit the scope with a SystemError state automatically

        //Do work that might fail (on expected errors you set scope.Result = TimedScopeResult.ExpectedError)

        // On success you set the result 
        scope.Result = TimedScopeResult.Success;
    }
```

Building the project will generate a file named `<timedscope_file_prefix>TimedScopes.cs` that allows you to use the timedscope type as shown in the C# code above. The namespace in the generated file is the one defined in the timedscope xml file (in the example it would be `namespace Microsoft.Omex.CodeGenerators.TimedScopeGen.UnitTests`).
