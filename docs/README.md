# Semantic Versioning for .NET

`libSemVer.NET` is a library, targeting .NET Framework 4.5 and written in
C#, that handles [Semantic Versions][1]. `libSemVer.NET` takes care of parsing,
comparing, and formatting semantic versions.

[1]: http://semver.org

The library is intended to be an easy-to-use, "plug and play" component of any
piece of software that needs to work with semantic versions, such as package
managers or self-updating programs.


## Getting Started

Installation is simple, as the library is available via [NuGet][2]. To install,
use the following from the NuGet [Package Manager Console][3]:

```
Install-Package McSherry.SemanticVersioning
```

Once installed, just import the `McSherry.SemanticVersioning` namespace and
you're all set. Here's a small example to get you started:

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

[2]: https://www.nuget.org/packages/McSherry.SemanticVersioning/
[3]: http://docs.nuget.org/consume/package-manager-console


## Contributing

Contributions are welcome, especially to documentation (both code comments
and the markdown documentation).


## Licence Information

The project is licensed under the [MIT licence](../LICENCE.txt).

Copyright &copy; 2015 Liam McSherry.