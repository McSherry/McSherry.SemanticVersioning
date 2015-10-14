# `SemanticVersion.Parse(String)` method

```c#
public static SemanticVersion Parse(
    string version
)
```

**Namespace:** [`McSherry.SemanticVersioning`][1]  
**Minimum version:** 1.0.0

[1]: ../

Converts a version string to a [SemanticVersion][2], only
accepting the format given in the Semantic Versioning
specification.

[2]: ./


### Parameters

- **`version`**  
  **Type:** `System.String`  
  The version string to be converted to a [SemanticVersion][2].
  
  
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
  
  
## Remarks

This method is equivalent to calling [Parse(String, ParseMode)][3] and
passing the value [`ParseMode.Strict`][4].

[3]: ./Parse(String,ParseMode).md
[4]: ../ParseMode.md