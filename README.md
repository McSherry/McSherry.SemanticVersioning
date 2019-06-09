# Semantic Versioning for .NET [![Build Status][5]][6]

[5]: https://ci.appveyor.com/api/projects/status/2cwcupcpk6ja90i4?branch=master
[6]: https://ci.appveyor.com/project/McSherry/libsemver-net

`McSherry.SemanticVersioning` is a comprehensive library for working with
[Semantic Versions][1]. It takes care of parsing, comparing, formatting, and filtering
and is intended as an easy-to-use, plug-and-play component of self-updating
software, package managers, and any other software that needs to work with
semantic versions.

[1]: http://semver.org

## Features

- Full support for Semantic Versioning ([2.0.0][7]) and Monotonic Versioning ([1.2][8])
- Practically full support for [`node-semver`][9] version ranges (up to v6.0.0)[**\***][10]
- Flexible and configurable parsing to suit nearly any application
- Targets .NET Framework 4.5 and 4.6, .NET Core 1.0, and .NET Standard 1.0

[7]: <https://semver.org/spec/v2.0.0.html>
[8]: <http://blog.appliedcompscilab.com/monotonic_versioning_manifesto/>
[9]: <https://github.com/npm/node-semver/tree/v6.0.0>
[10]: ./docs/McSherry.SemanticVersioning/Ranges/VersionRange#Remarks

## Getting Started

Installation is simple, as the library is available via [NuGet][2]. To install,
use the following from the NuGet [Package Manager Console][3]:

```
Install-Package McSherry.SemanticVersioning
```

Once installed, just import the `McSherry.SemanticVersioning` namespace and
you're all set. Here's a small example to get you started:

#### Basic comparison

```c#
// The version we'll be comparing against.
var comparand = (SemanticVersion)"1.7.0";

while (true)
{    
    Console.Write("Enter a version number: ");
    var versionStr = Console.ReadLine();
    
    SemanticVersion userVersion;
    if (!SemanticVersion.TryParse(versionStr, out userVersion))
        Console.WriteLine("Uh oh! That's not a valid version!");
    else if (userVersion > comparand)
        Console.WriteLine($"Higher precedence than {comparand}!");
    else if (userVersion < comparand)
        Console.WriteLine($"Lower precedence than {comparand}!");
    else
        Console.WriteLine($"Equal precedence to {comparand}!");
        
    Console.WriteLine();
}
```

#### Version range comparison

```c#
using McSherry.SemanticVersioning.Ranges;

// The range of versions we want to accept.
var range = new VersionRange("1.7.x || 1.8.x");

while (true)
{
    Console.Write("Enter a version number: ");
    var versionStr = Console.ReadLine();
    
    if (!SemanticVersion.TryParse(versionStr, out var result))
    {
        Console.WriteLine("That's not a valid version!");
    }
    else
    {
        Console.WriteLine($"Acceptable? {range.SatisfiedBy(result)}.");
    }
    
    Console.WriteLine();
}
```



[2]: https://www.nuget.org/packages/McSherry.SemanticVersioning/
[3]: http://docs.nuget.org/consume/package-manager-console


## Contributing

Contributions are welcome, especially to documentation (both code comments
and the [markdown documentation][4]).

[4]: ./docs


## Licence Information

The project is licensed under the MIT licence.

Copyright &copy; 2015-19 Liam McSherry.