# `McSherry.SemanticVersioning` namespace

The `McSherry.SemanticVersioning` namespace is the root namespace for `libSemVer.NET`. It
contains everything relevant to working with semantic versions.


## Classes

- **[SemanticVersion][1]**  
  Represents an immutable SemanticVersion. This class cannot be inherited.
- **[SemanticVersionFormat][2]**  
  Lists the format identifiers accepted by the [SemanticVersion][1] class's
  implementation of `IFormattable`.
  
[1]: ./SemanticVersion
[2]: ./SemanticVersionFormat


## Enumerations

- **[ParseMode][3]**  
  Represents the parsing modes that the [SemanticVersion][1] parser may be
  configured to use.
  
[3]: ./ParseMode.md


## Namespaces

- **[Ranges][4]**  
  Provides version range functionality intended to make complex comparisons
  against semantic versions easier to work with.
- **[Monotonic][5]**  
  Provides support for Monotonic Versions using the existing Semantic
  Version APIs.
  
[4]: ./Ranges
[5]: ./Monotonic