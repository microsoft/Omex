# Microsoft Omex

This repository contains source code for shared components used by the Omex team at Microsoft to build scalable and highly available distributed systems.

The code is released under the [MIT license](https://github.com/microsoft/Omex/blob/master/LICENSE).

The pre-release NuGet packages are available to download from the MyGet feed

    https://www.myget.org/F/omex/api/v3/index.json

### Projects in this repository

* [__Gating__](https://github.com/microsoft/Omex/tree/master/src/Gating) - This library provides an implementation of a flighting mechanism for new features.
* [__DocumentDb__](https://github.com/microsoft/Omex/tree/master/src/DocumentDb) - This library contains wrapper APIs over Microsoft Azure Document Db .Net Client SDK.
* [__System__](https://github.com/microsoft/Omex/tree/master/src/System) - This library contains shared code for Omex libraries. You'll find there utilities for logging,
argument validation, resource management, caching and more.
* [__System.UnitTests.Shared__](https://github.com/microsoft/Omex/tree/master/src/System.UnitTests.Shared) - This library contains abstractions and utilities used for creating unit tests.
* [__Gating.UnitTests.Shared__](https://github.com/microsoft/Omex/tree/master/src/Gating.UnitTests.Shared) - This library provides classes used in unit tests for Gating library.
* [__Gating.Example__](https://github.com/microsoft/Omex/tree/master/src/Gating.Example) - This is a small console application that uses the Gating library to showcase some of its features.
* [__GateGen__](https://github.com/microsoft/Omex/tree/master/src/CodeGenerators/GateGen) - This is a code generator tool that produces strongly typed gates (C# file) from the gates xml files. Should be used as a dependency in another project.

Please contribute to this repository via [pull requests](https://github.com/Microsoft/Omex/pulls) against the __master__ branch.

[![Build Status](https://dev.azure.com/ms/Omex/_apis/build/status/Microsoft.Omex?branchName=master)](https://dev.azure.com/ms/Omex/_build/latest?definitionId=73&branchName=master)

## Building

To build the solution you will need

* .NET Core 2.1 SDK
* Visual Studio 2017 or newer (optional)

Building in Visual Studio is straightforward. If you use .NET Core CLI then invoke

    dotnet restore
    dotnet build

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Documentation

All documentation is located in the `./doc` folder. If you would like to contribute to the documentation, please submit a pull request.

## Communicating with the Team
The easiest way to communicate with the team is via GitHub issues. Please file new issues, feature requests and suggestions.

# FAQ
## What is the difference between the master and release branches?
There are two types of NuGet packages that get built from the code in this repository:
* __Pre-release packages__: the pre-release packages are built after every change on the master branch
* __Release packages__: after a period of time when more changes are made to master, the Team creates release packages which are published to NuGet.org.
The release packages are built from the master branch. Public contributions are accepted only from the master branch.

## Where are the NuGet packages for the components in this repository available for download?
The pre-release NuGet packages which are built from the master branch are available to download from the MyGet feed

    https://www.myget.org/F/omex/api/v3/index.json


Soon, the release packages will be available for download from NuGet.org.


