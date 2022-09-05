# Microsoft OMEX

This repository contains source code for shared components used by the 
team, which is part of the Office organization, at Microsoft to build scalable and highly available distributed systems.

The code is released under the [MIT license](https://github.com/microsoft/Omex/blob/main/LICENSE).

The release NuGet packages are available for download from Nuget.org

    https://www.nuget.org/packages?q=omex

Additional source code from the OMEX team can be located at <https://github.com/microsoft/PR-Metrics>. This project is an Azure DevOps Extension and GitHub Action that augments PR titles with size and test coverage indicators.

### Projects in this repository

* [__DocumentDb__](https://github.com/microsoft/Omex/tree/main/src/DocumentDb) - This library contains wrapper APIs over Microsoft Azure Document Db .Net Client SDK.
* [__System__](https://github.com/microsoft/Omex/tree/main/src/System) - This library contains shared code for OMEX libraries. You'll find there utilities for logging,
argument validation, resource management, caching and more.
* [__System.UnitTests.Shared__](https://github.com/microsoft/Omex/tree/main/src/System.UnitTests.Shared) - This library contains abstractions and utilities used for creating unit tests.

Please contribute to this repository via [pull requests](https://github.com/Microsoft/Omex/pulls) against the __main__ branch.

[![Build Status](https://dev.azure.com/ms/Omex/_apis/build/status/Microsoft.Omex?branchName=main)](https://dev.azure.com/ms/Omex/_build/latest?definitionId=73&branchName=main)

## Building

To build the solution you will need

* .NET Core 3.0 SDK or newer
* Visual Studio 2019 (16.3) or newer

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


## Coding Style
Please follow the pattern of what you already see in the code.

### Code Overview
The code is organized in different projects, each one having its own project file.
Each project produces a NuGet package.
All the projects are included in the main solution file in the root directory.
Please contribute to existing projects.
If you would like to create a new project, please discuss this with the Team first using GitHub issues.


## Documentation

All documentation is located in the `./doc` folder. If you would like to contribute to the documentation, please submit a pull request.

## Communicating with the Team
The easiest way to communicate with the team is via GitHub issues. Please file new issues, feature requests and suggestions.

# FAQ
## What is the difference between the main and release branches?
There are two types of NuGet packages that get built from the code in this repository:
* __Pre-release packages__: the pre-release packages are built after every change on the main branch
* __Release packages__: after a period of time when more changes are made to main, the Team creates release packages which are published to NuGet.org.
The release packages are built from the main branch. Public contributions are accepted only from the main branch.

## Where are the NuGet packages for the components in this repository available for download?
The pre-release NuGet packages which are built from the main branch are available to download from the MyGet feed

    https://www.myget.org/F/omex/api/v3/index.json


The release packages are available for download from NuGet.org

    https://www.nuget.org/packages?q=omex


