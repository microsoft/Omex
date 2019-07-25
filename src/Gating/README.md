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

All possible attributes can be found in [`IGate`](https://github.com/microsoft/Omex/blob/master/src/Gating/IGate.cs
).

## Purpose

This Gating library is used internally by the Microsoft OMEX team as a flighting framework for new service features.
New features are introduced under a new gate. This means it can be controlled which users will get the new features.
If the gate is enabled, the new feature is active, otherwise the old code path is triggered.

The library allows to turn the feature on for a specific group of users. Only users in a specific region, market,
with the newer version of the app, etc. will get the new feature. The gate definition can be changed in an XML file,
without recompiling the whole project. Once the feature is tested it can easily be enabled for everybody.

## Usage

You can find usage information in the [Gating tutorial](https://github.com/microsoft/Omex/blob/master/docs/Microsoft.Omex.Gating/GatingTutorial.md).
Also take a look at the [Gating.Example application](https://github.com/microsoft/Omex/tree/master/src/Gating.Example).
