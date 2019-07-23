# Microsoft.Omex.Gating

This library provides a gating mechanism that can be used to switch application features
or code paths on/off depending on user request details, such as:

* User ids
* Markets (eg. 'en-us' or 'fr-fr')
* Client Environments
* Host Environments
* ClientVersions
* IP ranges
* Allowed browsers
* Start/End dates of availability

All possible atributes can be found in [`IGate`](https://github.com/microsoft/Omex/blob/master/src/Gating/IGate.cs
).

## Usage

You can find usage information in the [Gating tutorial](GatingTutorial.md).
Also take a look at the [Gating.Example application](https://github.com/microsoft/Omex/tree/master/src/Gating.Example).


