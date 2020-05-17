Whisk <img src="Whisk.svg" width="42" height="42" />
=======

Whisk is a micro-framework for tracking and updating computational dependencies.

[![MIT Licensed](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](license.md)
[![Get it on NuGet](https://img.shields.io/nuget/v/Whisk.svg?style=flat-square)](http://nuget.org/packages/Whisk)

[![Appveyor Build](https://img.shields.io/appveyor/ci/otac0n/Whisk.svg?style=flat-square)](https://ci.appveyor.com/project/otac0n/Whisk)
[![Test Coverage](https://img.shields.io/codecov/c/github/otac0n/Whisk.svg?style=flat-square)](https://codecov.io/gh/otac0n/Whisk)
[![Pre-release packages available](https://img.shields.io/nuget/vpre/Whisk.svg?style=flat-square)](http://nuget.org/packages/Whisk)

Getting Started
---------------

    PM> Install-Package Whisk

Example
-------

This C# snippet shows how to create mutable value, subscribe to changes, and make changes atomically:

```C#
// Create mutable containers for first and last name.
var first = D.Mutable("Reginald");
var last = D.Mutable("Dwight");

// Create a full-name dependency that will track the first and last name.
var full = D.Pure(new[] { first, last }, () => $"{first.Value} {last.Value}");

// Watch the full name and output all values to the console.
var subscription = full.Watch(Console.WriteLine);

// Atomically update the first and last name at the same time.
D.Set(D.Value(first, "Elton"), D.Value(last, "John"));

// Stop watching the full name.
subscription.Dispose();
```

The expected output is:

> Reginald Dwight
> Elton John
