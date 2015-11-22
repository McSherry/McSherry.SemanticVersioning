# `VersionRange.Parse` method

```c#
public static VersionRange Parse(
    string range
    )
```

**Namespace:** [`McSherry.SemanticVersioning.Ranges`][1]  
**Minimum version:** 1.1.0

[1]: ../

Parses a version range from a string.


### Parameters

- **`range`**  
  **Type:** `System.String`  
  The string representing the version range.
  
  
### Return Value

**Type:** [`McSherry.SemanticVersioning.Ranges.VersionRange`][2]

A [VersionRange][2] equivalent to the value of _`range`_.

[2]: ./


## Exceptions

- **`System.ArgumentNullException`**  
  Thrown when _`ranges`_ is null, empty, or contains only
  whitespace characters.
- **`System.ArgumentException`**  
  Thrown when _`range`_ is invalid for any reason unrelated to
  an invalid semantic version string.
- **`System.FormatException`**  
  Thrown when _`range`_ contains an invalid semantic version
  string.