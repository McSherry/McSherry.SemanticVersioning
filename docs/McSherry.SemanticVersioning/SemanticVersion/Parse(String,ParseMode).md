# `SemanticVersion.Parse(String, ParseMode)` method

```c#
public static SemanticVersion Parse(
    string version,
    ParseMode mode
)
```

**Namespace:** [`McSherry.SemanticVersioning`][1]  
**Minimum version:** 1.0.0

[1]: ../

Converts a version string to a [SemanticVersion][2], taking
into account a set of flags.

[2]: ./


### Parameters

- **`version`**  
  **Type:** `System.String`  
  The version string to be converted to a [SemanticVersion][2].
- **`mode`**  
  **Type:** [`McSherry.SemanticVersioning.ParseMode`][3]  
  A set of flags to augment how the version string is parsed.
  
[3]: ../ParseMode.md


### Return Value

**Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]

A [SemanticVersion][2] equivalent to the provided version string.


## Exceptions

- **`System.ArgumentNullException`**  
  Thrown when _`version`_ is null or empty.
- **`System.ArgumentException`**  
  Thrown when a component in the version string was expected but
  not found (for example, a missing minor or patch version).
- **`System.FormatException`**  
  Thrown when an invalid character or character sequence is
  encountered.
- **`System.OverflowException`**  
  Thrown when an attempt to convert the major, minor, or patch
  version into a `System.Int32` resulted in an overflow.