# `McSherry.SemanticVersioning.Monotonic` namespace

The `McSherry.SemanticVersioning.Monotonic` namespace provides classes which aid in the
support of [Monotonic Versioning v1.2][1]. Any valid Monotonic Version can be parsed
with a Semantic Version parser, and thus this namespace works with instances of the
[`SemanticVersion`][2] class.

[1]: http://web.archive.org/web/20160523103913/http://blog.appliedcompscilab.com/monotonic_versioning_manifesto/
[2]: ../SemanticVersion

## Classes

- **[MonotonicComparer][3]**  
  Represents a semantic version comparison operation using monotonic
  versioning comparison rules.
- **[MonotonicExtensions][4]**  
  Provides extension methods related to treating [SemanticVersion][2]s
  as monotonic versions.
- **[MonotonicVersioner][5]**  
  Provides a method of working with [SemanticVersion][2]s as monotonic
  versions.

[3]: ./MonotonicComparer
[4]: ./MonotonicExtensions
[5]: ./MonotonicVersioner

## Enumerations

- **[MonotonicChange][6]**  
  The types of change which may be made to a monotonic-versioned
  software package.

[6]: ./MonotonicChange.md