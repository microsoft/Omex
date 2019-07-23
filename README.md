# Microsoft OMEX

This repository contains source code for libraries created by the OMEX team at Microsoft.

The code is released under the [MIT license](https://github.com/microsoft/Omex/blob/master/LICENSE).

### Projects in this repository

* __Gating__ - This library provides an implementation of a feature/code path gating mechanism.
* __DocumentDb__ - This library contains wrapper APIs over Microsoft Azure Document Db .Net Client SDK.
* __System__ - This library contains shared code for Omex libraries. You'll find there utilities for logging,
argument validation, resource management, caching and more.
* __System.UnitTests.Shared__ - This library contains abstractions and utilities used for creating unit tests.
* __Gating.UnitTests.Shared__ - This library provides classes used in unit tests for Gating library.
* __Gating.Example__ - This is a small console application that uses the Gating library to showcase some of its features.

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
